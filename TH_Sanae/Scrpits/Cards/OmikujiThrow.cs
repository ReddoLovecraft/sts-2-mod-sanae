using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Patches;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class OmikujiThrow : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardModifier.DrawKeyword];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3, ValueProp.Move), new CardsVar(3)];

		public OmikujiThrow() : base(1, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			DrawResultType drawResult = CardKeywordAfterDrawPatch.InferDrawResult(this);
			for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
			{
				Creature? enemy = Owner.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
				if (enemy == null)
				{
					return;
				}
				if (NCombatRoom.Instance != null)
				{
					NCreature? attackerNode = NCombatRoom.Instance.GetCreatureNode(Owner.Creature);
					NCreature? targetNode = NCombatRoom.Instance.GetCreatureNode(enemy);
					if (attackerNode != null && targetNode != null)
					{
						Vector2 startPos = attackerNode.VfxSpawnPosition;
						Vector2 endPos = targetNode.GetTopOfHitbox() + Vector2.Up * 18f;
						var vfx = NOmikujiBulletVfx.Create(startPos, endPos, drawResult);
						if (vfx != null)
						{
							NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
						}
						await Cmd.Wait(0.35f);
					}
				}

				if (Owner.Character is SanaeCharacter)
				{
					await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
						.FromCard(this)
						.WithHitFx(null)
						.WithAttackerAnim("Throw", base.Owner.Character.CastAnimDelay, base.Owner.Creature)
						.Targeting(enemy)
						.Execute(choiceContext);
				}
				else
				{
					await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
						.FromCard(this)
						.WithHitFx(null)
						.WithAttackerAnim("Cast", base.Owner.Character.CastAnimDelay, base.Owner.Creature)
						.Targeting(enemy)
						.Execute(choiceContext);
				}
			}
				
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2);
		}
	}
}
