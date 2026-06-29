using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class GodMountainOffering : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
		protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
		protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";

		public override RelicRarity Rarity => RelicRarity.Ancient;

		public override bool ShouldAllowSelectingMoreCardRewards(Player player, CardReward cardReward)
		{
			return player == Owner;
		}

		public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
		{
			if (oldPileType == PileType.Deck || card.Pile?.Type != PileType.Deck || card.Owner != Owner)
			{
				return;
			}

			Flash();
			await CreatureCmd.GainMaxHp(Owner.Creature, 2m);
		}

		public override async Task AfterPotionProcured(PotionModel potion)
		{
			if (potion.Owner != Owner)
			{
				return;
			}

			Flash();
			await CreatureCmd.GainMaxHp(Owner.Creature, 5m);
		}

		public override async Task AfterRewardTaken(Player player, Reward reward)
		{
			if (player != Owner || reward is not RelicReward)
			{
				return;
			}

			Flash();
			await CreatureCmd.GainMaxHp(Owner.Creature, 8m);
		}
	}
}
