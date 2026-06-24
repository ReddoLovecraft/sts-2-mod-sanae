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
	public sealed class GodWindRaiseFromMountain : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];
		protected override bool IsPlayable => base.IsPlayable
			&& (!IsMutable
				|| !Owner.Creature.HasPower<WindStatePower>()
				|| Owner.Creature.HasPower<SelfishMikoPower>()
				|| Owner.Creature.HasPower<SelfishMikoDamagePower>());

		protected override bool ShouldGlowGoldInternal => !Owner.Creature.HasPower<WindStatePower>();

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(12)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WindPower>(), Tools.GetStaticKeyword("WindState"), Tools.GetStaticKeyword("WindSummon")];

		public GodWindRaiseFromMountain() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			ToolBox.playWindSfx(DynamicVars.Cards.IntValue,new Color("f0d46279"));
			if (ToolBox.IsWindControl(Owner.Creature, DynamicVars.Cards.IntValue))
			{
				await ToolBox.SummonWind(choiceContext, Owner.Creature);
			}

			await PowerCmd.Apply<WindPower>(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1);
		}
	}
}
