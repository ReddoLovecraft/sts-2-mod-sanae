using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
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
	private int MultiDamage => 8;
	private int DivineRainHits => int.Max(1, AliveOnbashiraCount);
	private int _cycleIndex = 1;
	private int _turnsSinceSummon;
	private bool _lastMoveWasSummon;
	private Vector2[]? _onbashiraBaseSlotPositions;
	private readonly Dictionary<uint, int> _onbashiraSlotAssignments = new();
	private readonly Vector2 _onbashiraSecondRowOffset = new(0f, -300f);

	public override LocString Title => MonsterModel.L10NMonsterLookup(GetType().Name + ".name");

	protected override string VisualsPath => "res://TH_Sanae/ArtWorks/Character/kanako.tscn";

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override CreatureAnimator GenerateAnimator(MegaSprite controller)
	{
		string idle = PickAnim(controller, "Idle", "idle_loop", "idle");
		string summon = PickAnim(controller, "Summon", "Cast", "cast");
		string atk = PickAnim(controller, "Atk", "attack", "Attack");
		string hit = PickAnim(controller, "OnHit", "hurt", "Hit");
		string dead = PickAnim(controller, "die", "Dead");

		AnimState idleState = new AnimState(idle, isLooping: true);
		AnimState summonState = new AnimState(summon) { NextState = idleState };
		AnimState atkState = new AnimState(atk) { NextState = idleState };
		AnimState hitState = new AnimState(hit) { NextState = idleState };
		AnimState deadState = new AnimState(dead);

		CreatureAnimator animator = new CreatureAnimator(idleState, controller);
		animator.AddAnyState("Idle", idleState);
		animator.AddAnyState("Summon", summonState);
		animator.AddAnyState("Cast", summonState);
		animator.AddAnyState("Atk", atkState);
		animator.AddAnyState("Attack", atkState);
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

		_turnsSinceSummon = 0;
		_lastMoveWasSummon = false;

		await PowerCmd.Apply<ArmyGodFormPower>(new ThrowingPlayerChoiceContext(), Creature, 10m, Creature, null);
		await PowerCmd.Apply<FaithCaughtPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
		await PowerCmd.Apply<BarricadePower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
	}

	public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		TryInitializeOnbashiraSlotsFromEncounter();
		return Task.CompletedTask;
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		List<MonsterState> states = [];
		MoveState summon = new("SUMMON_ONBASHIRA", SummonOnbashiraMove, new SummonIntent());
		MoveState rain = new("DIVINE_RAIN", DivineRainMove, new MultiAttackIntent(MultiDamage, () => DivineRainHits));
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

	private int RemainingOnbashiraCapacity => int.Max(0, MaxOnbashiraCount - AliveOnbashiraCount);

	private bool ShouldUseSummon()
	{
		if (_lastMoveWasSummon || RemainingOnbashiraCapacity <= 0)
		{
			return false;
		}

		if (AliveOnbashiraCount < 4)
		{
			return true;
		}

		return AliveOnbashiraCount < MaxOnbashiraCount && _turnsSinceSummon >= SummonCooldownTurns;
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
		TryInitializeOnbashiraSlotsFromEncounter();

		int summonCount = int.Min(SummonBatchSize, RemainingOnbashiraCapacity);
		for (int i = 0; i < summonCount; i++)
		{
			Creature onbashira = await CreatureCmd.Add(ModelDb.Monster<OnbashiraMinion>().ToMutable(), CombatState, Creature.Side, null);
			await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), onbashira, 1m, Creature, null);
			AssignOnbashiraToNextFreeSlot(onbashira);
			RepositionOnbashiras();
		}
	}

	private void RepositionOnbashiras()
	{
		if (CombatState == null || _onbashiraBaseSlotPositions == null)
		{
			return;
		}

		CleanupDeadOnbashiraSlotAssignments();
		HashSet<int> occupied = _onbashiraSlotAssignments.Values.ToHashSet();
		foreach (Creature enemy in CombatState.Enemies.Where(c => c.IsAlive && c.Monster is OnbashiraMinion))
		{
			if (!enemy.CombatId.HasValue)
			{
				continue;
			}

			if (!_onbashiraSlotAssignments.TryGetValue(enemy.CombatId.Value, out int assignedSlot))
			{
				int? slotIndex = FindFirstFreeSlotIndex(occupied);
				if (slotIndex == null)
				{
					continue;
				}

				assignedSlot = slotIndex.Value;
				_onbashiraSlotAssignments[enemy.CombatId.Value] = assignedSlot;
				occupied.Add(assignedSlot);
			}

			Vector2 slotPos = GetOnbashiraSlotPosition(assignedSlot);
			var node = enemy.GetCreatureNode();
			if (node != null)
			{
				node.GlobalPosition = slotPos;
			}
		}
	}

	private void AssignOnbashiraToNextFreeSlot(Creature onbashira)
	{
		if (CombatState == null || _onbashiraBaseSlotPositions == null || !onbashira.CombatId.HasValue)
		{
			return;
		}

		CleanupDeadOnbashiraSlotAssignments();
		HashSet<int> occupied = _onbashiraSlotAssignments.Values.ToHashSet();
		int? slotIndex = FindFirstFreeSlotIndex(occupied);
		if (slotIndex == null)
		{
			return;
		}

		_onbashiraSlotAssignments[onbashira.CombatId.Value] = slotIndex.Value;
		var node = onbashira.GetCreatureNode();
		if (node != null)
		{
			node.GlobalPosition = GetOnbashiraSlotPosition(slotIndex.Value);
		}
	}

	private int? FindFirstFreeSlotIndex(HashSet<int> occupied)
	{
		for (int i = 0; i < MaxOnbashiraCount; i++)
		{
			if (!occupied.Contains(i))
			{
				return i;
			}
		}

		return null;
	}

	private Vector2 GetOnbashiraSlotPosition(int slotIndex)
	{
		if (_onbashiraBaseSlotPositions == null)
		{
			return Vector2.Zero;
		}

		if (slotIndex < 4)
		{
			return _onbashiraBaseSlotPositions[slotIndex];
		}

		return _onbashiraBaseSlotPositions[slotIndex - 4] + _onbashiraSecondRowOffset;
	}

	private void CleanupDeadOnbashiraSlotAssignments()
	{
		if (CombatState == null)
		{
			return;
		}

		HashSet<uint> aliveIds = CombatState.Enemies
			.Where(c => c.IsAlive && c.Monster is OnbashiraMinion && c.CombatId.HasValue)
			.Select(c => c.CombatId!.Value)
			.ToHashSet();

		foreach (uint id in _onbashiraSlotAssignments.Keys.ToList())
		{
			if (!aliveIds.Contains(id))
			{
				_onbashiraSlotAssignments.Remove(id);
			}
		}
	}

	private void TryInitializeOnbashiraSlotsFromEncounter()
	{
		if (CombatState == null || _onbashiraBaseSlotPositions != null)
		{
			return;
		}

		var seeded = CombatState.Enemies
			.Where(enemy => enemy.IsAlive && enemy.Monster is OnbashiraMinion && enemy.CombatId.HasValue)
			.Select(enemy => (enemy, node: enemy.GetCreatureNode()))
			.Where(x => x.node != null)
			.Select(x => (x.enemy, pos: x.node!.GlobalPosition))
			.OrderBy(x => x.pos.X)
			.Take(4)
			.ToArray();

		if (seeded.Length != 4)
		{
			return;
		}

		Vector2[] slots = seeded.Select(x => x.pos).ToArray();
		float span = slots.Max(p => p.X) - slots.Min(p => p.X);
		if (span < 1f)
		{
			return;
		}

		_onbashiraBaseSlotPositions = slots;
		for (int i = 0; i < seeded.Length; i++)
		{
			uint id = seeded[i].enemy.CombatId!.Value;
			_onbashiraSlotAssignments[id] = i;
		}
	}


	private async Task DivineRainMove(IReadOnlyList<Creature> targets)
	{
		if (Creature != null)
		{
			await CreatureCmd.TriggerAnim(Creature, "Atk", 0.35f);
		}

		MarkNonSummonTurn(advanceCycle: true);
		await DamageCmd.Attack(MultiDamage)
			.FromMonster(this)
			.WithHitCount(DivineRainHits)
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
		await CreatureCmd.TriggerAnim(Creature, "Summon", 0.35f);
		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), LivingEnemies.ToList(), TeamStrengthGain, Creature, null);
	}

	private async Task SanctuaryMove(IReadOnlyList<Creature> targets)
	{
		if (Creature == null)
		{
			return;
		}

		MarkNonSummonTurn(advanceCycle: true);
		await CreatureCmd.TriggerAnim(Creature, "Summon", 0.35f);
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
		await CreatureCmd.TriggerAnim(Creature, "Summon", 0.35f);
		await PowerCmd.Apply<ArmyGodFormPower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);
		await CreatureCmd.GainBlock(Creature, 33m, ValueProp.Unpowered | ValueProp.Move, null);
		foreach (Creature teammate in LivingTeammates)
		{
			await CreatureCmd.GainBlock(teammate, 33m, ValueProp.Unpowered | ValueProp.Move, null);
		}
	}
}
