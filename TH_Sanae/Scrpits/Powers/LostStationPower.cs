using System.Threading.Tasks;
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
	public sealed class LostStationPower : SanaePowerModel
	{
		private bool _pendingIntangible;

		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;

		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/LSP32.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/LSP64.png";

			protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>(), HoverTipFactory.FromPower<IntangiblePower>()];

		public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
		{
			if (target == base.Owner && amount > 0m && canonicalPower is BeliefPower)
			{
				modifiedAmount = 0m;
				_pendingIntangible = true;
				return true;
			}

			modifiedAmount = amount;
			return false;
		}

		public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
		{
			if (!_pendingIntangible || base.Owner == null || power is not BeliefPower)
			{
				return;
			}

			_pendingIntangible = false;
			await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), base.Owner, 1m, null, null);
		}
	}
}
