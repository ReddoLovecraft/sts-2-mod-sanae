using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Events
{
	public sealed class MoriyaBranch : SanaeEventModel
	{
		public override string? CustomInitialPortraitPath => "res://TH_Sanae/ArtWorks/Events/moriyabanchanch.png";
		private bool _initialized;
		private bool _rewardSnake;

		protected override IEnumerable<DynamicVar> CanonicalVars =>
		[
			new HealVar(0),
			new GoldVar(0),
			new CardsVar(0),
			new StringVar("Relic")
		];

		public override void CalculateVars()
		{
			if (_initialized)
			{
				return;
			}

			int roll = Rng.NextInt(1, 4);
			DynamicVars.Heal.BaseValue = (int)(Owner.Creature.MaxHp * 0.3m);
			DynamicVars.Gold.BaseValue = roll * 30;
			DynamicVars.Cards.BaseValue = roll;
			_rewardSnake = roll % 2 == 0;
			((StringVar)DynamicVars["Relic"]).StringValue = _rewardSnake
				? ModelDb.Relic<SnakeTalisman>().Title.GetFormattedText()
				: ModelDb.Relic<FrogTalisman>().Title.GetFormattedText();
			_initialized = true;
		}

		protected override IReadOnlyList<EventOption> GenerateInitialOptions()
		{
			bool hasUpgradable = PileType.Deck.GetPile(Owner).Cards.Any(static card => card.IsUpgradable);
			EventOption prayOption = Owner.Gold >= DynamicVars.Gold.IntValue && hasUpgradable
				? new EventOption(this, Pray, $"{Id.Entry}.pages.INITIAL.options.PRAY")
				: new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.PRAY_LOCKED");

			EventOption wineOption = Owner.GetRelic<ChewingWine>() != null
				? new EventOption(this, MakeWine, $"{Id.Entry}.pages.INITIAL.options.MAKE_WINE")
				: new EventOption(this, null, $"{Id.Entry}.pages.INITIAL.options.MAKE_WINE_LOCKED");

			return
			[
				new EventOption(this, Rest, $"{Id.Entry}.pages.INITIAL.options.REST"),
				prayOption,
				new EventOption(this, TakeTalisman, $"{Id.Entry}.pages.INITIAL.options.TAKE_TALISMAN", GetRewardHoverTip()),
				wineOption
			];
		}

		private IEnumerable<IHoverTip> GetRewardHoverTip()
		{
			return _rewardSnake
				? HoverTipFactory.FromRelic<SnakeTalisman>()
				: HoverTipFactory.FromRelic<FrogTalisman>();
		}

		private async Task Rest()
		{
			await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);
			SetEventFinished(L10NLookup($"{Id.Entry}.pages.REST.description"));
		}

		private async Task Pray()
		{
			await PlayerCmd.LoseGold(DynamicVars.Gold.IntValue, Owner, GoldLossType.Spent);
			int upgradeCount = DynamicVars.Cards.IntValue;
			CardSelectorPrefs prefs = new(CardSelectorPrefs.UpgradeSelectionPrompt, 1, upgradeCount)
			{
				RequireManualConfirmation = true
			};

			foreach (CardModel card in await CardSelectCmd.FromDeckForUpgrade(Owner, prefs))
			{
				CardCmd.Upgrade(card);
			}

			SetEventFinished(L10NLookup($"{Id.Entry}.pages.PRAY.description"));
		}

		private async Task TakeTalisman()
		{
			if (_rewardSnake)
			{
				await RelicCmd.Obtain<SnakeTalisman>(Owner);
			}
			else
			{
				await RelicCmd.Obtain<FrogTalisman>(Owner);
			}

			SetEventFinished(L10NLookup($"{Id.Entry}.pages.TAKE_TALISMAN.description"));
		}

		private Task MakeWine()
		{
			if (Owner.GetRelic<ChewingWine>() is ChewingWine wine)
			{
				wine.Charges += 3;
			}

			SetEventFinished(L10NLookup($"{Id.Entry}.pages.MAKE_WINE.description"));
			return Task.CompletedTask;
		}
	}
}
