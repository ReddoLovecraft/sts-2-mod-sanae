using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class PrayForMorningWind : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("WindSummon"), HoverTipFactory.ForEnergy(this)];

		public PrayForMorningWind() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			ToolBox.PlayMiracleVfx(Owner.Creature,StsColors.blue,true);
			await ToolBox.SummonWind(choiceContext, Owner.Creature);
			await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Energy.UpgradeValueBy(1);
		}
	}
}


