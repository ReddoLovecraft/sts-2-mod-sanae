using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
public sealed class SinglePlatingPower : SanaePowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override bool ShouldScaleInMultiplayer => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner) && !base.Owner.IsDead)
		{
			Flash();
			await CreatureCmd.GainBlock(base.Owner, base.Amount, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null);
		}
	}

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, MegaCrit.Sts2.Core.ValueProps.ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == base.Owner && result.UnblockedDamage > 0)
		{
			Flash();
			await PowerCmd.Remove(this);
			await PowerCmd.Apply<NoBlockPower>(choiceContext, base.Owner, 1m, base.Owner, null, silent: true);
		}
	}
}
}
