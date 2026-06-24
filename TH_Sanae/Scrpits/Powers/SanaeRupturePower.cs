using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class SanaeRupturePower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

		public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, MegaCrit.Sts2.Core.ValueProps.ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner || dealer != Owner || cardSource == null || result.UnblockedDamage <= 0 || Owner.Player == null)
			{
				return;
			}

			Flash();
			await PlayerCmd.GainEnergy(Amount, Owner.Player);
			await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
		}
	}
}
