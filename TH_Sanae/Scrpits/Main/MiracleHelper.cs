using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TH_Sanae.Scrpits.Cards;

namespace TH_Sanae.Scripts.Main
{
	public interface IMiracleTriggeredListener
	{
		Task AfterMiracleTriggered(PlayerChoiceContext choiceContext, CardModel sourceCard);
	}

	public static class MiracleHelper
	{
		public static async Task<bool> TryTriggerMiracle(PlayerChoiceContext choiceContext, CardModel sourceCard, int energyAmount)
		{
			if (!ToolBox.RollMiracle(sourceCard.Owner.Creature))
			{
				return false;
			}

			ToolBox.PlayMiracleVfx(sourceCard.Owner);
			if (energyAmount > 0)
			{
				await PlayerCmd.GainEnergy(energyAmount, sourceCard.Owner);
			}

			if (sourceCard.CombatState != null)
			{
				foreach (AbstractModel model in sourceCard.CombatState.IterateHookListeners().OfType<IMiracleTriggeredListener>().Cast<AbstractModel>())
				{
					await ((IMiracleTriggeredListener)model).AfterMiracleTriggered(choiceContext, sourceCard);
					model.InvokeExecutionFinished();
				}
			}

			return true;
		}

		public static Task<bool> TryTriggerMiracle(PlayerChoiceContext choiceContext, CardModel sourceCard)
		{
			return TryTriggerMiracle(choiceContext, sourceCard, Math.Max(0, sourceCard.EnergyCost.Canonical));
		}

		public static CardModel CreateCombatCopyForPlayer(CardModel sourceCard, Player owner)
		{
			CardModel copy = owner.Creature.CombatState!.CreateCard(ModelDb.GetById<CardModel>(sourceCard.Id), owner);
			while (copy.CurrentUpgradeLevel < sourceCard.CurrentUpgradeLevel)
			{
				ToolBox.UpgradeCard(copy);
			}

			foreach (CardKeyword keyword in sourceCard.Keywords)
			{
				if (!copy.Keywords.Contains(keyword))
				{
					copy.AddKeyword(keyword);
				}
			}

			copy.BaseReplayCount = sourceCard.BaseReplayCount;
			return copy;
		}

		public static Creature? ResolveAutoPlayTarget(CardModel card, Creature? preferredTarget)
		{
			if (card.CombatState == null)
			{
				return preferredTarget;
			}

			if (preferredTarget != null && preferredTarget.IsHittable)
			{
				return preferredTarget;
			}

			return card.TargetType switch
			{
				TargetType.AnyEnemy or TargetType.RandomEnemy => card.Owner.RunState.Rng.CombatTargets.NextItem(card.CombatState.HittableEnemies),
				TargetType.Self or TargetType.AnyPlayer => card.Owner.Creature,
				_ => null
			};
		}
	}
}
