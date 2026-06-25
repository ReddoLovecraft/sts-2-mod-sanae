using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class InducePower : SanaePowerModel
	{
		private const string _tenPercentAmountKey = "TenPercentAmount";

		public override PowerType Type => PowerType.Debuff;

		public override PowerStackType StackType => PowerStackType.Counter;

		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/QY32.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/QY64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>()];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar(_tenPercentAmountKey, 0m)];

		public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (dealer != Owner || !props.IsPoweredAttack())
			{
				return 1m;
			}

			decimal modifier = 1m - (Amount * 0.1m);
			return modifier < 0m ? 0m : modifier;
		}

		public override Task AfterApplied(Creature? applier, CardModel? cardSource)
		{
			RefreshDisplayVars();
			return Task.CompletedTask;
		}

		public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
		{
			if (power == this)
			{
				RefreshDisplayVars();
			}
			return Task.CompletedTask;
		}

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (!participants.Contains(Owner))
			{
				return;
			}

			Flash();
			await PowerCmd.Apply<BeliefPower>(choiceContext, Owner, Amount, Owner, null);
			int nextAmount = Amount / 2;
			if (nextAmount <= 0)
			{
				await PowerCmd.Remove(this);
				return;
			}

			await PowerCmd.ModifyAmount(choiceContext, this, nextAmount - Amount, null, null);
		}

		private void RefreshDisplayVars()
		{
			base.DynamicVars[_tenPercentAmountKey].BaseValue = Amount * 10;
			InvokeDisplayAmountChanged();
		}
	}
}
