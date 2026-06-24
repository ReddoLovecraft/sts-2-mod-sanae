using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(ColorlessCardPool))]
	public sealed class GodPowerLight : SanaeCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>()];

		public GodPowerLight() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int beliefAmount = Owner.Creature.GetPowerAmount<BeliefPower>();
			if (beliefAmount <= 0)
			{
				return;
			}

			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
			List<Creature> enemies = CombatState?.HittableEnemies.ToList() ?? [];
			foreach (Creature enemy in enemies)
			{
				int hpLoss = IsUpgraded ? int.Max(1, (int)(beliefAmount * enemy.MaxHp * 0.01m)) : beliefAmount;
				await CreatureCmd.Damage(choiceContext, enemy, hpLoss, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
		}
	}
}


