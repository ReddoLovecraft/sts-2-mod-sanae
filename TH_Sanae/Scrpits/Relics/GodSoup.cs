using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
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
using TH_Sanae.Scrpits.Cards;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class GodSoup : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
		public override RelicRarity Rarity => RelicRarity.Ancient;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<HarvestRice>()];

		public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
	{
		if (player == base.Owner && base.Owner.PlayerCombatState.TurnNumber == 1)
		{
			Flash();
			for (int i = 0; (decimal)i < 5; i++)
			{
				CardModel card = combatState.CreateCard(ModelDb.Card<HarvestRice>(), base.Owner);
				CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, base.Owner, CardPilePosition.Random));
			}
		}
	}
		

	}
}
