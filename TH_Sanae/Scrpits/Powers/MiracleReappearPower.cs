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
public sealed class MiracleReappearPower : SanaePowerModel, IMiracleTriggeredListener
	{
		private bool _isResolving;

		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Counter;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardModifier.MiracleKeyword)];

		public async Task AfterMiracleTriggered(PlayerChoiceContext choiceContext, CardModel sourceCard)
		{
			if (_isResolving || sourceCard.Owner != Owner.Player || Owner.CombatState == null || Amount <= 0)
			{
				return;
			}

			_isResolving = true;
			try
			{
				Flash();
				foreach (Player player in Owner.CombatState.Players.Where(static player => player.Creature.IsAlive))
				{
					for (int i = 0; i < Amount; i++)
					{
						CardModel copy = MiracleHelper.CreateCombatCopyForPlayer(sourceCard, player);
						await CardCmd.AutoPlay(choiceContext, copy, MiracleHelper.ResolveAutoPlayTarget(copy, sourceCard.CurrentTarget));
					}
				}
			}
			finally
			{
				_isResolving = false;
			}
		}
	}
}
