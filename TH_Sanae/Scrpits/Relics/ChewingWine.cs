using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using Patchouib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class ChewingWine : SanaeRelicModel, IRightCilckable
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
		private int _charges = 2;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(3)];

		public override RelicRarity Rarity => RelicRarity.Common;

		public override bool ShowCounter => true;

		public override int DisplayAmount => Charges;

		[SavedProperty]
		public int Charges
		{
			get => _charges;
			set
			{
				AssertMutable();
				_charges = value < 0 ? 0 : value;
				InvokeDisplayAmountChanged();
				RefreshStatus();
			}
		}

		public override Task AfterRoomEntered(AbstractRoom room)
		{
			if (room is RestSiteRoom)
			{
				Charges += 1;
			}
			else
			{
				RefreshStatus(room is CombatRoom);
			}

			return Task.CompletedTask;
		}

		public override Task BeforeCombatStart()
		{
			RefreshStatus(inCombat: true);
			return Task.CompletedTask;
		}

		public override Task AfterCombatEnd(CombatRoom room)
		{
			RefreshStatus();
			return Task.CompletedTask;
		}

		public async Task OnRightClick(PlayerChoiceContext context)
		{
			if (Charges <= 0 || Owner.Creature.CombatState == null || Owner.Creature.CombatState.CurrentSide != Owner.Creature.Side)
			{
				return;
			}

			Flash();
			await PlayerCmd.GainEnergy(3, Owner);
			Charges -= 1;
			RefreshStatus(inCombat: true);
		}

		private void RefreshStatus(bool inCombat = false)
		{
			if (Charges <= 0)
			{
				Status = RelicStatus.Disabled;
			}
			else if (inCombat)
			{
				Status = RelicStatus.Active;
			}
			else
			{
				Status = RelicStatus.Normal;
			}
		}
	}
}
