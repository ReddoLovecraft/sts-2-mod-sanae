using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.ArtWorks.VFX;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class WindStatePower : SanaePowerModel
	{
		private NWindStateAuraVfx? _vfx;

		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;

		protected override bool IsVisibleInternal => false;

		public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != base.Owner)
			{
				return 1m;
			}

			if (!props.IsPoweredAttack())
			{
				return 1m;
			}

			return 0.25m;
		}

		public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			EnsureVfx();
			if (base.Owner.Player != null)
			{
				await ToolBox.ReturnBerserkWindFromDiscardToHand(base.Owner.Player);
			}
		}

		public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (power == this)
			{
				EnsureVfx();
			}

			return Task.CompletedTask;
		}

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (!participants.Contains(base.Owner))
			{
				return;
			}

			int leastRemainCount = 18;
			if (base.Owner.HasPower<WindGodLakePower>())
			{
				leastRemainCount -= base.Owner.GetPowerAmount<WindGodLakePower>();
			}

			if (base.Owner.GetPowerAmount<WindPower>() >= leastRemainCount)
			{
				return;
			}

			await PowerCmd.Remove(this);
		}

		public override async Task AfterRemoved(Creature oldOwner)
		{
			if (Godot.GodotObject.IsInstanceValid(_vfx))
			{
				_vfx.QueueFree();
			}

			_vfx = null;
			if (oldOwner.Player != null)
			{
				await ToolBox.ReturnBerserkWindFromDiscardToHand(oldOwner.Player);
			}
		}

		private void EnsureVfx()
		{
			if (Godot.GodotObject.IsInstanceValid(_vfx))
			{
				return;
			}

			_vfx = NWindStateAuraVfx.Create(base.Owner);
			base.Owner.GetBackVfxContainer()?.AddChildSafely(_vfx);
		}
	}
}
