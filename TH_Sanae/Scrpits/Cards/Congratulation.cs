using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(StatusCardPool))]
	public sealed class Congratulation : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Ethereal];
		protected override bool IsPlayable => false;

		protected override System.Collections.Generic.IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		protected override System.Collections.Generic.IEnumerable<MegaCrit.Sts2.Core.HoverTips.IHoverTip> ExtraHoverTips => [MegaCrit.Sts2.Core.HoverTips.HoverTipFactory.FromPower<BeliefPower>()];

		public Congratulation() : base(-2, CardType.Status, CardRarity.Common, TargetType.None, showInCardLibrary: false)
		{
		}

		public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool fromEndOfTurn)
		{
			await base.AfterCardExhausted(choiceContext, card, fromEndOfTurn);
			if (card == this)
			{
				await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, DynamicVars["Cards"].IntValue, Owner.Creature, this);
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


