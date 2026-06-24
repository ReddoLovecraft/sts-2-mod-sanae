using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class WindPersonPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>(), HoverTipFactory.FromPower<WindPower>()];

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player || !Owner.HasPower<BeliefPower>())
			{
				return;
			}

			int faithAmount = Owner.GetPowerAmount<BeliefPower>();
			int windAmount = faithAmount * Amount;
			if (windAmount > 0)
			{
				await PowerCmd.Apply<WindPower>(choiceContext, Owner, windAmount, Owner, null);
			}
		}
	}
}
