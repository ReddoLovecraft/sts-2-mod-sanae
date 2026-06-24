using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using TH_Sanae.Scrpits.Cards;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class MilkyPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<SanaeMilk>();

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player)
			{
				return;
			}

			SanaeMilk milk = player.RunState.CreateCard<SanaeMilk>(player);
			await CardPileCmd.AddGeneratedCardToCombat(milk, MegaCrit.Sts2.Core.Entities.Cards.PileType.Hand, player, MegaCrit.Sts2.Core.Entities.Cards.CardPilePosition.Random);
			if (Amount > 1)
			{
				for (int i = 1; i < Amount; i++)
				{
					SanaeMilk extraMilk = player.RunState.CreateCard<SanaeMilk>(player);
					await CardPileCmd.AddGeneratedCardToCombat(extraMilk, MegaCrit.Sts2.Core.Entities.Cards.PileType.Hand, player, MegaCrit.Sts2.Core.Entities.Cards.CardPilePosition.Random);
				}
			}
		}
	}
}
