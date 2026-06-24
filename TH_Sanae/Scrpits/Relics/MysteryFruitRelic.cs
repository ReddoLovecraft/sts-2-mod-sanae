using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class MysteryFruitRelic : SanaeRelicModel
	{
		private int _energyAmount = 1;

		public override RelicRarity Rarity => RelicRarity.Event;

		public override bool ShowCounter => true;

		public override int DisplayAmount => EnergyAmount;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

		[SavedProperty]
		public int EnergyAmount
		{
			get => _energyAmount;
			set
			{
				AssertMutable();
				_energyAmount = value;
				if (DynamicVars.ContainsKey("Energy"))
				{
					DynamicVars.Energy.BaseValue = value;
				}
				InvokeDisplayAmountChanged();
			}
		}

		public void AddCounter()
		{
			EnergyAmount += 1;
		}
		public override decimal ModifyMaxEnergy(Player player, decimal amount)
		{
		if (player != base.Owner)
		{
			return amount;
		}
		return amount + EnergyAmount;
		}
	}
}
