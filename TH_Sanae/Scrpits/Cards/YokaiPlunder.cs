using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class YokaiPlunder : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(5)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

		public YokaiPlunder() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			int totalStrengthGain = 0;
			foreach (var enemy in (CombatState?.HittableEnemies ?? []).ToList())
			{
				await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -DynamicVars["Cards"].IntValue, Owner.Creature, this);
				if (enemy.HasPower<ArtifactPower>())
				{
					continue;
				}

				await PowerCmd.Apply<TemporaryStrengthPower>(choiceContext, enemy, DynamicVars["Cards"].IntValue, Owner.Creature, this);
				totalStrengthGain += DynamicVars["Cards"].IntValue;
			}

			if (totalStrengthGain > 0)
			{
				await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, totalStrengthGain, Owner.Creature, this);
				await PowerCmd.Apply<TemporaryStrengthPower>(choiceContext, Owner.Creature, totalStrengthGain, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1);
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
