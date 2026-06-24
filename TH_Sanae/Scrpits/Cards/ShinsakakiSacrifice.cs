using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class ShinsakakiSacrifice : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2), new EnergyVar(2)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [..HoverTipFactory.FromCardWithCardHoverTips<Congratulation>(), HoverTipFactory.ForEnergy(this)];

		public ShinsakakiSacrifice() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			IReadOnlyList<CardModel> handCards = PileType.Hand.GetPile(Owner).Cards.ToList();
			if (handCards.Count > 0)
			{
				CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1);
				CardModel? selectedCard = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this)).FirstOrDefault();
				if (selectedCard != null)
				{
					if (selectedCard is Congratulation)
					{
						await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
					}

					await CardCmd.Exhaust(choiceContext, selectedCard);
				}
			}

			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
		}

		protected override void OnUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1);
		}
	}
}
