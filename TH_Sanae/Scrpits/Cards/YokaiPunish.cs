using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class YokaiPunish : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, ValueProp.Move), new CardsVar(1)];

		public YokaiPunish() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target == null)
			{
				return;
			}
			int defendCount = CountStarterCards(CardTag.Defend);
			int strikeCount = CountStarterCards(CardTag.Strike);
			decimal damage = DynamicVars.Damage.BaseValue + (IsUpgraded ? 5 : 3) * defendCount;
			int hitCount = DynamicVars.Cards.IntValue + strikeCount;
			await DamageCmd.Attack(damage).WithHitCount(hitCount).FromCard(this).WithHitFx("vfx/vfx_attack_blunt").Targeting(cardPlay.Target).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
		}

		private int CountStarterCards(CardTag tag)
		{
			return PileType.Hand.GetPile(Owner).Cards.Count(card => card.IsBasicStrikeOrDefend && card.Tags.Contains(tag));
		}
	}
}
