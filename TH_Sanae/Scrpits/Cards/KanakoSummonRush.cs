using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class KanakoSummonRush : SanaeCardModel
	{
		 public override bool GainsBlock => true;
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move), new CardsVar(2)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

		public KanakoSummonRush() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			Creature? target = cardPlay.Target;
			if (target == null)
			{
				return;
			}

			if (!(target.IsMonster && target.Monster?.IntendsToAttack == true))
			{
				await PowerCmd.Apply<VulnerablePower>(choiceContext, target, DynamicVars["Cards"].IntValue, Owner.Creature, this);
			}

			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(target).Execute(choiceContext);
			if (target.Block > 0)
			{
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(target).Execute(choiceContext);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2);
		}
	}
}
