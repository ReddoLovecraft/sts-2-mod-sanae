using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class ChangedTimeLine : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
		private const string _countKey = "Count";
		private int _count = 1;

		public override RelicRarity Rarity => RelicRarity.Event;

		public override bool ShowCounter => true;

		public override int DisplayAmount => Count;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar(_countKey, 1m)];

		public int Count
		{
			get => _count;
			set
			{
				AssertMutable();
				_count = Math.Max(1, value);
				DynamicVars[_countKey].BaseValue = _count;
				InvokeDisplayAmountChanged();
			}
		}

		public void AddCounter()
		{
			Count += 1;
		}

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner)
			{
				return;
			}

			CardPile? handPile = PileType.Hand.GetPile(Owner);
			if (handPile == null || handPile.IsEmpty)
			{
				return;
			}

			CardSelectorPrefs discardPrefs = new(SelectionScreenPrompt, 0, Count)
			{
				Cancelable = true
			};
			List<CardModel> discardedCards = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner, discardPrefs, null, this)).ToList();
			if (discardedCards.Count == 0)
			{
				return;
			}

			Flash();
			await CardCmd.Discard(choiceContext, discardedCards);

			CardPile? discardPile = PileType.Discard.GetPile(Owner);
			if (discardPile == null || discardPile.IsEmpty)
			{
				return;
			}

			CardSelectorPrefs returnPrefs = new(ToolBox.GetCustomText("relics", Id.Entry, ".returnSelectionScreenPrompt"), discardedCards.Count);
			List<CardModel> cardsToReturn = (await CardSelectCmd.FromCombatPile(choiceContext, discardPile, Owner, returnPrefs)).ToList();
			foreach (CardModel card in cardsToReturn)
			{
				await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Random, this);
			}
		}
	}
}
