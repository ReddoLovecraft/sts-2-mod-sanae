using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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
	public sealed class WindRaiseCloudRise : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(6)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WindPower>(), Tools.GetStaticKeyword("WindSummon")];

		public WindRaiseCloudRise() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			ToolBox.playWindSfx( DynamicVars.Cards.IntValue, new Color("9fdfff56"));
			await PowerCmd.Apply<WindPower>(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
			await ToolBox.SummonWind(choiceContext, Owner.Creature);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(3);
		}
	}
}
