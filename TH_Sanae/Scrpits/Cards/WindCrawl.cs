using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class WindCrawl : SanaeCardModel
	{
		protected override bool HasEnergyCostX => true;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>(), HoverTipFactory.FromCard<GrassTreeSolider>(IsUpgraded)];

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

		public WindCrawl() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int totalCount = ResolveEnergyXValue();
			if (CurrentUpgradeLevel > 0)
			{
				totalCount++;
			}

			if (totalCount <= 0)
			{
				return;
			}

			foreach (var enemy in CombatState.HittableEnemies)
			{
				await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, totalCount, Owner.Creature, this);
			}

			var cards = new List<MegaCrit.Sts2.Core.Models.CardModel>();
			for (int i = 0; i < totalCount; i++)
			{
				GrassTreeSolider card = Owner.RunState.CreateCard<GrassTreeSolider>(Owner);
				if (CurrentUpgradeLevel > 0)
				{
					card.UpgradeInternal();
					card.FinalizeUpgradeInternal();
				}
				cards.Add(card);
			}

			await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, Owner, CardPilePosition.Random);
		}

		protected override void OnUpgrade()
		{
		}
	}
}
