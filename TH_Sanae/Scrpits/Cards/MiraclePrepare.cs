using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class MiraclePrepare : YCCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardModifier.MiracleKeyword];
		protected override bool ShouldGlowGoldInternal => CurrentUpgradeLevel == 2 && ToolBox.IsPiety(Owner.Creature, 50);
		public override int YC_count
		{
			get => CurrentUpgradeLevel switch
			{
				0 => 1,
				1 => 2,
				_ => 3
			};
			set { }
		}

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(4), new EnergyVar(3)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				RefreshUpgradeExtraHoverTipsIfNeeded();
				List<IHoverTip> tips = new List<IHoverTip>(UpgradeExtraHoverTips)
				{
					Tools.GetStaticKeyword("Spellcard"),
					Tools.GetStaticKeyword("Chant")
				};
				switch (CurrentUpgradeLevel)
				{
					case 0:
						tips.Add(HoverTipFactory.ForEnergy(this));
						tips.Add(HoverTipFactory.FromPower<BeliefPower>());
						break;
					case 1:
						tips.Add(HoverTipFactory.FromPower<WindPower>());
						break;
					default:
						tips.Add(HoverTipFactory.FromPower<BeliefPower>());
						tips.Add(HoverTipFactory.FromPower<InducePower>());
						break;
				}
				tips.Add(Tools.GetStaticKeyword("Piety"));
				return tips;
			}
		}

		public MiraclePrepare() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await base.OnPlay(choiceContext, cardPlay);
			if (!NotYC)
			{
				YCPower yc = await PowerCmd.Apply<YCPower>(choiceContext, Owner.Creature, YC_count, Owner.Creature, this);
				yc.SetCardAndHoverTip(new YCPreviewCardHoverTip((YCCardModel)CreateDupe(), $"yc-{CurrentUpgradeLevel}"), this);
				return;
			}

			switch (CurrentUpgradeLevel)
			{
				case 2:
					await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, DynamicVars["Cards"].IntValue, Owner.Creature, this);
					foreach (Creature enemy in CombatState.HittableEnemies.ToList())
					{
						await PowerCmd.Apply<InducePower>(choiceContext, enemy, 10, Owner.Creature, this);
					}
					if (ToolBox.IsPiety(Owner.Creature, 50))
					{
						var kanakoCards = new List<MegaCrit.Sts2.Core.Models.CardModel>();
						for (int i = 0; i < 5; i++)
						{
							MegaCrit.Sts2.Core.Models.CardModel? card = ToolBox.CreateRandomKanakoCard(Owner, upgraded: true);
							if (card == null)
							{
								continue;
							}
							card.EnergyCost.SetThisTurn(0);
							kanakoCards.Add(card);
						}
						if (kanakoCards.Count > 0)
						{
							await CardPileCmd.AddGeneratedCardsToCombat(kanakoCards, PileType.Hand, Owner, CardPilePosition.Random);
						}
					}
					break;
				case 1:
					int stage = ToolBox.RollOmikuji(Owner.Creature) switch
					{
						"大吉" => 2,
						"吉" => 1,
						_ => 0
					};
					MiracleWind miracleWind = Owner.RunState.CreateCard<MiracleWind>(Owner);
					for (int i = 0; i < stage; i++)
					{
						miracleWind.UpgradeInternal();
						miracleWind.FinalizeUpgradeInternal();
					}
					miracleWind.NotYC = false;
					miracleWind.EnergyCost.SetThisTurn(0);
					await CardPileCmd.AddGeneratedCardToCombat(miracleWind, PileType.Hand, Owner, CardPilePosition.Random);
					await PowerCmd.Apply<WindPower>(choiceContext, Owner.Creature, DynamicVars["Cards"].IntValue, Owner.Creature, this);
					break;
				default:
					await CardPileCmd.Draw(choiceContext, 2, Owner);
					await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
					await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, DynamicVars["Cards"].IntValue, Owner.Creature, this);
					break;
			}

			NotYC = false;
		}

		protected override void firstUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(8);
			base.EnergyCost.UpgradeBy(1);
		}

		protected override void secondUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(12);
			base.EnergyCost.UpgradeBy(-1);
		}
	}
}
