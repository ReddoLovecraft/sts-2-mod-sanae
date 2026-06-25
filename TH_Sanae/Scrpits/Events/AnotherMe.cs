using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using TH_Sanae.Scrpits.Cards;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Events
{
	public sealed class AnotherMe : SanaeEventModel
	{
		public override string? CustomInitialPortraitPath => "res://TH_Sanae/ArtWorks/Events/anotherme.png";
		protected override IReadOnlyList<EventOption> GenerateInitialOptions()
		{
			return
			[
				new EventOption(this, Chase, $"{Id.Entry}.pages.INITIAL.options.CHASE"),
				new EventOption(this, Ignore, $"{Id.Entry}.pages.INITIAL.options.IGNORE", HoverTipFactory.FromCard<Doubt>())
			];
		}

		private Task Chase()
		{
			SetEventState(
				L10NLookup($"{Id.Entry}.pages.CHASE.description"),
				[
					new EventOption(this, Talk, $"{Id.Entry}.pages.CHASE.options.TALK", HoverTipFactory.FromRelic<ChangedTimeLine>().Append(HoverTipFactory.FromCard<Ghost>()))
				]);
			return Task.CompletedTask;
		}

		private async Task Talk()
		{
			ChangedTimeLine? timeline = Owner.GetRelic<ChangedTimeLine>();
			if (timeline != null)
			{
				timeline.AddCounter();
			}
			else
			{
				await RelicCmd.Obtain<ChangedTimeLine>(Owner);
			}

			CardModel card = Owner.RunState.CreateCard(ModelDb.Card<Ghost>(), Owner);
			CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck);
			CardCmd.PreviewCardPileAdd(result, 2f);

			SetEventFinished(L10NLookup($"{Id.Entry}.pages.TALK.description"));
		}

		private async Task Ignore()
		{
			CardModel card = Owner.RunState.CreateCard(ModelDb.Card<Doubt>(), Owner);
			CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck);
			CardCmd.PreviewCardPileAdd(result, 2f);

			SetEventFinished(L10NLookup($"{Id.Entry}.pages.IGNORE.description"));
		}
	}
}
