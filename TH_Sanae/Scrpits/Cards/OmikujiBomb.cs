using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Patches;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class OmikujiBomb : SanaeCardModel, IResolvedDrawResultCard
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardModifier.DrawKeyword];
		DrawResultType? IResolvedDrawResultCard.ResolvedDrawResult { get; set; }

		protected override bool ShouldGlowGoldInternal => CardKeywordAfterDrawPatch.GetResolvedDrawResult(this) == DrawResultType.GreatLuck;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>()];

		public OmikujiBomb() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			DrawResultType drawResult = CardKeywordAfterDrawPatch.GetResolvedDrawResult(this);
			string animName = Owner.Character is SanaeCharacter ? "Throw" : "Cast";
			if (NCombatRoom.Instance != null)
			{
				NCreature? attackerNode = NCombatRoom.Instance.GetCreatureNode(Owner.Creature);
				if (attackerNode != null)
				{
					List<Creature> targets = [Owner.Creature];
					targets.AddRange(CombatState.Players
						.Where(player => player != Owner && player.Creature.IsAlive)
						.Select(player => player.Creature));
					targets.AddRange(CombatState.HittableEnemies.ToList());
					foreach (Creature target in targets)
					{
						NCreature? targetNode = NCombatRoom.Instance.GetCreatureNode(target);
						if (targetNode == null)
						{
							continue;
						}

						Vector2 startPos = attackerNode.VfxSpawnPosition;
						Vector2 endPos = targetNode.GetTopOfHitbox() + Vector2.Up * 18f;
						var vfx = NOmikujiBulletVfx.Create(startPos, endPos, drawResult);
						if (vfx != null)
						{
							NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
						}
					}
				}
			}

			await CreatureCmd.TriggerAnim(base.Owner.Creature, animName, base.Owner.Character.CastAnimDelay);
			await Cmd.Wait(0.35f);
			if (IsUpgraded && Owner.Creature.HasPower<BeliefPower>())
			{
				await CreatureCmd.GainBlock(Owner.Creature, Owner.Creature.GetPowerAmount<BeliefPower>(), MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null);
			}
			if (drawResult != DrawResultType.GreatLuck)
			{
				await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.Damage.BaseValue, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, Owner.Creature, this);
			}

			foreach (Creature teammate in CombatState.Players
				.Where(player => player != Owner && player.Creature.IsAlive)
				.Select(player => player.Creature))
			{
				await CreatureCmd.Damage(choiceContext, teammate, DynamicVars.Damage.BaseValue, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, Owner.Creature, this);
			}

			await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
				.FromCard(this)
				.WithHitFx(null)
				.TargetingAllOpponents(CombatState!)
				.Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
		}
	}
}
