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
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rooms;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class CurseMaterial : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

		public override RelicRarity Rarity => RelicRarity.Ancient;

		public override bool IsAllowed(IRunState runState)
		{
			return runState.CurrentActIndex == 1;
		}

		public override decimal ModifyMaxEnergy(Player player, decimal amount)
		{
			if (player != Owner)
			{
				return amount;
			}

			return amount + 1;
		}
		
		public override async Task AfterRoomEntered(AbstractRoom _)
	{
		if (!base.Owner.Creature.IsDead)
		{
			await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), base.Owner.Creature, 1m, isFromCard: false);
		}
	}
		
	}
}
