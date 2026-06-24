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
	public sealed class FaithNotNull : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move), new CardsVar(8)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>(), HoverTipFactory.FromPower<InducePower>(), Tools.GetStaticKeyword("Devotee"), Tools.GetStaticKeyword("Persuasion")];

		public FaithNotNull() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}

			if (!ToolBox.IsDevotee(cardPlay.Target))
			{
				int hitCount = Owner.Creature.HasPower<BeliefPower>() ? Owner.Creature.GetPowerAmount<BeliefPower>() : 0;
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitCount(hitCount).WithHitFx("vfx/vfx_starry_impact").Targeting(cardPlay.Target).Execute(choiceContext);
			}
			else
			{
				await PowerCmd.Apply<InducePower>(choiceContext, cardPlay.Target,DynamicVars.Cards.IntValue, Owner.Creature, this);
			}

			if (cardPlay.Target.IsAlive)
			{
				await ToolBox.Persuasion(Owner.Creature, cardPlay.Target);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
			DynamicVars.Damage.UpgradeValueBy(2);
		}
	}
}
