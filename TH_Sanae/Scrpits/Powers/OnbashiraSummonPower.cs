using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TH_Sanae.Scrpits.Cards;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class OnbashiraSummonPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/OSP232.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/OSP264.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<KanakoSummonOnbashira>();

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player || CombatState == null || Amount <= 0)
			{
				return;
			}

			List<MegaCrit.Sts2.Core.Models.CardModel> cards =
				Enumerable.Range(0, Amount)
					.Select(_ =>
					{
						KanakoSummonOnbashira card = CombatState.CreateCard<KanakoSummonOnbashira>(player);
						ToolBox.UpgradeCard(card);
						card.AddKeyword(CardKeyword.Ethereal);
						return (MegaCrit.Sts2.Core.Models.CardModel)card;
					})
					.ToList();

			if (cards.Count == 0)
			{
				return;
			}

			Flash();
			await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, player, CardPilePosition.Random);
		}
	}
}
