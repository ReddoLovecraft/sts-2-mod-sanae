using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
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
	public sealed class FantasyTrafficLight : SanaeCardModel
	{
		protected override bool ShouldGlowGoldInternal => GetPreviousPlayedCardType() == CardType.Power;

		protected override bool ShouldGlowRedInternal => GetPreviousPlayedCardType() == CardType.Attack;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2), new EnergyVar(2)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<DexterityPower>(),
			HoverTipFactory.FromPower<StrengthPower>(),
			HoverTipFactory.ForEnergy(this)
		];

		public FantasyTrafficLight() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			switch (GetPreviousPlayedCardType())
			{
				case CardType.Skill:
					await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, DynamicVars["Cards"].IntValue, Owner.Creature, this);
					break;
				case CardType.Attack:
					await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars["Cards"].IntValue, Owner.Creature, this);
					break;
				case CardType.Power:
					await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
					break;
			}
		}

		protected override void OnUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1);
		}

		private CardType? GetPreviousPlayedCardType()
		{
			if (CombatState == null)
			{
				return null;
			}

			CardPlayFinishedEntry? previousPlay = CombatManager.Instance.History.CardPlaysFinished
				.Where(entry => entry.CardPlay.Card.Owner == Owner && entry.CardPlay.Card != this)
				.LastOrDefault();
			return previousPlay?.CardPlay.Card.Type;
		}
	}
}
