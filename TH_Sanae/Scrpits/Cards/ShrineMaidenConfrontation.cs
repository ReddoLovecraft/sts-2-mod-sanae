using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class ShrineMaidenConfrontation : SanaeCardModel
	{
		protected override bool IsPlayable => base.IsPlayable
			&& (Owner == null
				|| Owner.Creature.HasPower<SelfishMikoPower>()
				|| Owner.Creature.HasPower<SelfishMikoDamagePower>()
				|| AllEnemiesIntendToAttack());

		protected override bool ShouldGlowGoldInternal => IsMutable
			&& Owner != null
			&& (Owner.Creature.HasPower<SelfishMikoPower>()
				|| Owner.Creature.HasPower<SelfishMikoDamagePower>()
				|| AllEnemiesIntendToAttack());

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(18, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move), new CardsVar(2)];

		public ShrineMaidenConfrontation() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (CombatState == null)
			{
				return;
			}

			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState!).Execute(choiceContext);
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(6);
			DynamicVars.Cards.UpgradeValueBy(1);
		}

		private bool AllEnemiesIntendToAttack()
		{
			if (CombatState == null)
			{
				return false;
			}

			List<MegaCrit.Sts2.Core.Entities.Creatures.Creature> enemies = CombatState.HittableEnemies.ToList();
			return enemies.Count > 0 && enemies.All(enemy => enemy.IsMonster && enemy.Monster?.IntendsToAttack == true);
		}
	}
}
