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
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class Reservation : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<BeliefPower>(), HoverTipFactory.FromPower<InducePower>(), Tools.GetStaticKeyword("Persuasion")];

		public Reservation() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
			await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, 5, Owner.Creature, this);

			foreach (var enemy in (CombatState?.HittableEnemies ?? []).ToList())
			{
				await PowerCmd.Apply<InducePower>(choiceContext, enemy, DynamicVars["Cards"].IntValue, Owner.Creature, this);
				await ToolBox.Persuasion(Owner.Creature, enemy);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
