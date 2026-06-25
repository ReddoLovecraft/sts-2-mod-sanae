using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rewards;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Events
{
	public sealed class WindGodLake : SanaeEventModel
	{
		public override string? CustomInitialPortraitPath => "res://TH_Sanae/ArtWorks/Events/windgodlake.png";
		protected override IReadOnlySet<int> AllowedActs => new HashSet<int> { 1, 2 };
		protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(0)];

		public override void CalculateVars()
		{
			DynamicVars.Heal.BaseValue = Owner.RunState.AscensionLevel >= 10
				? Owner.Creature.MaxHp / 4m
				: Owner.Creature.MaxHp / 3m;
		}

		protected override IReadOnlyList<EventOption> GenerateInitialOptions()
		{
			return
			[
				new EventOption(this, Drink, $"{Id.Entry}.pages.INITIAL.options.DRINK"),
				new EventOption(this, Bottle, $"{Id.Entry}.pages.INITIAL.options.BOTTLE"),
				new EventOption(this, Bathe, $"{Id.Entry}.pages.INITIAL.options.BATHE", HoverTipFactory.FromCard<Shame>())
			];
		}

		private async Task Drink()
		{
			await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);
			SetEventFinished(L10NLookup($"{Id.Entry}.pages.DRINK.description"));
		}

		private async Task Bottle()
		{
			int potionCount = 2;
			List<Reward> rewards = Enumerable.Range(0, potionCount)
				.Select(_ => (Reward)new PotionReward(Owner))
				.ToList();
			await RewardsCmd.OfferCustom(Owner, rewards);
			SetEventFinished(L10NLookup($"{Id.Entry}.pages.BOTTLE.description"));
		}

		private async Task Bathe()
		{
			List<CardModel> removableCards = PileType.Deck.GetPile(Owner).Cards.Where(static card => card.IsRemovable).ToList();
			if (removableCards.Count <= 3)
			{
				if (removableCards.Count > 0)
				{
					await CardPileCmd.RemoveFromDeck(removableCards);
				}
			}
			else
			{
				CardSelectorPrefs prefs = new(CardSelectorPrefs.RemoveSelectionPrompt, 3);
				List<CardModel> selected = (await CardSelectCmd.FromDeckForRemoval(Owner, prefs)).ToList();
				if (selected.Count > 0)
				{
					await CardPileCmd.RemoveFromDeck(selected);
				}
			}

			CardModel curse = Owner.RunState.CreateCard(ModelDb.Card<Shame>(), Owner);
			CardPileAddResult result = await CardPileCmd.Add(curse, PileType.Deck);
			CardCmd.PreviewCardPileAdd(result, 2f);

			SetEventFinished(L10NLookup($"{Id.Entry}.pages.BATHE.description"));
		}
	}
}
