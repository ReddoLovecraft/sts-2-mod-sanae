using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class FlowerBirdWindMoonPower : SanaePowerModel
	{
		private const string _phaseKey = "Phase";

		private int _phase = 1;

		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;
		public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/HNFY32.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/HNFY64.png";

		protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar(_phaseKey, "花")];

		public override Task AfterApplied(MegaCrit.Sts2.Core.Entities.Creatures.Creature? applier, MegaCrit.Sts2.Core.Models.CardModel? cardSource)
		{
			RefreshPhaseText();
			return Task.CompletedTask;
		}

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player)
			{
				return;
			}

			switch (_phase)
			{
				case 1:
					await PlayerCmd.GainEnergy(2, player);
					break;
				case 2:
					await PowerCmd.Apply<FlightPower>(choiceContext, Owner, 3, Owner, null);
					break;
				case 3:
					int windAmount = Owner.GetPowerAmount<WindPower>();
					await PowerCmd.Apply<WindPower>(choiceContext, Owner, windAmount > 0 ? windAmount : 10, Owner, null);
					break;
				case 4:
					await CardPileCmd.Draw(choiceContext, 4, player);
					break;
			}

			_phase++;
			if (_phase > 4)
			{
				_phase = 1;
			}

			RefreshPhaseText();
		}

		private void RefreshPhaseText()
		{
			((StringVar)DynamicVars[_phaseKey]).StringValue = _phase switch
			{
				1 => "花",
				2 => "鸟",
				3 => "风",
				_ => "月"
			};
			InvokeDisplayAmountChanged();
		}
	}
}
