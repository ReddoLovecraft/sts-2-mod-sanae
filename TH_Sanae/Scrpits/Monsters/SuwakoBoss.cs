using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Monsters;

public sealed class SuwakoBoss : CustomMonsterModel
{
	private const int CurseWheelHits = 3;
	private const int CurseWheelStatusCount = 3;
	private const int VoidGuardVoidCount = 3;
	private int InitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 600, 500);
	private int MultiDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 10);
	private int HeavyDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 56, 40);
	private int EmergencyArmourAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 100, 80);
	private int EmergencyStrengthAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 3);

	private bool _lastMoveWasEmergencyBlessing;

	public override LocString Title => MonsterModel.L10NMonsterLookup(GetType().Name + ".name");

	protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/defect");

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override async Task BeforeCombatStart()
	{
		if (Creature == null)
		{
			return;
		}

		await PowerCmd.Apply<FaithCaughtPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
		await PowerCmd.Apply<OnceArmourPower>(new ThrowingPlayerChoiceContext(), Creature, 50m, Creature, null);
		await PowerCmd.Apply<OnceThornPower>(new ThrowingPlayerChoiceContext(), Creature, 5m, Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> states = [];
		MoveState divineIronRing = new("DIVINE_IRON_RING", DivineIronRingMove, new SingleAttackIntent(HeavyDamage), new BuffIntent());
		MoveState curseWheel = new("CURSE_WHEEL", CurseWheelMove, new MultiAttackIntent(MultiDamage, CurseWheelHits), new DebuffIntent(), new StatusIntent(CurseWheelStatusCount));
		MoveState calamity = new("CALAMITY", CalamityMove, new DebuffIntent());
		MoveState recover = new("RECOVER", RecoverMove, new DefendIntent(), new BuffIntent());
		MoveState voidGuard = new("VOID_GUARD", VoidGuardMove, new DefendIntent(), new StatusIntent(VoidGuardVoidCount));
		MoveState tripleSlash = new("TRIPLE_SLASH", TripleSlashMove, new MultiAttackIntent(MultiDamage, CurseWheelHits));
		MoveState emergencyBlessing = new("EMERGENCY_BLESSING", EmergencyBlessingMove, new BuffIntent());
		ConditionalBranchState chooser = new("SUWAKO_CHOOSE_MOVE");
		RandomBranchState omikujiBranch = new("SUWAKO_OMIKUJI_BRANCH");

		foreach (MoveState state in new[] { divineIronRing, curseWheel, calamity, recover, voidGuard, tripleSlash, emergencyBlessing })
		{
			state.FollowUpState = chooser;
			states.Add(state);
		}

		chooser.AddState(emergencyBlessing, () => !HasOnceArmour() && !_lastMoveWasEmergencyBlessing);
		chooser.AddState(divineIronRing, () => !HasOnceArmour() && !HasIntangible());
		chooser.AddState(voidGuard, () => !HasOnceArmour() && HasIntangible());
		chooser.AddState(omikujiBranch, HasOnceArmour);

		omikujiBranch.AddBranch(divineIronRing, MoveRepeatType.CanRepeatForever, GetOmikujiWeight(10f));
		omikujiBranch.AddBranch(curseWheel, MoveRepeatType.CanRepeatForever, GetOmikujiWeight(10f));
		omikujiBranch.AddBranch(calamity, MoveRepeatType.CanRepeatForever, GetOmikujiWeight(12f));
		omikujiBranch.AddBranch(voidGuard, MoveRepeatType.CanRepeatForever, GetOmikujiWeight(12f));
		omikujiBranch.AddBranch(recover, MoveRepeatType.CanRepeatForever, GetOmikujiWeight(21f));
		omikujiBranch.AddBranch(tripleSlash, MoveRepeatType.CanRepeatForever, GetOmikujiWeight(35f, forceWhenAlwaysGoodLuck: true));

		states.Add(chooser);
		states.Add(omikujiBranch);
		return new MonsterMoveStateMachine(states, chooser);
	}

	private Creature? GetPlayerCreature()
	{
		return CombatState?.Players.FirstOrDefault()?.Creature;
	}

	private bool HasOnceArmour() => Creature?.HasPower<OnceArmourPower>() == true;

	private bool HasIntangible() => Creature?.HasPower<IntangiblePower>() == true;

	private Func<float> GetOmikujiWeight(float baseWeight, bool forceWhenAlwaysGoodLuck = false)
	{
		return () =>
		{
			Creature? playerCreature = GetPlayerCreature();
			if (playerCreature != null && ToolBox.HasAlwaysGoodLuck(playerCreature))
			{
				return forceWhenAlwaysGoodLuck ? 1f : 0f;
			}

			return baseWeight;
		};
	}

	private void MarkEmergencyBlessing(bool value)
	{
		_lastMoveWasEmergencyBlessing = value;
	}

	private async Task DivineIronRingMove(IReadOnlyList<Creature> targets)
	{
		if (Creature == null)
		{
			return;
		}

		MarkEmergencyBlessing(false);
		await DamageCmd.Attack(HeavyDamage)
			.FromMonster(this)
			.WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3")
			.Execute(null);
		await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);
	}

	private async Task CurseWheelMove(IReadOnlyList<Creature> targets)
	{
		MarkEmergencyBlessing(false);
		IReadOnlyList<Creature> livingTargets = targets.Where(target => target.IsAlive).ToList();
		if (livingTargets.Count == 0)
		{
			return;
		}

		await DamageCmd.Attack(MultiDamage)
			.FromMonster(this)
			.WithHitCount(CurseWheelHits)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);

		await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), livingTargets, 3m, Creature, null);
		await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), livingTargets, 3m, Creature, null);
		await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), livingTargets, 3m, Creature, null);
		await CardPileCmd.AddToCombatAndPreview<Wound>(livingTargets, PileType.Draw, 2, null);
		await CardPileCmd.AddToCombatAndPreview<Dazed>(livingTargets, PileType.Draw, 1, null);
	}

	private async Task CalamityMove(IReadOnlyList<Creature> targets)
	{
		MarkEmergencyBlessing(false);
		IReadOnlyList<Creature> livingTargets = targets.Where(target => target.IsAlive).ToList();
		if (livingTargets.Count == 0)
		{
			return;
		}

		foreach (Creature target in livingTargets)
		{
			if (target.GetPower<HexPower>() == null)
			{
				await PowerCmd.Apply<HexPower>(new ThrowingPlayerChoiceContext(), target, 2m, Creature, null);
			}

			DampenPower? dampenPower = target.GetPower<DampenPower>();
			bool shouldApplyDampen = dampenPower == null;
			if (shouldApplyDampen)
			{
				dampenPower = (DampenPower)ModelDb.Power<DampenPower>().ToMutable();
			}

			dampenPower!.AddCaster(Creature);
			if (shouldApplyDampen)
			{
				await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), dampenPower, target, 1m, Creature, null);
			}
		}
	}

	private async Task RecoverMove(IReadOnlyList<Creature> targets)
	{
		if (Creature == null)
		{
			return;
		}

		MarkEmergencyBlessing(false);
		await CreatureCmd.GainBlock(Creature, 80m, ValueProp.Unpowered | ValueProp.Move, null);
		await PowerCmd.Apply<OnceArmourPower>(new ThrowingPlayerChoiceContext(), Creature, 50m, Creature, null);
		await PowerCmd.Apply<OnceThornPower>(new ThrowingPlayerChoiceContext(), Creature, 3m, Creature, null);
	}

	private async Task VoidGuardMove(IReadOnlyList<Creature> targets)
	{
		if (Creature == null)
		{
			return;
		}

		MarkEmergencyBlessing(false);
		await CreatureCmd.GainBlock(Creature, 80m, ValueProp.Unpowered | ValueProp.Move, null);
		IReadOnlyList<Creature> livingTargets = targets.Where(target => target.IsAlive).ToList();
		if (livingTargets.Count > 0)
		{
			await CardPileCmd.AddToCombatAndPreview<MegaCrit.Sts2.Core.Models.Cards.Void>(livingTargets, PileType.Draw, VoidGuardVoidCount, null);
		}
	}

	private async Task TripleSlashMove(IReadOnlyList<Creature> targets)
	{
		MarkEmergencyBlessing(false);
		await DamageCmd.Attack(MultiDamage)
			.FromMonster(this)
			.WithHitCount(CurseWheelHits)
			.WithHitFx("vfx/vfx_attack_slash")
			.Execute(null);
	}

	private async Task EmergencyBlessingMove(IReadOnlyList<Creature> targets)
	{
		if (Creature == null)
		{
			return;
		}

		MarkEmergencyBlessing(true);
		await PowerCmd.Apply<OnceArmourPower>(new ThrowingPlayerChoiceContext(), Creature, EmergencyArmourAmount, Creature, null);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, EmergencyStrengthAmount, Creature, null);
	}
}
