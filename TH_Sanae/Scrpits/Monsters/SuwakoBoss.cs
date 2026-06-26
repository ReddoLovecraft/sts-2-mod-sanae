using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
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

	protected override string VisualsPath => "res://TH_Sanae/ArtWorks/Character/suwako.tscn";

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		string idle = PickAnim(controller, "Idle", "idle_loop", "idle");
		string defend = PickAnim(controller, "Defend", "cast", "Cast");
		string atk = PickAnim(controller, "Atk", "attack", "Attack");
		string atk2 = PickAnim(controller, "Atk2", "attack2", "Attack2");
		string hit = PickAnim(controller, "OnHit", "hurt", "Hit");
		string dead = PickAnim(controller, "die", "Dead");

		AnimState idleState = new AnimState(idle, isLooping: true);
		AnimState defendState = new AnimState(defend) { NextState = idleState };
		AnimState atkState = new AnimState(atk) { NextState = idleState };
		AnimState atk2State = new AnimState(atk2) { NextState = idleState };
		AnimState hitState = new AnimState(hit) { NextState = idleState };
		AnimState deadState = new AnimState(dead);

		CreatureAnimator animator = new CreatureAnimator(idleState, controller);
		animator.AddAnyState("Idle", idleState);
		animator.AddAnyState("Defend", defendState);
		animator.AddAnyState("Atk", atkState);
		animator.AddAnyState("Attack", atkState);
		animator.AddAnyState("Atk2", atk2State);
		animator.AddAnyState("Hit", hitState);
		animator.AddAnyState("OnHit", hitState);
		animator.AddAnyState("Dead", deadState);
		return animator;
	}

	private static string PickAnim(MegaSprite controller, params string[] candidates)
	{
		foreach (string candidate in candidates)
		{
			if (controller.HasAnimation(candidate))
			{
				return candidate;
			}
		}

		return candidates[0];
	}

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
		MoveState calamity = new("CALAMITY", CalamityMove, new CardDebuffIntent());
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

		chooser.AddState(emergencyBlessing, () => !HasOnceArmour() && !_lastMoveWasEmergencyBlessing && !WouldRepeatIntent(emergencyBlessing.Id));
		chooser.AddState(divineIronRing, () => !HasOnceArmour() && !HasIntangible() && !WouldRepeatIntent(divineIronRing.Id));
		chooser.AddState(voidGuard, () => !HasOnceArmour() && HasIntangible() && !WouldRepeatIntent(voidGuard.Id));
		chooser.AddState(omikujiBranch, HasOnceArmour);

		omikujiBranch.AddBranch(divineIronRing, MoveRepeatType.CanRepeatForever, GetOmikujiWeight(10f, divineIronRing.Id));
		omikujiBranch.AddBranch(curseWheel, MoveRepeatType.CanRepeatForever, GetOmikujiWeight(10f, curseWheel.Id));
		omikujiBranch.AddBranch(voidGuard, MoveRepeatType.CanRepeatForever, GetOmikujiWeight(12f, voidGuard.Id));
		omikujiBranch.AddBranch(recover, MoveRepeatType.CanRepeatForever, GetOmikujiWeight(21f, recover.Id));
		omikujiBranch.AddBranch(tripleSlash, MoveRepeatType.CanRepeatForever, GetOmikujiWeight(35f, tripleSlash.Id, forceWhenAlwaysGoodLuck: true));

		states.Add(chooser);
		states.Add(omikujiBranch);
		return new MonsterMoveStateMachine(states, calamity);
	}

	private Creature? GetPlayerCreature()
	{
		return CombatState?.Players.FirstOrDefault()?.Creature;
	}

	private bool HasOnceArmour() => Creature?.HasPower<OnceArmourPower>() == true;

	private bool HasIntangible() => Creature?.HasPower<IntangiblePower>() == true;

	private Func<float> GetOmikujiWeight(float baseWeight, string moveId, bool forceWhenAlwaysGoodLuck = false)
	{
		return () =>
		{
			if (WouldRepeatIntent(moveId))
			{
				return 0f;
			}

			Creature? playerCreature = GetPlayerCreature();
			if (playerCreature != null && ToolBox.HasAlwaysGoodLuck(playerCreature))
			{
				return forceWhenAlwaysGoodLuck ? 1f : 0f;
			}

			return baseWeight;
		};
	}

	private bool WouldRepeatIntent(string nextMoveId)
	{
		MonsterMoveStateMachine? machine = Creature?.Monster?.MoveStateMachine;
		if (machine == null)
		{
			return false;
		}

		MonsterState? lastMove = machine.StateLog.LastOrDefault(state => state.IsMove);
		if (lastMove == null)
		{
			return false;
		}

		return GetPrimaryIntentType(lastMove.Id) == GetPrimaryIntentType(nextMoveId);
	}

	private static IntentType GetPrimaryIntentType(string moveId)
	{
		return moveId switch
		{
			"DIVINE_IRON_RING" => IntentType.Attack,
			"CURSE_WHEEL" => IntentType.Attack,
			"TRIPLE_SLASH" => IntentType.Attack,
			"RECOVER" => IntentType.Defend,
			"VOID_GUARD" => IntentType.Defend,
			"EMERGENCY_BLESSING" => IntentType.Buff,
			"CALAMITY" => IntentType.CardDebuff,
			_ => IntentType.Unknown
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
		await CreatureCmd.TriggerAnim(Creature, "Atk", 0.35f);
		await DamageCmd.Attack(HeavyDamage)
			.FromMonster(this)
			.WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3")
			.Execute(null);
		await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);
	}

	private async Task CurseWheelMove(IReadOnlyList<Creature> targets)
	{
		MarkEmergencyBlessing(false);
		if (Creature != null)
		{
			await CreatureCmd.TriggerAnim(Creature, "Atk", 0.35f);
		}
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
		if (Creature != null)
		{
			await CreatureCmd.TriggerAnim(Creature, "Atk2", 0.35f);
		}
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
		await CreatureCmd.TriggerAnim(Creature, "Defend", 0.25f);
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
		await CreatureCmd.TriggerAnim(Creature, "Defend", 0.25f);
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
		if (Creature != null)
		{
			await CreatureCmd.TriggerAnim(Creature, "Atk2", 0.35f);
		}
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
		await CreatureCmd.TriggerAnim(Creature, "Atk2", 0.35f);
		await PowerCmd.Apply<OnceArmourPower>(new ThrowingPlayerChoiceContext(), Creature, EmergencyArmourAmount, Creature, null);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, EmergencyStrengthAmount, Creature, null);
	}
}
