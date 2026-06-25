using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class CatFormPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;

		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/CFP32.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/CFP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("WindSummon"), HoverTipFactory.FromPower<WindPower>(), HoverTipFactory.Static(StaticHoverTip.Block)];

		public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			if (player == Owner.Player)
			{
				Tools.Talk("早喵~", Owner);
			}

			return Task.CompletedTask;
		}

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (!participants.Contains(Owner))
			{
				return;
			}

			await ToolBox.SummonWind(choiceContext, Owner);
			if (Owner.HasPower<WindPower>())
			{
				await CreatureCmd.GainBlock(Owner, Owner.GetPowerAmount<WindPower>(), ValueProp.Unpowered, null);
			}
		}
	}
}
