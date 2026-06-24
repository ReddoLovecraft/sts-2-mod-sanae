using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class ReduceYCHpLosePower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("Devotee"), Tools.GetStaticKeyword("Chant")];
	}
}
