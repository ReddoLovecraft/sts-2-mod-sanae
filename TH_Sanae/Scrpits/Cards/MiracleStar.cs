using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class MiracleStar : YCCardModel
	{
		public override bool GainsBlock => true;
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardModifier.MiracleKeyword];
		public override int YC_count
		{
			get => CurrentUpgradeLevel switch
			{
				0 => 2,
				1 => 1,
				_ => 3
			};
			set { }
		}

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move), new CardsVar(8)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				RefreshUpgradeExtraHoverTipsIfNeeded();
				return new List<IHoverTip>(UpgradeExtraHoverTips)
				{
					Tools.GetStaticKeyword("Spellcard"),
					Tools.GetStaticKeyword("Chant"),
					HoverTipFactory.FromPower<BeliefPower>(),
					HoverTipFactory.FromPower<InducePower>(),
					HoverTipFactory.FromPower<VulnerablePower>()
				};
			}
		}

		public MiracleStar() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await base.OnPlay(choiceContext, cardPlay);
			if (!NotYC && await QueueSingleChantWithPreview(choiceContext, YC_count, $"yc-{CurrentUpgradeLevel}"))
			{
				return;
			}

			switch (CurrentUpgradeLevel)
			{
				case 2:
					int totalHpLoss = (await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitFx("vfx/vfx_starry_impact")
					.SpawningHitVfxOnEachCreature().TargetingAllOpponents(CombatState!).Execute(choiceContext))
						.Results.SelectMany(results => results)
						.Sum(result => result.TotalDamage + result.OverkillDamage);
					if (totalHpLoss > 0)
					{
						await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, totalHpLoss, Owner.Creature, this);
					}
					break;
				case 1:
					await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
					foreach (Creature enemy in CombatState.HittableEnemies.ToList())
					{
						await CreatureCmd.Stun(enemy);
						ToolBox.PlayMiracleVfx(enemy,StsColors.transparentWhite,true);
						await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, 2, Owner.Creature, this);
					}
					await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
					break;
				default:
					for (int i = 0; i < 3; i++)
					{
						List<Creature> enemies = CombatState.HittableEnemies.ToList();
						if (enemies.Count == 0)
						{
							break;
						}

						Creature enemy = enemies[Owner.RunState.Rng.CombatCardGeneration.NextInt(enemies.Count)];
						int hpLoss = (await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitFx("vfx/vfx_starry_impact").Targeting(enemy).Execute(choiceContext))
							.Results.SelectMany(results => results)
							.Sum(result => result.TotalDamage + result.OverkillDamage);
						if (hpLoss > 0)
						{
							await PowerCmd.Apply<InducePower>(choiceContext, enemy, hpLoss, Owner.Creature, this);
						}
					}
					await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
					break;
			}

			NotYC = false;
		}

		protected override void firstUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
			base.EnergyCost.UpgradeBy(1);
		}

		protected override void secondUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(18);
			base.EnergyCost.UpgradeBy(2);
		}
	}
}
