using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class BrokenPartKanako : SanaeRelicModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

		public override RelicRarity Rarity => RelicRarity.Event;

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner)
			{
				return;
			}

			CardModel? card = ToolBox.CreateRandomKanakoCard(Owner);
			List<Creature> targets = Owner.Creature.CombatState?.HittableEnemies
				.Where(enemy => enemy.Monster?.IntendsToAttack != true)
				.ToList() ?? [];

			if (card == null && targets.Count == 0)
			{
				return;
			}

			Flash();
			if (card != null)
			{
				await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
			}

			foreach (Creature enemy in targets)
			{
				await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, 2, Owner.Creature, null);
			}
		}
	}
}
