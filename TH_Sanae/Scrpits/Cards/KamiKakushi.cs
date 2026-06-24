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
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(ColorlessCardPool))]
	public sealed class KamiKakushi : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];
		public override int MaxUpgradeLevel => 99;

		public override CardPoolModel VisualCardPool => ModelDb.CardPool<ColorlessCardPool>();

		protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<IntangiblePower>(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IntangiblePower>()];

		public KamiKakushi() : base(1, CardType.Skill, CardRarity.Token, TargetType.Self, showInCardLibrary: false)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature, DynamicVars["IntangiblePower"].BaseValue, Owner.Creature, this);
			PlayerCmd.EndTurn(Owner, canBackOut: false);
		}

		protected override void OnUpgrade()
		{
			DynamicVars["IntangiblePower"].UpgradeValueBy(1);
		}
	}
}


