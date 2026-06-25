using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Events
{
	public sealed class SearchForNewsTengu : SanaeEventModel
	{
		public override string? CustomInitialPortraitPath => "res://TH_Sanae/ArtWorks/Events/searchfortengu.png";
		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new GoldVar(250),
			new HpLossVar(0)
		];

		public override void CalculateVars()
		{
			DynamicVars.Gold.BaseValue = Owner.RunState.AscensionLevel >= 10 ? 200 : 250;
			DynamicVars.HpLoss.BaseValue = Owner.RunState.AscensionLevel >= 10 ? Owner.Creature.MaxHp / 5m : Owner.Creature.MaxHp / 10m;
		}

		protected override IReadOnlyList<EventOption> GenerateInitialOptions()
		{
			EventOption companionOption = Owner.GetRelic<AyaNews>() != null
				? new EventOption(this, RequestCompanion, $"{Id.Entry}.pages.INITIAL.options.REQUEST_COMPANION", HoverTipFactory.FromRelic<AyaFollow>())
				: new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.REQUEST_COMPANION_LOCKED");

			return
			[
				companionOption,
				new EventOption(this, Interview, $"{Id.Entry}.pages.INITIAL.options.INTERVIEW", HoverTipFactory.FromCard<Writhe>()),
				new EventOption(this, Refuse, $"{Id.Entry}.pages.INITIAL.options.REFUSE").ThatDoesDamage(DynamicVars.HpLoss.BaseValue)
			];
		}

		private async Task RequestCompanion()
		{
			AyaNews? relic = Owner.GetRelic<AyaNews>();
			if (relic != null)
			{
				await RelicCmd.Replace(relic, ModelDb.Relic<AyaFollow>().ToMutable());
			}

			SetEventFinished(L10NLookup($"{Id.Entry}.pages.REQUEST_COMPANION.description"));
		}

		private async Task Interview()
		{
			await PlayerCmd.GainGold(DynamicVars.Gold.IntValue, Owner);
			CardModel card = Owner.RunState.CreateCard(ModelDb.Card<Writhe>(), Owner);
			CardPileAddResult result = await CardPileCmd.Add(card, PileType.Deck);
			CardCmd.PreviewCardPileAdd(result, 2f);
			SetEventFinished(L10NLookup($"{Id.Entry}.pages.INTERVIEW.description"));
		}

		private async Task Refuse()
		{
			await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
			SetEventFinished(L10NLookup($"{Id.Entry}.pages.REFUSE.description"));
		}
	}
}
