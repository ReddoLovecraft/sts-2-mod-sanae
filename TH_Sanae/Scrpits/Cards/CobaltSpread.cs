using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class CobaltSpread : SanaeCardModel
	{
		protected override bool ShouldGlowGoldInternal => ToolBox.IsWindControl(Owner.Creature, 8);

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move), new CardsVar(4)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("WindControl")];

		public CobaltSpread() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			decimal damage = DynamicVars.Damage.BaseValue;
			if (ToolBox.IsWindControl(Owner.Creature, 8))
			{
				damage += 3;
			}

			for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
			{
				Creature? enemy = Owner.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
				if (enemy == null)
				{
					return;
				}

				await DamageCmd.Attack(damage).FromCard(this).Targeting(enemy).Execute(choiceContext);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2);
		}
	}
}
