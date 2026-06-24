using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class YesGiveTom : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

		public YesGiveTom() : base(4, CardType.Power, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			if (IsUpgraded)
			{
				await PowerCmd.Apply<UseGainEnergyAllCardsPower>(choiceContext, Owner.Creature, DynamicVars.Energy.IntValue, Owner.Creature, this);
				return;
			}

			await PowerCmd.Apply<UseGainEnergyPower>(choiceContext, Owner.Creature, DynamicVars.Energy.IntValue, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1);
		}
	}
}

