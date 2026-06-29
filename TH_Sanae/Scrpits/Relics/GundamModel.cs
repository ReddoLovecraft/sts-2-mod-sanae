using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers.Models;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class GundamModel : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
		public override RelicRarity Rarity => RelicRarity.Ancient;

		public override bool IsAllowed(IRunState runState)
		{
			return runState.CurrentActIndex == 1 && runState.Players.Any(p => p != null && p.Character is SanaeCharacter);
		}

		public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
		{
			if (player != Owner)
			{
				return false;
			}
			this.Flash();

			IEnumerable<CardModel> pool = creationOptions.GetPossibleCards(player)
				.Where(c => cardRewardOptions.TrueForAll(o => o.originalCard.Id != c.Id));

			if (!pool.Any())
			{
				pool = creationOptions.GetPossibleCards(player);
			}

			CardCreationOptions extraOptions = new CardCreationOptions(pool, CardCreationSource.Other, creationOptions.RarityOdds)
				.WithFlags(CardCreationFlags.NoModifyHooks | CardCreationFlags.NoCardPoolModifications);

			int addedCount = 0;
			foreach (CardCreationResult result in CardFactory.CreateForReward(player, 2, extraOptions))
			{
				if (result.Card == null)
				{
					continue;
				}

				result.ModifyCard(result.Card, this);
				cardRewardOptions.Add(result);
				addedCount++;
			}

			return addedCount > 0;
		}
		public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewards, CardCreationOptions options)
	{
		if (player != base.Owner)
		{
			return false;
		}
		if (options.Flags.HasFlag(CardCreationFlags.NoHookUpgrades))
		{
			return false;
		}
		ToolBox.UpgradeValidCards(cardRewards, base.Owner, this);
		return true;
	}

	public override void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> cards)
	{
		if (player == base.Owner)
		{
			ToolBox.UpgradeValidCards(cards, base.Owner, this);
		}
	}

	public override bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
	{
		newCard = null;
		if (card.Owner != base.Owner)
		{
			return false;
		}
		if (!card.IsUpgradable)
		{
			return false;
		}
		newCard = base.Owner.RunState.CloneCard(card);
		CardCmd.Upgrade(newCard, CardPreviewStyle.None);
		return true;
	}
	}
}
