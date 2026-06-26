using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.ValueProps;

namespace TH_Sanae.Scripts.Monsters;

public sealed class OnbashiraMinion : CustomMonsterModel
{
	private int InitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 88, 66);
	private int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 4);
	private int StrengthGain => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 2);

	private int _attackHits = 1;

	public override LocString Title => MonsterModel.L10NMonsterLookup(GetType().Name + ".name");

	protected override string VisualsPath => "res://TH_Sanae/ArtWorks/Character/onbashira.tscn";

	public override int MinInitialHp => InitialHp;

	public override int MaxInitialHp => InitialHp;

	public override async Task BeforeCombatStart()
	{
		if (Creature != null)
		{
			await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
		}
	}

	protected override MonsterMoveStateMachine GenerateMoveStateMachine()
	{
		MoveState attack = new("ATTACK", AttackMove, new MultiAttackIntent(AttackDamage, () => _attackHits));
		MoveState support = new("SUPPORT", SupportMove, new BuffIntent(), new DefendIntent());
		attack.FollowUpState = support;
		support.FollowUpState = attack;
		return new MonsterMoveStateMachine([attack, support], attack);
	}

	private Creature? GetKanako()
	{
		return Creature?.CombatState?.Enemies.FirstOrDefault(enemy => enemy.Monster is KanakoBoss && enemy.IsAlive);
	}

	private async Task AttackMove(IReadOnlyList<Creature> targets)
	{
		await BumpAttack();
		await DamageCmd.Attack(AttackDamage)
			.FromMonster(this)
			.WithHitCount(_attackHits)
			.WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3")
			.Execute(null);

		Creature? kanako = GetKanako();
		if (kanako != null)
		{
			await CreatureCmd.GainBlock(kanako, 8m, ValueProp.Unpowered | ValueProp.Move, null);
		}

		_attackHits++;
	}

	private async Task BumpAttack()
	{
		if (Creature == null)
		{
			return;
		}

		NCreature? node = Creature.GetCreatureNode();
		if (node == null)
		{
			return;
		}

		Node2D body = node.Body;
		Vector2 start = body.Position;
		Vector2 dir = Creature.Side == CombatSide.Enemy ? Vector2.Left : Vector2.Right;
		Vector2 target = start + dir * 120f;

		Tween tweenOut = node.CreateTween();
		tweenOut.TweenProperty(body, "position", target, 0.12).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		await tweenOut.AwaitFinished(node);

		Tween tweenBack = node.CreateTween();
		tweenBack.TweenProperty(body, "position", start, 0.18).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
		await tweenBack.AwaitFinished(node);
	}

	private async Task SupportMove(IReadOnlyList<Creature> targets)
	{
		if (Creature == null)
		{
			return;
		}

		await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthGain, Creature, null);
		await CreatureCmd.GainBlock(Creature, 8m, ValueProp.Unpowered | ValueProp.Move, null);

		Creature? kanako = GetKanako();
		if (kanako != null)
		{
			await CreatureCmd.GainBlock(kanako, 8m, ValueProp.Unpowered | ValueProp.Move, null);
		}

		_attackHits++;
	}
}
