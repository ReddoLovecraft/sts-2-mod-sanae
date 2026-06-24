using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(StatusCardPool))]
	public sealed class Hisoutensoku : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];
		public override int MaxUpgradeLevel => 0;

		public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();

		protected override bool ShouldGlowGoldInternal => Owner.Creature.GetPowerAmount<BeliefPower>() > 19;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<InducePower>(), HoverTipFactory.FromPower<BeliefPower>(), Tools.GetStaticKeyword("Piety"), Tools.GetStaticKeyword("Persuasion"), ..HoverTipFactory.FromCardWithCardHoverTips<FaithBroken>()];

		public Hisoutensoku() : base(0, CardType.Skill, CardRarity.Common, TargetType.AllEnemies)
		{
		}

		public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromStartOfTurn)
		{
			await base.AfterCardDrawn(choiceContext, card, fromStartOfTurn);
			if (card != this)
			{
				return;
			}

			if (Owner.Creature.HasPower<InducePower>())
			{
				int induceAmount = Owner.Creature.GetPowerAmount<InducePower>();
				await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, induceAmount, Owner.Creature, this);
				await PowerCmd.Remove<InducePower>(Owner.Creature);
			}
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int counts = 20;
			if (Owner.Creature.HasPower<BeliefPower>())
			{
				counts = Owner.Creature.GetPowerAmount<BeliefPower>();
				if (counts > 19)
				{
					await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, counts, Owner.Creature, this);
					counts *= 2;
				}
				else
				{
					int gainAmount = 20 - counts;
					if (gainAmount > 0)
					{
						await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, gainAmount, Owner.Creature, this);
					}
					counts = 20;
				}
			}
			else
			{
				await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, 20, Owner.Creature, this);
			}

			foreach (Creature enemy in CombatState?.HittableEnemies.ToList() ?? [])
			{
				await PowerCmd.Apply<BeliefPower>(choiceContext, enemy, counts, Owner.Creature, this);
				await ToolBox.Persuasion(Owner.Creature, enemy);
			}

			await CardPileCmd.AddCurseToDeck<FaithBroken>(Owner);

		// 	FaithBroken combatCard = CombatState!.CreateCard<FaithBroken>(Owner);
		// 	CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(combatCard, PileType.Discard, Owner, CardPilePosition.Random));
		}

		protected override void OnUpgrade()
		{
		}
	}
}
