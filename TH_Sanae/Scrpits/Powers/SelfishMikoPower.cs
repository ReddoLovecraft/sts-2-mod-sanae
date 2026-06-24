using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class SelfishMikoPower : SanaePowerModel
	{
		private bool _skipFirstTrigger = true;

		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;

		public override LocString Description => ToolBox.GetCustomText("powers", Id.Entry, ".description");

		protected override void DeepCloneFields()
		{
			base.DeepCloneFields();
			_skipFirstTrigger = true;
		}

		public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Card.Owner != Owner.Player)
			{
				return;
			}

			if (_skipFirstTrigger)
			{
				_skipFirstTrigger = false;
				return;
			}

			Flash();
			await CreatureCmd.Damage(choiceContext, Owner, 2, ValueProp.Unpowered | ValueProp.Unblockable, Owner, null);
		}

		public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			if (participants.Contains(Owner))
			{
				await PowerCmd.Remove(this);
			}
		}
	}
}
