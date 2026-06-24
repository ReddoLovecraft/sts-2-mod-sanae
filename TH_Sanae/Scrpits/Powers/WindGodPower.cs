using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class WindGodPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WindPower>()];

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (!participants.Contains(Owner) || Amount <= 0 || !Owner.HasPower<WindPower>())
			{
				return;
			}

			int windAmount = Owner.GetPowerAmount<WindPower>();
			if (windAmount <= 0)
			{
				return;
			}

			List<Creature> targets = CombatState?.HittableEnemies.ToList() ?? [];
			if (targets.Count == 0)
			{
				return;
			}

			Flash();
			for (int i = 0; i < Amount; i++)
			{
				await CreatureCmd.Damage(choiceContext, targets, windAmount, ValueProp.Unpowered, Owner, null);
			}
		}
	}
}
