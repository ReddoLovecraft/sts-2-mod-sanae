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
public sealed class MoriyaMiracleMikoPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;
		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/MMMP32.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/MMMP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardModifier.MiracleKeyword)];

		public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Card.Owner != Owner.Player || !cardPlay.IsFirstInSeries || Amount <= 0)
			{
				return;
			}
			if (!cardPlay.Card.Keywords.Contains(CardModifier.MiracleKeyword))
			{
				return;
			}
			
			Flash();
			for (int i = 0; i < Amount; i++)
			{
				await MiracleHelper.TryTriggerMiracle(choiceContext, cardPlay.Card);
			}
		}
	}
}
