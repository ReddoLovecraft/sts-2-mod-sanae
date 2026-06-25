using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using TH_Sanae.Scrpits.Cards;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Events
{
	public sealed class AbandonedMoriyaShrine : SanaeEventModel
	{
		public override string? CustomInitialPortraitPath => "res://TH_Sanae/ArtWorks/Events/abandonedmoriyashrine.png";
		protected override IReadOnlySet<int> AllowedActs => new HashSet<int> { 2 };
		protected override bool RequiresAllSanaeParty => true;
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0)];

		public override void CalculateVars()
		{
			DynamicVars.Cards.BaseValue = Owner.RunState.AscensionLevel >= 10 ? 3 : 5;
		}

		protected override IReadOnlyList<EventOption> GenerateInitialOptions()
		{
			bool hasStarterRelic = GetFaithRelic() != null;
			EventOption gensokyoOption = hasStarterRelic
				? new EventOption(this, EnterGensokyo, $"{Id.Entry}.pages.INITIAL.options.ENTER_GENSOKYO", HoverTipFactory.FromCard<KamiKakushi>())
				: new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.ENTER_GENSOKYO_LOCKED");

			EventOption restoreOption = GetFaithCounter() >= 12
				? new EventOption(this, RestoreShrine, $"{Id.Entry}.pages.INITIAL.options.RESTORE_SHRINE", HoverTipFactory.FromRelic<ChangedTimeLine>())
				: new EventOption(this, null, hasStarterRelic ? $"{Id.Entry}.pages.INITIAL.options.RESTORE_SHRINE_LOCKED" : $"{Id.Entry}.pages.INITIAL.options.ENTER_GENSOKYO_LOCKED");

			return
			[
				gensokyoOption,
				new EventOption(this, StayAlone, $"{Id.Entry}.pages.INITIAL.options.STAY_ALONE", HoverTipFactory.FromCard<Normality>()),
				restoreOption
			];
		}

		private async Task EnterGensokyo()
		{
			switch (GetFaithRelic())
			{
				case FakeFaith fakeFaith:
					fakeFaith.HealAmount = 0;
					break;
				case TrueFaith trueFaith:
					trueFaith.HealAmount = 0;
					break;
			}

			for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
			{
				CardModel card = Owner.RunState.CreateCard(ModelDb.Card<KamiKakushi>(), Owner);
				await CardPileCmd.Add(card, PileType.Deck);
			}

			SetEventFinished(L10NLookup($"{Id.Entry}.pages.ENTER_GENSOKYO.description"));
		}

		private async Task StayAlone()
		{
			CardModel curse = Owner.RunState.CreateCard(ModelDb.Card<Normality>(), Owner);
			CardPileAddResult result = await CardPileCmd.Add(curse, PileType.Deck);
			CardCmd.PreviewCardPileAdd(result, 2f);
			SetEventFinished(L10NLookup($"{Id.Entry}.pages.STAY_ALONE.description"));
		}

		private async Task RestoreShrine()
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

			SetEventFinished(L10NLookup($"{Id.Entry}.pages.RESTORE_SHRINE.description"));
		}

		private SanaeRelicModel? GetFaithRelic()
		{
			TrueFaith? trueFaith = Owner.GetRelic<TrueFaith>();
			if (trueFaith != null)
			{
				return trueFaith;
			}

			return Owner.GetRelic<FakeFaith>();
		}

		private int GetFaithCounter()
		{
			return GetFaithRelic() switch
			{
				TrueFaith trueFaith => trueFaith.HealAmount,
				FakeFaith fakeFaith => fakeFaith.HealAmount,
				_ => -1
			};
		}
	}
}
