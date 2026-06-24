using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using CardTargetType = MegaCrit.Sts2.Core.Entities.Cards.TargetType;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class SeaOpening : YCCardModel
	{
		public override bool GainsBlock => true;
		private const int _beliefAmount = 6;

		public override int YC_count
		{
			get => CurrentUpgradeLevel == 0 ? 1 : 2;
			set { }
		}

		public override CardTargetType TargetType => CurrentUpgradeLevel == 1 ? CardTargetType.AllEnemies : CardTargetType.AnyEnemy;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Move), new CardsVar(2)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				RefreshUpgradeExtraHoverTipsIfNeeded();
				List<IHoverTip> tips =
				[
					.. UpgradeExtraHoverTips,
					Tools.GetStaticKeyword("Spellcard"),
					Tools.GetStaticKeyword("Chant")
				];
				switch (CurrentUpgradeLevel)
				{
					case 0:
						tips.Add(HoverTipFactory.FromPower<WeakPower>());
						break;
					case 1:
						tips.Add(HoverTipFactory.FromPower<VulnerablePower>());
						tips.Add(HoverTipFactory.FromPower<BeliefPower>());
						break;
				}
				return tips;
			}
		}

		public SeaOpening() : base(1, CardType.Attack, CardRarity.Rare, CardTargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await base.OnPlay(choiceContext, cardPlay);
			if (!NotYC && await QueueSingleChantWithPreview(choiceContext, YC_count, $"yc-{CurrentUpgradeLevel}", cardPlay.Target))
			{
				return;
			}

			switch (CurrentUpgradeLevel)
			{
				case 2:
					ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
					decimal removedBlock = cardPlay.Target.Block;
					if (removedBlock > 0)
					{
						await CreatureCmd.LoseBlock(cardPlay.Target, removedBlock);
						await DamageCmd.Attack(removedBlock).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
					}
					await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3").FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
					break;
				case 1:
					foreach (var enemy in CombatState?.HittableEnemies.ToList() ?? [])
					{
						if (enemy.Block > 0)
						{
							await CreatureCmd.LoseBlock(enemy, enemy.Block);
						}
						await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, DynamicVars.Cards.IntValue, Owner.Creature, this);
						await PowerCmd.Apply<BeliefPower>(choiceContext, enemy, _beliefAmount, Owner.Creature, this);
					}
					await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3")
			.WithHitVfxSpawnedAtBase().TargetingAllOpponents(CombatState!).Execute(choiceContext);
					break;
				default:
					ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
					if (cardPlay.Target.Block > 0)
					{
						await CreatureCmd.LoseBlock(cardPlay.Target, cardPlay.Target.Block);
					}
					await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3").Targeting(cardPlay.Target).Execute(choiceContext);
					await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, DynamicVars.Cards.IntValue, Owner.Creature, this);
					break;
			}

			NotYC = false;
		}

		protected override void firstUpgrade()
		{
		}

		protected override void secondUpgrade()
		{
			AddKeyword(CardModifier.MiracleKeyword);
			DynamicVars.Damage.UpgradeValueBy(28);
			base.EnergyCost.UpgradeBy(4);
		}
	}
}
