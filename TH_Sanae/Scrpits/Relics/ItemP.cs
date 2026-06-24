using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class ItemP : SanaeRelicModel, IRightCilckable
	{
		private const string _percentKey = "Percent";
		private int _count;

		public override RelicRarity Rarity => RelicRarity.Shop;

		public override bool ShowCounter => true;

		public override int DisplayAmount => Count;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar(_percentKey, 0m)];

		[SavedProperty]
		public int Count
		{
			get => _count;
			set
			{
				AssertMutable();
				_count = int.Clamp(value, 0, 5);
				DynamicVars[_percentKey].BaseValue = _count * 50;
				InvokeDisplayAmountChanged();
				RefreshStatus();
			}
		}

		public override Task AfterRoomEntered(AbstractRoom room)
		{
			RefreshStatus(room is CombatRoom);
			return Task.CompletedTask;
		}

		public override Task BeforeCombatStart()
		{
			RefreshStatus(inCombat: true);
			return Task.CompletedTask;
		}

		public override Task AfterCombatEnd(CombatRoom room)
		{
			RefreshStatus();
			return Task.CompletedTask;
		}

		public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (Count <= 0 || cardSource == null || !props.IsPoweredAttack() || !cardSource.Tags.Contains(CardTag.Strike))
			{
				return 1m;
			}

			if (dealer != Owner.Creature && cardSource.Owner != Owner)
			{
				return 1m;
			}

			return 1m + (Count * 0.5m);
		}

		public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target == Owner.Creature && result.UnblockedDamage > 0 && Count > 0)
			{
				Count /= 2;
			}

			return Task.CompletedTask;
		}

		public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
		{
			if (wasRemovalPrevented || creature.Side == Owner.Creature.Side || !creature.IsDead)
			{
				return Task.CompletedTask;
			}

			if (Owner.RunState.CurrentRoom is not CombatRoom room)
			{
				return Task.CompletedTask;
			}

			if (room.RoomType == RoomType.Monster && creature.HasPower<MinionPower>())
			{
				return Task.CompletedTask;
			}

			int gain = room.RoomType switch
			{
				RoomType.Boss => 3,
				RoomType.Elite => 2,
				_ => 1
			};

			if (gain <= 0)
			{
				return Task.CompletedTask;
			}

			Flash();
			Count += gain;
			return Task.CompletedTask;
		}

		public async Task OnRightClick(PlayerChoiceContext context)
		{
			if (Count <= 0 || Owner.Creature.CombatState == null || Owner.Creature.CombatState.CurrentSide != Owner.Creature.Side)
			{
				return;
			}

			Flash();
			await CreatureCmd.Damage(context, Owner.Creature.CombatState.HittableEnemies, 10, ValueProp.Unpowered, Owner.Creature, null);
			Count -= 1;
			RefreshStatus(inCombat: true);
		}

		private void RefreshStatus(bool inCombat = false)
		{
			if (Count <= 0)
			{
				Status = RelicStatus.Normal;
			}
			else if (inCombat)
			{
				Status = RelicStatus.Active;
			}
			else
			{
				Status = RelicStatus.Normal;
			}
		}
	}
}
