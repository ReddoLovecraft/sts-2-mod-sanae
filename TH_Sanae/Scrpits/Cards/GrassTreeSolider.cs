using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(StatusCardPool))]
	public sealed class GrassTreeSolider : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, ValueProp.Move)];

		public GrassTreeSolider() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}
			CardPile? hand = ToolBox.GetPile(Owner, PileType.Hand);
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitCount(hand.Cards.Count).WithHitFx("vfx/vfx_attack_slash",null,"slash_attack.mp3").Targeting(cardPlay.Target).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(3);
		}
	}
}
