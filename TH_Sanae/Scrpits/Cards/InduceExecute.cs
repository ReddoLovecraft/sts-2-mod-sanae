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
	public sealed class InduceExecute : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move), new CardsVar(3)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<InducePower>(), HoverTipFactory.FromPower<BeliefPower>(), Tools.GetStaticKeyword("Devotee"), Tools.GetStaticKeyword("Persuasion")];

		public InduceExecute() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}

			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
			if (ToolBox.IsDevotee(cardPlay.Target))
			{
				await PowerCmd.Apply<BeliefPower>(choiceContext, cardPlay.Target, DynamicVars["Cards"].IntValue, Owner.Creature, this);
			}
			else
			{
				await PowerCmd.Apply<InducePower>(choiceContext, cardPlay.Target, DynamicVars["Cards"].IntValue, Owner.Creature, this);
			}

			await ToolBox.Persuasion(Owner.Creature, cardPlay.Target);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2);
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
