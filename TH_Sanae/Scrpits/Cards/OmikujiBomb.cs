using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class OmikujiBomb : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardModifier.DrawKeyword];

		private string _omikujiResult = string.Empty;

		protected override bool ShouldGlowGoldInternal => _omikujiResult == "大吉";

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>()];

		public OmikujiBomb() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromStartOfTurn)
		{
			await base.AfterCardDrawn(choiceContext, card, fromStartOfTurn);
			if (card == this)
			{
				ApplyOmikujiResult();
			}
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState!).Execute(choiceContext);
			if (IsUpgraded && Owner.Creature.HasPower<BeliefPower>())
			{
				await CreatureCmd.GainBlock(Owner.Creature, Owner.Creature.GetPowerAmount<BeliefPower>(), MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null);
			}

			if (_omikujiResult != "大吉")
			{
				await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.Damage.BaseValue, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
		}

		private void ApplyOmikujiResult()
		{
			_omikujiResult = ToolBox.RollOmikuji(Owner.Creature);
			DynamicVars.Damage.BaseValue = _omikujiResult switch
			{
				"大吉" => 40,
				"大凶" => 40,
				"吉" => 20,
				"凶" => 10,
				_ => DynamicVars.Damage.BaseValue
			};
			DynamicVars.RecalculateForUpgradeOrEnchant();
		}
	}
}
