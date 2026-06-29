using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class FakeFaith : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";

		private int _healAmount;

		public override RelicRarity Rarity => RelicRarity.Starter;

		public override bool ShowCounter => true;

		public override int DisplayAmount => HealAmount;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(0m)];
		public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<TrueFaith>();

		[SavedProperty]
		public int HealAmount
		{
			get => _healAmount;
			set
			{
				AssertMutable();
				_healAmount = value;
				if (DynamicVars.ContainsKey("Heal"))
				{
					DynamicVars.Heal.BaseValue = value;
				}
				InvokeDisplayAmountChanged();
			}
		}

		public override async Task BeforeCombatStart()
		{
			if (HealAmount > 0)
			{
				Flash();
				await CreatureCmd.Heal(Owner.Creature, HealAmount);
			}
		}

		public override async Task AfterCombatVictory(CombatRoom room)
		{
			if (HealAmount > 0)
			{
				Flash();
				await CreatureCmd.Heal(Owner.Creature, HealAmount);
			}

			if (room.RoomType is RoomType.Elite or RoomType.Boss)
			{
				HealAmount += 3;
			}
		}

		
	}
}
