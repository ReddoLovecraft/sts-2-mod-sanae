using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class PurificationRitual : YCCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain];
		public override int MaxUpgradeLevel => 1;

		public override int YC_count
		{
			get => 1;
			set { }
		}

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				return new List<IHoverTip>
				{
					Tools.GetStaticKeyword("Chant")
				};
			}
		}

		public PurificationRitual() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await base.OnPlay(choiceContext, cardPlay);
			if (!NotYC)
			{
				YCPower? yc = await PowerCmd.Apply<YCPower>(choiceContext, Owner.Creature, YC_count, Owner.Creature, this);
				if (yc != null)
				{
					yc.SetCardAndHoverTip(new YCPreviewCardHoverTip((YCCardModel)CreateDupe(), $"yc-{CurrentUpgradeLevel}"), this);
				}
				return;
			}

			if (Owner.PlayerCombatState != null)
			{
				List<CardModel> cardsToExhaust = Owner.PlayerCombatState.AllCards
					.Where(card => card.Pile?.Type != PileType.Exhaust && (card.Type == CardType.Curse || card.Type == CardType.Status))
					.ToList();
				foreach (CardModel card in cardsToExhaust)
				{
					await CardCmd.Exhaust(choiceContext, card);
				}
			}

			NotYC = false;
		}

		protected override void firstUpgrade()
		{
			base.EnergyCost.UpgradeBy(-1);
			AddKeyword(CardKeyword.Innate);
		}
	}
}


