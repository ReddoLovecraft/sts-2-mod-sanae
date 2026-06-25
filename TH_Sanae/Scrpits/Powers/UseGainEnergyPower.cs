using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class UseGainEnergyPower : SanaePowerModel
	{
		private bool _skipFirstTrigger = true;

		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/UGEP32.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/UGEP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

		public override LocString Description => ToolBox.GetCustomText("powers", Id.Entry, ".description");

		public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Card.Owner != Owner.Player)
			{
				return;
			}

			if (_skipFirstTrigger)
			{
				_skipFirstTrigger = false;
				return;
			}

			if (cardPlay.Card.EnergyCost.Canonical == 0)
			{
				return;
			}

			await PlayerCmd.GainEnergy(Amount, Owner.Player);
		}
	}
}
