using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class HinaFollow : SanaeRelicModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

		public override RelicRarity Rarity => RelicRarity.Event;

		public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
		{
			if (card.Owner != Owner || card.Type != CardType.Curse || card.Pile?.Type != PileType.Hand)
			{
				return;
			}

			Flash();
			await CardCmd.Exhaust(choiceContext, card);
			await PlayerCmd.GainEnergy(1, Owner);
			if (!Owner.Creature.HasPower<NoDrawPower>())
			{
				await CardPileCmd.Draw(choiceContext, Owner);
			}
		}
	}
}
