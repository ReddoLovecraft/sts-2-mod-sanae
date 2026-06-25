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
public sealed class MoriyaTwoGodBlessingPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;
		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/MTGBP32.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/MTGBP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromPower<DexterityPower>()];

		public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Card.Owner != Owner.Player || !cardPlay.IsFirstInSeries)
			{
				return;
			}

			if (cardPlay.Card.Type == CardType.Attack)
			{
				Flash();
				await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, cardPlay.Card);
			}
			else if (cardPlay.Card.Type == CardType.Skill)
			{
				Flash();
				await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, Amount, Owner, cardPlay.Card);
			}
		}
	}
}

