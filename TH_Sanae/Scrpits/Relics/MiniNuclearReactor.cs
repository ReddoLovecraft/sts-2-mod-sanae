using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class MiniNuclearReactor : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
		public override RelicRarity Rarity => RelicRarity.Ancient;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust),HoverTipFactory.ForEnergy(this)];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2)];

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != base.Owner)
		{
			return;
		}
		foreach (CardModel item in await CardSelectCmd.FromHand(choiceContext, player, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), null, this))
		{
			await CardCmd.Exhaust(choiceContext, item);
		}
		this.Flash();
		await PlayerCmd.GainEnergy(2,Owner);
		
	}
	}
}
