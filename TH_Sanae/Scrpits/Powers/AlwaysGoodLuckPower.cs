using MegaCrit.Sts2.Core.Entities.Powers;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class AlwaysGoodLuckPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;

		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/XRS32.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/XRS64.png";
	}
}
