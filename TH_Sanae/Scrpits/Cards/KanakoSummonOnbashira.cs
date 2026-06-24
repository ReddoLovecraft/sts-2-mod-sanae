using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class KanakoSummonOnbashira : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move), new CardsVar(1)];

		public KanakoSummonOnbashira() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}

			int hitCount = GetCombatHitCount();
			for (int i = 0; i < hitCount; i++)
			{
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3")
			.WithHitVfxSpawnedAtBase().FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
			}
		}

		public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Card.Owner != Owner || cardPlay.Card.Id != Id)
			{
				return Task.CompletedTask;
			}

			SyncAllCombatCopies();

			return Task.CompletedTask;
		}

		public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
		{
			if (card != this)
			{
				return Task.CompletedTask;
			}

			SyncDisplayedHitCount(Pile?.IsCombatPile == true ? GetCombatHitCount() : 1);
			return Task.CompletedTask;
		}

		public override Task BeforeCombatStart()
		{
			SyncDisplayedHitCount(1);
			return Task.CompletedTask;
		}

		public override Task AfterCombatEnd(CombatRoom room)
		{
			SyncDisplayedHitCount(1);
			return Task.CompletedTask;
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2);
		}

		private int GetCombatHitCount()
		{
			if (CombatState == null)
			{
				return 1;
			}

			return 1 + CombatManager.Instance.History.CardPlaysFinished.Count(entry => entry.CardPlay.Card.Owner == Owner && entry.CardPlay.Card.Id == Id);
		}

		private void SyncAllCombatCopies()
		{
			if (CombatState == null)
			{
				return;
			}

			int hitCount = GetCombatHitCount();
			foreach (KanakoSummonOnbashira card in ToolBox.GetAllCombatCards(Owner).OfType<KanakoSummonOnbashira>())
			{
				card.SyncDisplayedHitCount(hitCount);
			}
		}

		private void SyncDisplayedHitCount(int hitCount)
		{
			DynamicVars.Cards.BaseValue = hitCount;
			DynamicVars.RecalculateForUpgradeOrEnchant();
		}
	}
}
