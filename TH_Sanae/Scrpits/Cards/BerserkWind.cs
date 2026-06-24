using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
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
	public sealed class BerserkWind : SanaeCardModel
	{
		protected override bool ShouldGlowGoldInternal => ToolBox.IsWindControl(Owner.Creature, 10);

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move), new CardsVar(5), new IntVar("Power", 1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WindPower>(), Tools.GetStaticKeyword("WindState"), Tools.GetStaticKeyword("WindControl")];

		public BerserkWind() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (ToolBox.IsWindControl(Owner.Creature, 10))
			{
				DynamicVars["Power"].BaseValue += 1;
			}
			for (int i = 0; i < DynamicVars["Power"].IntValue; i++)
			{
				ToolBox.playWindSfx(DynamicVars["Power"].IntValue, new Color("FFFFFF80"));
				await PowerCmd.Apply<WindPower>(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitFx("vfx/vfx_attack_slash").Targeting(cardPlay.Target).Execute(choiceContext);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars["Power"].UpgradeValueBy(1);
		}
	}
}
