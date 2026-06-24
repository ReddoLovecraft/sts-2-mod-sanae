using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(ColorlessCardPool))]
	public sealed class ArmyGodForm : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(10)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VigorPower>()];

		public ArmyGodForm() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			await PowerCmd.Apply<ArmyGodFormPower>(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
			EnergyCost.UpgradeBy(-1);
		}
	}
}
