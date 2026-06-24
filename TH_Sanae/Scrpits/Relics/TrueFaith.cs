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
	public sealed class TrueFaith : SanaeRelicModel
	{
		private int _healAmount;

		public override RelicRarity Rarity => RelicRarity.Rare;

		public override bool ShowCounter => true;

		public override int DisplayAmount => HealAmount;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(0m)];

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

		public override async Task AfterObtained()
		{
			FakeFaith? oldFaith = Owner.GetRelic<FakeFaith>();
			if (oldFaith != null)
			{
				HealAmount = oldFaith.HealAmount;
				await RelicCmd.Remove(oldFaith);
			}
		}

		public override async Task AfterPlayerTurnStart(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext choiceContext, Player player)
		{
			if (player == Owner && HealAmount > 0)
			{
				Flash();
				await CreatureCmd.Heal(Owner.Creature, HealAmount);
			}
		}

		public override async Task AfterSideTurnEnd(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Combat.CombatSide side, System.Collections.Generic.IEnumerable<MegaCrit.Sts2.Core.Entities.Creatures.Creature> participants)
		{
			if (participants.Contains(Owner.Creature) && HealAmount > 0)
			{
				Flash();
				await CreatureCmd.Heal(Owner.Creature, HealAmount);
			}
		}

		public override Task AfterCombatVictory(CombatRoom room)
		{
			if (room.RoomType is RoomType.Elite or RoomType.Boss)
			{
				HealAmount += 3;
			}
			return Task.CompletedTask;
		}
	}
}
