using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class BrockenPhenomenon : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardModifier.MiracleKeyword, CardKeyword.Exhaust, CardKeyword.Ethereal];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(20)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<InducePower>(), ..HoverTipFactory.FromCardWithCardHoverTips<Hisoutensoku>()];

		public BrockenPhenomenon() : base(5, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool fromEndOfTurn)
		{
			await base.AfterCardExhausted(choiceContext, card, fromEndOfTurn);
			if (card == this)
			{
				Hisoutensoku hisoutensoku = Owner.RunState.CreateCard<Hisoutensoku>(Owner);
				await CardPileCmd.AddGeneratedCardToCombat(hisoutensoku, PileType.Discard, Owner, CardPilePosition.Random);
			}
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PowerCmd.Apply<InducePower>(choiceContext, Owner.Creature, DynamicVars["Cards"].IntValue, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			RemoveKeyword(CardKeyword.Ethereal);
		}
	}
}


