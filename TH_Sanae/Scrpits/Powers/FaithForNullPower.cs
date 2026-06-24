using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class FaithForNullPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IntangiblePower>(), HoverTipFactory.FromPower<MinionPower>(), Tools.GetStaticKeyword("Devotee")];

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, System.Collections.Generic.IEnumerable<Creature> participants)
		{
			if (!participants.Contains(Owner))
			{
				return;
			}

			bool allDevotees = CombatState.HittableEnemies
				.Where(enemy => !enemy.HasPower<MinionPower>())
				.All(ToolBox.IsDevotee);
			if (!allDevotees)
			{
				return;
			}

			await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner, 1, Owner, null);
		}
	}
}
