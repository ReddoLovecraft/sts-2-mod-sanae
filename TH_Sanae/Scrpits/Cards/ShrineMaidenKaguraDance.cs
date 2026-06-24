using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class ShrineMaidenKaguraDance : SanaeCardModel
	{
		protected override bool ShouldGlowGoldInternal => ToolBox.IsPiety(Owner.Creature, 5);

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>(), Tools.GetStaticKeyword("forseen"), Tools.GetStaticKeyword("Piety")];

		public ShrineMaidenKaguraDance() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
				await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await Tools.Forseen(choiceContext, Owner, DynamicVars.Cards.IntValue);
			await CardPileCmd.Draw(choiceContext, 2, Owner);
			if (!ToolBox.IsPiety(Owner.Creature, 5))
			{
				return;
			}

			CardPile? discardPile = PileType.Discard.GetPile(Owner);
			if (discardPile == null || discardPile.IsEmpty)
			{
				return;
			}

			CardSelectorPrefs returnPrefs = new(ToolBox.GetCustomText("cards", Id.Entry, ".returnSelectionScreenPrompt"), 1);
			CardModel? selectedCard = (await CardSelectCmd.FromCombatPile(choiceContext, discardPile, Owner, returnPrefs)).FirstOrDefault();
			if (selectedCard == null)
			{
				return;
			}

			await CardPileCmd.Add(selectedCard, PileType.Hand, CardPilePosition.Random, selectedCard, false);
			if (IsUpgraded)
			{
				selectedCard.EnergyCost.SetUntilPlayed(0);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
