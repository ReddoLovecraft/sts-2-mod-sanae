using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Events
{
	public sealed class NewspaperSale : SanaeEventModel
	{
		public override string? CustomInitialPortraitPath => "res://TH_Sanae/ArtWorks/Events/newspapersale.png";
		protected override IReadOnlySet<int> AllowedActs => new HashSet<int> { 0, 1 };
		private int GoldCost => Owner.RunState.AscensionLevel >= 10 ? 125 : 100;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(0)];

		public override void CalculateVars()
		{
			DynamicVars.Gold.BaseValue = GoldCost;
		}

		protected override IReadOnlyList<EventOption> GenerateInitialOptions()
		{
			EventOption buyOption = Owner.Gold >= GoldCost
				? new EventOption(this, Buy, $"{Id.Entry}.pages.INITIAL.options.BUY", HoverTipFactory.FromRelic<AyaNews>())
				: new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.BUY_LOCKED");

			return
			[
				buyOption,
				new EventOption(this, Leave, $"{Id.Entry}.pages.INITIAL.options.LEAVE")
			];
		}

		private async Task Buy()
		{
			await PlayerCmd.LoseGold(GoldCost, Owner, MegaCrit.Sts2.Core.Entities.Gold.GoldLossType.Spent);
			await RelicCmd.Obtain<AyaNews>(Owner);
			SetEventFinished(L10NLookup($"{Id.Entry}.pages.BUY.description"));
		}

		private Task Leave()
		{
			SetEventFinished(L10NLookup($"{Id.Entry}.pages.LEAVE.description"));
			return Task.CompletedTask;
		}
	}
}
