using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(CurseCardPool))]
	public sealed class Ghost : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Ethereal];
		public override int MaxUpgradeLevel => 0;

		protected override bool IsPlayable => false;

		public Ghost() : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
		{
		}

		public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromStartOfTurn)
		{
			await base.AfterCardDrawn(choiceContext, card, fromStartOfTurn);
			if (card != this)
			{
				return;
			}

			foreach (CardModel handCard in PileType.Hand.GetPile(Owner).Cards.ToList())
			{
				if (!handCard.Keywords.Contains(CardKeyword.Ethereal))
				{
					handCard.AddKeyword(CardKeyword.Ethereal);
					CardCmd.Preview(handCard);
				}
			}
		}

		protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			return Task.CompletedTask;
		}

		protected override void OnUpgrade()
		{
		}
	}
}


