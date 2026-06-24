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
	public sealed class OmikujiSummonPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Omikuji>();

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player)
			{
				return;
			}

			for (int i = 0; i < Amount; i++)
			{
				Omikuji omikuji = player.RunState.CreateCard<Omikuji>(player);
				await CardPileCmd.AddGeneratedCardToCombat(omikuji, PileType.Hand, player, CardPilePosition.Random);
			}
		}
	}
}
