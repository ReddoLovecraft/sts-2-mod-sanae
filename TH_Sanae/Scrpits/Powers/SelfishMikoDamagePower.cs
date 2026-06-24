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
	public sealed class SelfishMikoDamagePower : SanaePowerModel
	{
		private bool _skipFirstTrigger = true;

		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;

		public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

		public override LocString Description => ToolBox.GetCustomText("powers", Id.Entry, ".description");

		protected override void DeepCloneFields()
		{
			base.DeepCloneFields();
			_skipFirstTrigger = true;
		}

		public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			modifiedCost = originalCost;
			if (card.Owner.Creature != Owner)
			{
				return false;
			}

			if (card.EnergyCost.CostsX)
			{
				return false;
			}

			switch (card.Pile?.Type)
			{
				case PileType.Hand:
				case PileType.Play:
					modifiedCost = 0m;
					return true;
				default:
					return false;
			}
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
			await CreatureCmd.Damage(choiceContext, Owner, 2, ValueProp.Unpowered, Owner, null);
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
