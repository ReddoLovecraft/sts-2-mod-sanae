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
	public sealed class FullMoriyaShrine : SanaeRelicModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

		public override RelicRarity Rarity => RelicRarity.Event;

		public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
		{
			return card.Owner == Owner && card.Type == CardType.Attack ? playCount + 1 : playCount;
		}

		public override Task AfterModifyingCardPlayCount(CardModel card)
		{
			Flash();
			return Task.CompletedTask;
		}

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner)
			{
				return;
			}

			List<CardModel> cards = [];
			for (int i = 0; i < 2; i++)
			{
				CardModel? card = ToolBox.CreateRandomKanakoCard(Owner);
				if (card == null)
				{
					continue;
				}

				card.EnergyCost.SetThisTurn(0);
				cards.Add(card);
			}

			List<Creature> targets = Owner.Creature.CombatState?.HittableEnemies
				.Where(enemy => enemy.Monster?.IntendsToAttack != true)
				.ToList() ?? [];

			if (cards.Count == 0 && targets.Count == 0)
			{
				return;
			}

			Flash();
			foreach (CardModel card in cards)
			{
				await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
			}

			foreach (Creature enemy in targets)
			{
				await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, 3, Owner.Creature, null);
			}
		}
	}
}
