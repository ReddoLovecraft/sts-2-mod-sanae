using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class FaithCaughtPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>()];

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (!participants.Contains(Owner))
			{
				return;
			}

			int totalStolen = 0;
			IEnumerable<Creature> targets = CombatState?.Creatures.Where(creature => creature != Owner && creature.Side != Owner.Side) ?? [];
			foreach (Creature target in targets.ToList())
			{
				if (!target.HasPower<BeliefPower>())
				{
					continue;
				}

				int stolen = target.GetPowerAmount<BeliefPower>() / 2;
				if (stolen <= 0)
				{
					continue;
				}

				BeliefPower? power = target.GetPower<BeliefPower>();
				if (power == null)
				{
					continue;
				}

				await PowerCmd.ModifyAmount(choiceContext, power, -stolen, Owner, null);
				totalStolen += stolen;
			}

			if (totalStolen <= 0)
			{
				return;
			}

			Flash();
			await PowerCmd.Apply<BeliefPower>(choiceContext, Owner, totalStolen, Owner, null);
		}
	}
}
