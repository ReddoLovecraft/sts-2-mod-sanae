using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
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

			for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
			{
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3")
			.WithHitVfxSpawnedAtBase().FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
			}
		}

		public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Card != this)
			{
				return Task.CompletedTask;
			}

			foreach (KanakoSummonOnbashira card in ToolBox.GetAllCombatCards(Owner).OfType<KanakoSummonOnbashira>())
			{
				card.DynamicVars.Cards.BaseValue += 1;
				card.DynamicVars.RecalculateForUpgradeOrEnchant();
			}

			return Task.CompletedTask;
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2);
		}
	}
}
