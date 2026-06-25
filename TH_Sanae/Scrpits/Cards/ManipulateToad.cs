using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
[Pool(typeof(SanaeCardPool))]
	public sealed class ManipulateToad : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
		protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded?[HoverTipFactory.FromKeyword(CardKeyword.Exhaust)]:[];

		public ManipulateToad() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			CardSelectorPrefs prefs = new(SelectionScreenPrompt, 0, DynamicVars.Cards.IntValue)
			{
				Cancelable = true
			};
			List<CardModel> selectedCards = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner, prefs, null, this)).ToList();
			if (selectedCards.Count == 0)
			{
				return;
			}

			if (IsUpgraded)
			{
				foreach (CardModel card in selectedCards)
				{
					await CardCmd.Exhaust(choiceContext, card);
				}
			}
			else
			{
				await CardCmd.Discard(choiceContext, selectedCards);
			}

			await CardPileCmd.Draw(choiceContext, selectedCards.Count, Owner);
		}

		protected override void OnUpgrade()
		{
			
		}
	}
}

