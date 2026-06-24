using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
[Pool(typeof(SanaeCardPool))]
	public sealed class CloudSwimmingSerpent : SanaeCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FlightPower>()];

		public CloudSwimmingSerpent() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			while (true)
			{
				int drawCountBefore = CombatManager.Instance.History.Entries.OfType<CardDrawnEntry>().Count();
				await CardPileCmd.Draw(choiceContext, 1, Owner);
				CardModel? drawnCard = CombatManager.Instance.History.Entries
					.OfType<CardDrawnEntry>()
					.Skip(drawCountBefore)
					.Select(entry => entry.Card)
					.LastOrDefault(card => card.Owner == Owner);
				if (drawnCard == null)
				{
					break;
				}

				await PowerCmd.Apply<FlightPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
				if (drawnCard.Type == CardType.Attack)
				{
					break;
				}
			}
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}
	}
}

