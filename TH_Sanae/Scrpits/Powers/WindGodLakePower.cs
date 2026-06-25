using System.Collections.Generic;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class WindGodLakePower : SanaePowerModel
	{
		private const string _tenPercentAmountKey = "TenPercentAmount";

		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/WGLP32.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/WGLP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("WindState"), HoverTipFactory.FromPower<WindPower>()];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar(_tenPercentAmountKey, 0m)];

		public override Task AfterApplied(MegaCrit.Sts2.Core.Entities.Creatures.Creature? applier, CardModel? cardSource)
		{
			RefreshDisplayVars();
			return Task.CompletedTask;
		}

		public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, MegaCrit.Sts2.Core.Entities.Creatures.Creature? applier, CardModel? cardSource)
		{
			if (power == this)
			{
				RefreshDisplayVars();
			}
			return Task.CompletedTask;
		}

		private void RefreshDisplayVars()
		{
			base.DynamicVars[_tenPercentAmountKey].BaseValue = Amount * 10;
			InvokeDisplayAmountChanged();
		}
	}
}
