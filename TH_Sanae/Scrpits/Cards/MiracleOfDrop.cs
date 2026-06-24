using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class MiracleOfDrop : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardModifier.MiracleKeyword];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(27, ValueProp.Move), new CardsVar(5), new EnergyVar(2)];

		protected override bool ShouldGlowGoldInternal => ToolBox.IsPiety(Owner.Creature, 5);

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<InducePower>(), HoverTipFactory.FromPower<BeliefPower>(), HoverTipFactory.ForEnergy(this), Tools.GetStaticKeyword("Piety")];

		public MiracleOfDrop() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PowerCmd.Apply<InducePower>(choiceContext, Owner.Creature, DynamicVars["Cards"].IntValue, Owner.Creature, this);
			if (cardPlay.Target != null)
			{
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
			}

			if (ToolBox.IsPiety(Owner.Creature, 5))
			{
				await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(9);
		}
	}
}
