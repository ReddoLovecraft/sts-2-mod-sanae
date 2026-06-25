using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
	using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
	using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class TH10Power : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/TH32.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/TH64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust), HoverTipFactory.FromKeyword(CardKeyword.Ethereal), Tools.GetStaticKeyword("Spellcard")];

		public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player != Owner.Player)
			{
				return;
			}

			for (int i = 0; i < Amount; i++)
			{
				if (ToolBox.CreateRandomSanaeSpellCard(player) is not CardModel card)
				{
					continue;
				}

				card.AddKeyword(CardKeyword.Ethereal);
				card.AddKeyword(CardKeyword.Exhaust);
				card.EnergyCost.SetThisTurn(0);
				card.EnergyCost.SetUntilPlayed(0);
				await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player, CardPilePosition.Random);
			}
		}
	}
}
