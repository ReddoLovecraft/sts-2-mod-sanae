using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Events
{
	public sealed class SimilarShrine : SanaeEventModel
	{
		public override string? CustomInitialPortraitPath => "res://TH_Sanae/ArtWorks/Events/similarshrine.png";
		protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(0)];

		public override void CalculateVars()
		{
			DynamicVars.HpLoss.BaseValue = Owner.RunState.AscensionLevel >= 10 ? 10 : 15;
		}

		protected override IReadOnlyList<EventOption> GenerateInitialOptions()
		{
			return GenerateChoosingOptions();
		}

		private async Task Touch()
		{
			await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, null, null);

			switch (Rng.NextInt(3))
			{
				case 0:
					await RemoveCard();
					break;
				case 1:
					await UpgradeCard();
					break;
				default:
					await TransformCard();
					break;
			}

			SetEventState(
				L10NLookup($"{Id.Entry}.pages.TOUCH.description"),
				GenerateChoosingOptions());
		}

		private Task Leave()
		{
			SetEventFinished(L10NLookup($"{Id.Entry}.pages.LEAVE.description"));
			return Task.CompletedTask;
		}

		private IReadOnlyList<EventOption> GenerateChoosingOptions()
		{
			return
			[
				new EventOption(this, Touch, $"{Id.Entry}.pages.INITIAL.options.TOUCH", HoverTipFactory.Static(StaticHoverTip.Transform)).ThatDoesDamage(DynamicVars.HpLoss.BaseValue),
				new EventOption(this, Leave, $"{Id.Entry}.pages.INITIAL.options.LEAVE")
			];
		}

		private async Task UpgradeCard()
		{
			CardModel? card = (await CardSelectCmd.FromDeckForUpgrade(Owner, new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1))).FirstOrDefault();
			if (card != null)
			{
				CardCmd.Upgrade(card);
			}
		}

		private async Task TransformCard()
		{
			CardModel? card = (await CardSelectCmd.FromDeckForTransformation(Owner, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1))).FirstOrDefault();
			if (card != null)
			{
				await CardCmd.TransformToRandom(card, Rng, CardPreviewStyle.EventLayout);
			}
		}

		private async Task RemoveCard()
		{
			CardModel? card = (await CardSelectCmd.FromDeckForRemoval(Owner, new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1))).FirstOrDefault();
			if (card != null)
			{
				await CardPileCmd.RemoveFromDeck(card);
			}
		}
	}
}
