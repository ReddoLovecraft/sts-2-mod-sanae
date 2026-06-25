using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class OnceThornPower : SanaePowerModel
	{
		private int _useCount;

		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/OT32.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/OT64.png";

		public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Entities.Players.Player player)
		{
			if (player == Owner.Player)
			{
				_useCount = 0;
			}

			return Task.CompletedTask;
		}

		public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner || dealer == null || dealer == Owner || !props.IsPoweredAttack() || result.TotalDamage <= 0 || _useCount >= Amount)
			{
				return;
			}

			_useCount++;
			Flash();
			if (Owner.Player != null)
			{
				await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, 10, ValueProp.Unpowered, Owner, null);
			}
			else
			{
				var playerCreature = CombatState?.Players.FirstOrDefault()?.Creature;
				if (playerCreature != null)
				{
					await CreatureCmd.Damage(choiceContext, playerCreature, 10, ValueProp.Unpowered, Owner, null);
				}
			}
		}
	}
}
