using System.Collections.Generic;
using System.Linq;
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
			int livingBefore = CombatState.HittableEnemies.Count();
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState!).Execute(choiceContext);
			int livingAfter = CombatState.HittableEnemies.Count();
			if (livingAfter < livingBefore)
			{
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState!).Execute(choiceContext);
			}

			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(4);
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}


