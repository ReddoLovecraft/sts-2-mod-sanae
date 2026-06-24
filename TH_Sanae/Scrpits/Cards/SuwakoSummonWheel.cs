using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class SuwakoSummonWheel : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move), new CardsVar(2)];

		public SuwakoSummonWheel() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
			{
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitFx("vfx/vfx_cross_heal").Targeting(cardPlay.Target).Execute(choiceContext);
			}
		}

		protected override PileType GetResultPileTypeForCardPlay()
	{
		PileType resultPileTypeForCardPlay = base.GetResultPileTypeForCardPlay();
		if (resultPileTypeForCardPlay != PileType.Discard)
		{
			return resultPileTypeForCardPlay;
		}
		return PileType.Draw;
	}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
