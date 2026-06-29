using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class Ichimegasa : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

		public override RelicRarity Rarity => RelicRarity.Ancient;
		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner)
		{
			return;
		}
		if (base.Owner.PlayerCombatState.TurnNumber > 1)
		{
			return;
		}
		Flash();
		foreach(CardModel card in PileType.Hand.GetPile(base.Owner).Cards.ToList())
		{
			if(!card.Keywords.Contains(CardKeyword.Ethereal))
			CardCmd.ApplyKeyword(card, CardKeyword.Ethereal);
			card.EnergyCost.AddThisCombat(-1);
			await CardCmd.Afflict<Hexed>(card, 1);
		}
		//CardCmd.Upgrade(PileType.Hand.GetPile(base.Owner).Cards, CardPreviewStyle.HorizontalLayout);
	}
	}
}
