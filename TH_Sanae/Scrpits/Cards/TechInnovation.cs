using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(ColorlessCardPool))]
	public sealed class TechInnovation : SanaeCardModel
	{
		public TechInnovation() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

			foreach (CardModel card in ToolBox.GetPile(Owner, PileType.Hand)?.Cards.ToList() ?? [])
			{
				if (!card.IsUpgradable || card.DeckVersion?.IsUpgradable != true)
				{
					continue;
				}

				ToolBox.UpgradeCard(card);
				ToolBox.UpgradeCard(card.DeckVersion);
			}
		}

		protected override void OnUpgrade()
		{
			EnergyCost.UpgradeBy(-1);
		}
	}
}


