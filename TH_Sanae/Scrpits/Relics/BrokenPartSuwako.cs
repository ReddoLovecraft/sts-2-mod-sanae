using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class BrokenPartSuwako : SanaeRelicModel
	{
		public override RelicRarity Rarity => RelicRarity.Event;

		public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
		{
			if (card.Owner != Owner || card.Type != CardType.Attack || card.EnergyCost.CostsX)
			{
				return playCount;
			}

			return card.EnergyCost.GetWithModifiers(CostModifiers.All) <= 1 ? playCount + 1 : playCount;
		}

		public override Task AfterModifyingCardPlayCount(CardModel card)
		{
			Flash();
			return Task.CompletedTask;
		}
	}
}
