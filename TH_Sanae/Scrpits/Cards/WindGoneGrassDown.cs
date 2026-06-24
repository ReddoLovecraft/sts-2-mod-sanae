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
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class WindGoneGrassDown : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardModifier.WindStepKeyword];
		protected override bool IsPlayable => base.IsPlayable && (!IsMutable || Owner.Creature.HasPower<WindStatePower>());

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move), new CardsVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [];

		public WindGoneGrassDown() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			Color color = new Color("9fdfff56");
			double num2 = ((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.2 : 0.3);
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NHorizontalLinesVfx.Create(color, 0.8 + (double)Mathf.Min(8, this.DynamicVars.Damage.IntValue) * num2));
			SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_whirlwind");
			NRun.Instance?.GlobalUi.AddChildSafely(NSmokyVignetteVfx.Create(color, color));
			int livingBefore = CombatState.HittableEnemies.Count();
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_giant_horizontal_slash").Execute(choiceContext);
			int livingAfter = CombatState.HittableEnemies.Count();
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
			if (livingAfter < livingBefore)
			{
				await CardCmd.AutoPlay(choiceContext, this,null);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(4);
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}


