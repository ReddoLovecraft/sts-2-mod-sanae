using System.Linq;
using System.Threading.Tasks;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using Patchouib.Scrpits.Main;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class HinaNingyou : SanaeRelicModel, IRightCilckable
	{
		public override RelicRarity Rarity => RelicRarity.Shop;

		public async Task OnRightClick(PlayerChoiceContext context)
		{
			Flash();

			var removableDeckCards = PileType.Deck.GetPile(Owner)
				.Cards
				.Where(card => card.Type == CardType.Curse || card.Type == CardType.Status)
				.ToList();
			if (removableDeckCards.Count > 0)
			{
				await CardPileCmd.RemoveFromDeck(removableDeckCards);
			}

			var cardsToExhaust = Owner.PlayerCombatState?.AllCards
				.Where(card => card.Pile?.Type != PileType.Exhaust && (card.Type == CardType.Curse || card.Type == CardType.Status))
				.ToList() ?? [];
			foreach (CardModel card in cardsToExhaust)
			{
				if (card.Pile?.Type == PileType.Exhaust)
				{
					continue;
				}

				await CardCmd.Exhaust(context, card);
			}

			await RelicCmd.Replace(this, ModelDb.Relic<GoneHinaNingyou>().ToMutable());
		}
	}
}
