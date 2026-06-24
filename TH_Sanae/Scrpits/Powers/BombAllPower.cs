using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class BombAllPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (!participants.Contains(Owner))
			{
				return;
			}

			Flash();
			await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, Amount, ValueProp.Unpowered, Owner, null);
			await CreatureCmd.Damage(choiceContext, Owner, Amount, ValueProp.Unpowered, null, null);
		}
	}
}
