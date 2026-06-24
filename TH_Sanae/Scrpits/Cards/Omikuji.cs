using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(ColorlessCardPool))]
	public sealed class Omikuji : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];
		public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardModifier.DrawKeyword)];

		public Omikuji() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, showInCardLibrary: false)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int selectCount = DynamicVars["Cards"].IntValue;
			if (selectCount <= 0)
			{
				return;
			}

			IReadOnlyList<CardModel> handCards = PileType.Hand.GetPile(Owner).Cards
				.Where(card => card != this)
				.ToList();
			if (handCards.Count == 0)
			{
				return;
			}

			selectCount = int.Min(selectCount, handCards.Count);
			CardSelectorPrefs prefs = new(SelectionScreenPrompt, selectCount);
			IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromHand(choiceContext, Owner, prefs, card => card != this, this);
			foreach (CardModel selectedCard in selectedCards.ToList())
			{
				ToolBox.ApplyOmikuji(selectedCard);
				CardCmd.Preview(selectedCard);
			}
		}

		protected override void OnUpgrade()
		{
		}
	}
}


