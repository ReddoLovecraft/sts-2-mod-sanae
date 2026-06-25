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
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Encounters;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Monsters;

public sealed class KanakoBoss : CustomMonsterModel
{
	private const int SummonBatchSize = 4;
	private const int MaxOnbashiraCount = 8;
	private const int SummonCooldownTurns = 2;
	private const int PunishStatusCount = 4;
	private const int TeamStrengthGain = 8;

	private int InitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 888, 800);
	private int MultiDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 6);
	private int MultiHits => 8;
	private int _cycleIndex = 1;
	private int _turnsSinceSummon;
	private bool _lastMoveWasSummon;

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

		_turnsSinceSummon = 0;
		_lastMoveWasSummon = false;

		await PowerCmd.Apply<ArmyGodFormPower>(new ThrowingPlayerChoiceContext(), Creature, 4m, Creature, null);
		await PowerCmd.Apply<FaithCaughtPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
		await PowerCmd.Apply<BarricadePower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> states = [];
		MoveState summon = new("SUMMON_ONBASHIRA", SummonOnbashiraMove, new SummonIntent());
		MoveState rain = new("DIVINE_RAIN", DivineRainMove, new MultiAttackIntent(MultiDamage, MultiHits));
		MoveState punish = new("DIVINE_PUNISH", DivinePunishMove, new DebuffIntent(), new StatusIntent(PunishStatusCount));
		MoveState faith = new("FAITH_BLESSING", FaithBlessingMove, new BuffIntent());
		MoveState sanctuary = new("SANCTUARY", SanctuaryMove, new DefendIntent(), new BuffIntent());
		MoveState forcedSanctuary = new("SANCTUARY_FORCED", ForcedSanctuaryMove, new DefendIntent(), new BuffIntent());
		ConditionalBranchState chooser = new("KANAKO_CHOOSE_MOVE");

		foreach (MoveState state in new[] { summon, rain, punish, faith, sanctuary, forcedSanctuary })
		{
			state.FollowUpState = chooser;
			states.Add(state);
		}

		chooser.AddState(summon, ShouldUseSummon);
		chooser.AddState(forcedSanctuary, IsBelowHalfHealth);
		chooser.AddState(rain, () => _cycleIndex == 1);
		chooser.AddState(punish, () => _cycleIndex == 2);
		chooser.AddState(faith, () => _cycleIndex == 3);
		chooser.AddState(sanctuary, () => _cycleIndex == 4);
		states.Add(chooser);

		return new MonsterMoveStateMachine(states, chooser);
	}

	private IEnumerable<Creature> LivingEnemies =>
		Creature?.CombatState?.Enemies.Where(enemy => enemy.IsAlive) ?? [];

	private IEnumerable<Creature> LivingTeammates =>
		Creature?.CombatState?.GetTeammatesOf(Creature).Where(teammate => teammate.IsAlive) ?? [];

	private IEnumerable<Creature> LivingOnbashiras =>
		LivingTeammates.Where(teammate => teammate.Monster is OnbashiraMinion);

	private int AliveOnbashiraCount => LivingOnbashiras.Count();

	private bool IsBelowHalfHealth() =>
		Creature != null && Creature.CurrentHp < Creature.MaxHp / 2m;

	private bool HasFreeOnbashiraSlot() => GetAvailableOnbashiraSlots().Count > 0;

	private bool ShouldUseSummon()
	{
		if (_lastMoveWasSummon || !HasFreeOnbashiraSlot())
		{
			return false;
		}

		if (AliveOnbashiraCount < 4)
		{
			return true;
		}

		return AliveOnbashiraCount < MaxOnbashiraCount && _turnsSinceSummon >= SummonCooldownTurns;
	}

	private List<string> GetAvailableOnbashiraSlots()
	{
		HashSet<string> occupiedSlots = Creature?.CombatState?.Enemies
			.Where(enemy => enemy.IsAlive && enemy.SlotName != null)
			.Select(enemy => enemy.SlotName!)
			.ToHashSet() ?? [];

		return KanakoAndOnbashiraEventEncounter.OnbashiraSlots
			.Where(slot => !occupiedSlots.Contains(slot))
			.ToList();
	}

	private void AdvanceCycle()
	{
		_cycleIndex++;
		if (_cycleIndex > 4)
		{
			_cycleIndex = 1;
		}
	}

	private void MarkNonSummonTurn(bool advanceCycle)
	{
		_lastMoveWasSummon = false;
		_turnsSinceSummon++;
		if (advanceCycle)
		{
			AdvanceCycle();
		}
	}

	private async Task SummonOnbashiraMove(IReadOnlyList<Creature> targets)
	{
		if (Creature == null)
		{
			return;
		}

		_lastMoveWasSummon = true;
		_turnsSinceSummon = 0;
		await CreatureCmd.TriggerAnim(Creature, "Summon", 0.35f);

		foreach (string slot in GetAvailableOnbashiraSlots().Take(SummonBatchSize))
		{
			Creature onbashira = await CreatureCmd.Add<OnbashiraMinion>(CombatState, slot);
			await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), onbashira, 1m, Creature, null);
		}
	}


	private async Task DivineRainMove(IReadOnlyList<Creature> targets)
	{
		MarkNonSummonTurn(advanceCycle: true);
		await DamageCmd.Attack(MultiDamage)
			.FromMonster(this)
			.WithHitCount(MultiHits)
			.Execute(null);
	}

	private async Task DivinePunishMove(IReadOnlyList<Creature> targets)
	{
		MarkNonSummonTurn(advanceCycle: true);
		IReadOnlyList<Creature> livingTargets = targets.Where(target => target.IsAlive).ToList();
		if (livingTargets.Count == 0)
		{
			return;
		}

		await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), livingTargets, 4m, Creature, null);
		await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), livingTargets, 4m, Creature, null);
		await CardPileCmd.AddToCombatAndPreview<Dazed>(livingTargets, PileType.Discard, PunishStatusCount, null);
	}

	private async Task FaithBlessingMove(IReadOnlyList<Creature> targets)
	{
		if (Creature == null)
		{
			return;
		}

		MarkNonSummonTurn(advanceCycle: true);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), LivingEnemies.ToList(), TeamStrengthGain, Creature, null);
	}

	private async Task SanctuaryMove(IReadOnlyList<Creature> targets)
	{
		if (Creature == null)
		{
			return;
		}

		MarkNonSummonTurn(advanceCycle: true);
		await PowerCmd.Apply<ArmyGodFormPower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);
		await CreatureCmd.GainBlock(Creature, 33m, ValueProp.Unpowered | ValueProp.Move, null);
		foreach (Creature teammate in LivingTeammates)
		{
			await CreatureCmd.GainBlock(teammate, 33m, ValueProp.Unpowered | ValueProp.Move, null);
		}
	}

	private async Task ForcedSanctuaryMove(IReadOnlyList<Creature> targets)
	{
		if (Creature == null)
		{
			return;
		}

		MarkNonSummonTurn(advanceCycle: false);
		await PowerCmd.Apply<ArmyGodFormPower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);
		await CreatureCmd.GainBlock(Creature, 33m, ValueProp.Unpowered | ValueProp.Move, null);
		foreach (Creature teammate in LivingTeammates)
		{
			await CreatureCmd.GainBlock(teammate, 33m, ValueProp.Unpowered | ValueProp.Move, null);
		}
	}
}
