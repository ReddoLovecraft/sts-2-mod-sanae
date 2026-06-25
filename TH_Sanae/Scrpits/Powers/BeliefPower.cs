using System.Collections.Generic;
using System.Linq;
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
public sealed class BeliefPower : SanaePowerModel
{
	
	private const string _doubleAmountKey = "DoubleAmount";

	private const string _tenPercentAmountKey = "TenPercentAmount";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/XY32.png";
	public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/XY64.png";

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar(_doubleAmountKey, 0m), new IntVar(_tenPercentAmountKey, 0m)];

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (!participants.Contains(base.Owner) || base.Owner.IsDead)
		{
			return;
		}

		decimal currentBlock = base.Owner.Block;
		decimal targetBlock = GetDoubleAmount();

		if (currentBlock < targetBlock)
		{
			Flash();
			await CreatureCmd.GainBlock(base.Owner, targetBlock - currentBlock, ValueProp.Unpowered, null);
		}
		else
		{
			decimal bonusBlock = GetTenPercentAmount(currentBlock);
			if (bonusBlock > 0m)
			{
				Flash();
				await CreatureCmd.GainBlock(base.Owner, bonusBlock, ValueProp.Unpowered, null);
			}
		}

		RefreshDisplayVars();
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		RefreshDisplayVars();
		return TriggerBandAidHeal();
	}

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (power == this)
		{
			RefreshDisplayVars();
			if (amount > 0m && amount != base.Amount)
			{
				await TriggerBandAidHeal();
			}
		}
	}

	public override Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
	{
		if (creature == base.Owner)
		{
			RefreshDisplayVars();
		}
		return Task.CompletedTask;
	}

	public override Task AfterBlockCleared(Creature creature)
	{
		if (creature == base.Owner)
		{
			RefreshDisplayVars();
		}
		return Task.CompletedTask;
	}

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner)
		{
			RefreshDisplayVars();
		}
		return Task.CompletedTask;
	}

	private decimal GetDoubleAmount()
	{
		return base.Amount * 2m;
	}

	private decimal GetTenPercentAmount(decimal currentBlock)
	{
		return decimal.Floor(currentBlock * GetDoubleAmount() * 0.1m);
	}

	private void RefreshDisplayVars()
	{
		base.DynamicVars[_doubleAmountKey].BaseValue = GetDoubleAmount();
		base.DynamicVars[_tenPercentAmountKey].BaseValue = GetTenPercentAmount(base.Owner.Block);
		InvokeDisplayAmountChanged();
	}

	private async Task TriggerBandAidHeal()
	{
		if (base.Owner.Player?.GetRelic<BandAid>() == null)
		{
			return;
		}

		await CreatureCmd.Heal(base.Owner, 2m);
	}
}
}
