using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Events
{
	public sealed class CollectBadLuckHina : SanaeEventModel
	{
		public override string? CustomInitialPortraitPath => "res://TH_Sanae/ArtWorks/Events/collectbadluckhina.png";
		protected override IReadOnlySet<int> AllowedActs => new HashSet<int> { 2, 3 };
		protected override bool RequiresSanaeInParty => true;
		private const int GoldCost = 80;

		protected override IReadOnlyList<EventOption> GenerateInitialOptions()
		{
			EventOption companionOption = Owner.GetRelic<GoneHinaNingyou>() != null
				? new EventOption(this, RequestCompanion, $"{Id.Entry}.pages.INITIAL.options.REQUEST_COMPANION", HoverTipFactory.FromRelic<HinaFollow>())
				: new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.REQUEST_COMPANION_LOCKED");

			EventOption buyOption = Owner.Gold >= GoldCost
				? new EventOption(this, BuyDoll, $"{Id.Entry}.pages.INITIAL.options.BUY_DOLL", HoverTipFactory.FromRelic<HinaNingyou>())
				: new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.BUY_DOLL_LOCKED");

			return
			[
				new EventOption(this, PurifyBadLuck, $"{Id.Entry}.pages.INITIAL.options.PURIFY_BAD_LUCK"),
				companionOption,
				buyOption
			];
		}

		private async Task PurifyBadLuck()
		{
			List<CardModel> curses = PileType.Deck.GetPile(Owner)
				.Cards
				.Where(static card => card.Type == CardType.Curse)
				.ToList();
			if (curses.Count > 0)
			{
				await CardPileCmd.RemoveFromDeck(curses);
			}

			SetEventFinished(L10NLookup($"{Id.Entry}.pages.PURIFY_BAD_LUCK.description"));
		}

		private async Task RequestCompanion()
		{
			GoneHinaNingyou? relic = Owner.GetRelic<GoneHinaNingyou>();
			if (relic != null)
			{
				await RelicCmd.Remove(relic);
			}

			await RelicCmd.Obtain<HinaFollow>(Owner);
			SetEventFinished(L10NLookup($"{Id.Entry}.pages.REQUEST_COMPANION.description"));
		}

		private async Task BuyDoll()
		{
			await PlayerCmd.LoseGold(GoldCost, Owner, GoldLossType.Spent);
			if (Owner.GetRelic<HinaNingyou>() == null)
			{
				await RelicCmd.Obtain<HinaNingyou>(Owner);
			}
			else
			{
				await RelicCmd.Obtain(ModelDb.Relic<Circlet>().ToMutable(), Owner);
			}

			SetEventFinished(L10NLookup($"{Id.Entry}.pages.BUY_DOLL.description"));
		}
	}
}
