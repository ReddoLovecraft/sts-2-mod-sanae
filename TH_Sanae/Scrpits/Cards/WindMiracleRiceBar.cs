using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class WindMiracleRiceBar : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardModifier.MiracleKeyword, CardKeyword.Exhaust];
		private int _remainingUses = 3;

		public override int MaxUpgradeLevel => 0;

		protected override bool ShouldGlowGoldInternal => PileType.Hand.GetPile(Owner).Cards.OfType<HarvestRice>().Any();

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WindPower>(), ..HoverTipFactory.FromCardWithCardHoverTips<HarvestRice>(), Tools.GetStaticKeyword("Remove")];

		[SavedProperty]
		public int RemainingUses
		{
			get => _remainingUses;
			set
			{
				AssertMutable();
				_remainingUses = value;
				DynamicVars["Cards"].BaseValue = value;
			}
		}

		public WindMiracleRiceBar() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			ModifyRemainingUses(-1);
			(DeckVersion as WindMiracleRiceBar)?.ModifyRemainingUses(-1);

			if (Owner.Creature.HasPower<WindPower>())
			{
				await CreatureCmd.Heal(Owner.Creature, Owner.Creature.GetPowerAmount<WindPower>());
			}

			List<HarvestRice> riceCards = PileType.Hand.GetPile(Owner).Cards.OfType<HarvestRice>().ToList();
			foreach (HarvestRice riceCard in riceCards)
			{
				await CardCmd.Exhaust(choiceContext, riceCard);
				ModifyRemainingUses(1);
				(DeckVersion as WindMiracleRiceBar)?.ModifyRemainingUses(1);
			}

			if (RemainingUses <= 0 && DeckVersion != null)
			{
				await CardPileCmd.RemoveFromDeck(DeckVersion);
				DeckVersion = null;
			}
		}

		protected override void OnUpgrade()
		{
		}

		private void ModifyRemainingUses(int delta)
		{
			RemainingUses += delta;
			DynamicVars.RecalculateForUpgradeOrEnchant();
		}
	}
}
