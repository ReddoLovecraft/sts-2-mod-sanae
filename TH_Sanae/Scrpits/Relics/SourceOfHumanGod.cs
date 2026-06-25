using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class SourceOfHumanGod : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";

		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<BeliefPower>(),
			HoverTipFactory.FromPower<InducePower>()
		];

		public override RelicRarity Rarity => RelicRarity.Rare;

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner)
			{
				return;
			}

			int triggerCount = 0;
			if (ToolBox.IsDevotee(Owner.Creature))
			{
				triggerCount++;
			}

			foreach (Creature enemy in Owner.Creature.CombatState?.HittableEnemies?.ToList() ?? [])
			{
				if (ToolBox.IsDevotee(enemy))
				{
					triggerCount++;
				}
			}

			for (int i = 0; i < triggerCount; i++)
			{
				Flash();
				await PlayerCmd.GainEnergy(1, Owner);
				await CardPileCmd.Draw(choiceContext, 1,Owner);
			}
		}

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (!participants.Contains(Owner.Creature))
			{
				return;
			}

			bool hasDevotee = ToolBox.IsDevotee(Owner.Creature);
			foreach (Creature enemy in Owner.Creature.CombatState?.HittableEnemies?.ToList() ?? [])
			{
				if (ToolBox.IsDevotee(enemy))
				{
					hasDevotee = true;
				}

				await ToolBox.Persuasion(Owner.Creature, enemy);
			}

			if (hasDevotee)
			{
				return;
			}

			Flash();
			await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
			await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
			await PowerCmd.Apply<FrailPower>(choiceContext, Owner.Creature, 1, Owner.Creature, null);
		}
	}
}
