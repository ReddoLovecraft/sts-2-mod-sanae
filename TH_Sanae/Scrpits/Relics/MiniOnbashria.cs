using System.Linq;
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
using MegaCrit.Sts2.Core.Runs;
using TH_Sanae.Scripts.Powers;
using TH_Sanae.Scrpits.Cards;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class MiniOnbashria : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
		public override RelicRarity Rarity => RelicRarity.Ancient;

		public override bool IsAllowed(IRunState runState)
		{
			return runState.CurrentActIndex == 1;
		}

		public override bool HasUponPickupEffect => true;
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<KanakoSummonOnbashira>(),HoverTipFactory.FromCard<ExtendOnbashria>()];
		public override async Task AfterObtained()
	{
		List<CardModel> deckCards = ToolBox.GetPile(base.Owner, PileType.Deck)?.Cards.ToList() ?? new List<CardModel>();
		if (deckCards.Count == 0)
		{
			return;
		}

		List<CardModel> startingDeck = base.Owner.Character.StartingDeck.ToList();
		List<ModelId> startingStrikeIds = startingDeck.Where(c => c.Tags.Contains(CardTag.Strike)).Select(c => c.Id).Distinct().ToList();
		List<ModelId> startingDefendIds = startingDeck.Where(c => c.Tags.Contains(CardTag.Defend)).Select(c => c.Id).Distinct().ToList();

		List<CardModel> strikesToReplace = deckCards.Where(c => startingStrikeIds.Contains(c.Id)).ToList();
		List<CardModel> defendsToReplace = deckCards.Where(c => startingDefendIds.Contains(c.Id)).ToList();

		List<CardTransformation> transformations = new List<CardTransformation>(strikesToReplace.Count + defendsToReplace.Count);
		foreach (CardModel strike in strikesToReplace)
		{
			CardModel replacement = base.Owner.RunState.CreateCard(ModelDb.Card<KanakoSummonOnbashira>(), base.Owner);
			transformations.Add(new CardTransformation(strike, replacement));
		}

		foreach (CardModel defend in defendsToReplace)
		{
			CardModel replacement = base.Owner.RunState.CreateCard(ModelDb.Card<ExtendOnbashria>(), base.Owner);
			transformations.Add(new CardTransformation(defend, replacement));
		}

		if (transformations.Count > 0)
		{
			await CardCmd.Transform(transformations, null);
		}
	}

	}
}
