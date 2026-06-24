using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(StatusCardPool))]
	public sealed class HarvestRice : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];
		protected override bool IsPlayable => false;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(3)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

		public HarvestRice() : base(-2, CardType.Status, CardRarity.Common, TargetType.None, showInCardLibrary: false)
		{
		}

		public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromStartOfTurn)
		{
			await base.AfterCardDrawn(choiceContext, card, fromStartOfTurn);
			if (card == this)
			{
				await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
			}
		}

		public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool fromEndOfTurn)
		{
			await base.AfterCardExhausted(choiceContext, card, fromEndOfTurn);
			if (card == this && Owner.PlayerCombatState != null)
			{
				await PlayerCmd.GainEnergy(Owner.PlayerCombatState.Energy, Owner);
			}
		}

		protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			return Task.CompletedTask;
		}

		protected override void OnUpgrade()
		{
		}
	}
}
