using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scrpits.Cards;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
public sealed class MiracleHandPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardModifier.MiracleKeyword)];

		public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
		{
			if (card.Owner == Owner.Player && !card.Keywords.Contains(CardModifier.MiracleKeyword))
			{
				Flash();
				card.AddKeyword(CardModifier.MiracleKeyword);
			}

			return Task.CompletedTask;
		}
	}
}

