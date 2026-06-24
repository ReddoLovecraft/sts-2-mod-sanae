using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class MasterWindPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("WindState"), HoverTipFactory.FromPower<WindPower>()];

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (!participants.Contains(Owner))
			{
				return;
			}

			int leastRemainCount = 18;
			if (Owner.HasPower<WindGodLakePower>())
			{
				leastRemainCount -= Owner.GetPowerAmount<WindGodLakePower>();
			}

			if (Owner.GetPowerAmount<WindPower>() >= leastRemainCount && !Owner.HasPower<WindStatePower>())
			{
				await PowerCmd.Apply<WindStatePower>(choiceContext, Owner, 1, Owner, null);
			}
		}
	}
}
