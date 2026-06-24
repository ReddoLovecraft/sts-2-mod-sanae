using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class PrimalJapaneseSceneTheGirlSawPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player)
			{
				return;
			}

			CardPile? exhaustPile = PileType.Exhaust.GetPile(player);
			if (exhaustPile == null || exhaustPile.IsEmpty)
			{
				return;
			}

			int count = int.Min(Amount, exhaustPile.Cards.Count);
			CardSelectorPrefs prefs = new(CardSelectorPrefs.TransformSelectionPrompt, count);
			List<CardModel> selectedCards = (await CardSelectCmd.FromCombatPile(choiceContext, exhaustPile, player, prefs)).ToList();
			foreach (CardModel selectedCard in selectedCards)
			{
				await CardPileCmd.Add(selectedCard, PileType.Hand, CardPilePosition.Random, selectedCard, false);
				selectedCard.EnergyCost.SetUntilPlayed(0);
			}
		}
	}
}
