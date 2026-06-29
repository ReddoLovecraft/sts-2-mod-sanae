using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TH_Sanae.Scrpits.Cards;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class OldStationCard : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
		public override bool HasUponPickupEffect => true;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<LostStation>()];

		public override RelicRarity Rarity => RelicRarity.Ancient;

		public override bool IsAllowed(IRunState runState)
		{
			return runState.Players.Any(p => p != null && p.Character is SanaeCharacter);
		}

		public override async Task AfterObtained()
		{
			CardModel card = base.Owner.RunState.CreateCard(ModelDb.Card<LostStation>(), base.Owner);
			CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck);
			CardCmd.PreviewCardPileAdd(result, 2f);
		}
	}
}
