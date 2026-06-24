using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
[Pool(typeof(SanaeCardPool))]
	public sealed class Starfall : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

		public Starfall() : base(1, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			Creature? target = Owner.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
			if (target == null)
			{
				return;
			}
			VfxCmd.PlayOnCreature(target,"vfx/vfx_starry_impact");
			int strengthLoss = (await CreatureCmd.Damage(choiceContext, target, DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this))
				.Sum(static result => result.TotalDamage);
			if (strengthLoss > 0)
			{
				await PowerCmd.Apply<PiercingWailPower>(choiceContext, target, strengthLoss, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(3);
		}
	}
}

