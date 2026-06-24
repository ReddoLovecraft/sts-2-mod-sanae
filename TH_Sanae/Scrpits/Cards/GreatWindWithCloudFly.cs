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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class GreatWindWithCloudFly : SanaeCardModel
	{
		protected override bool ShouldGlowGoldInternal => ToolBox.IsWindControl(Owner.Creature, 7);

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9, ValueProp.Move), new CardsVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("WindControl"), HoverTipFactory.FromPower<WindPower>()];

		public GreatWindWithCloudFly() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_whirlwind");
			Color color = new Color("4ffa8580");
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NHorizontalLinesVfx.Create(color, 0.8));
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState!)	.WithHitFx("vfx/vfx_giant_horizontal_slash").Execute(choiceContext);
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
			if (ToolBox.IsWindControl(Owner.Creature, 7))
			{
				await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(3);
		}
	}
}
