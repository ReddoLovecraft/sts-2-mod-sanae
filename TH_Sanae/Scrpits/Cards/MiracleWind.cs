using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class MiracleWind : YCCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardModifier.MiracleKeyword, CardKeyword.Ethereal, CardKeyword.Exhaust];
		public override int YC_count
		{
			get => CurrentUpgradeLevel switch
			{
				0 => 3,
				1 => 4,
				_ => 8
			};
			set { }
		}

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(1, ValueProp.Move), new CardsVar(10), new EnergyVar(2)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				RefreshUpgradeExtraHoverTipsIfNeeded();
				List<IHoverTip> tips = new List<IHoverTip>(UpgradeExtraHoverTips)
				{
					Tools.GetStaticKeyword("Spellcard"),
					Tools.GetStaticKeyword("Chant"),
					Tools.GetStaticKeyword("WindSummon"),
					HoverTipFactory.FromPower<WindPower>()
				};
				if (CurrentUpgradeLevel == 1)
				{
					tips.Add(HoverTipFactory.ForEnergy(this));
				}
				return tips;
			}
		}

		public MiracleWind() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await base.OnPlay(choiceContext, cardPlay);
			if (!NotYC)
			{
				for (int i = 1; i <= YC_count; i++)
				{
					await QueueChantWithPreview(choiceContext, i, $"yc-{CurrentUpgradeLevel}-{i}");
				}
				return;
			}
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await ToolBox.SummonWind(choiceContext, Owner.Creature);
			switch (CurrentUpgradeLevel)
			{
				case 2:
					int windAmount = Owner.Creature.GetPowerAmount<WindPower>();
					ToolBox.playWindSfx(DynamicVars.Cards.IntValue,new Color("f0d46279"));
					await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitCount(windAmount).WithHitFx("vfx/vfx_attack_slash").TargetingAllOpponents(CombatState!).Execute(choiceContext);
					break;
				case 1:
					await PowerCmd.Apply<WindPower>(choiceContext, Owner.Creature,DynamicVars.Cards.IntValue, Owner.Creature, this);
					await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
					break;
				default:
					await PowerCmd.Apply<WindPower>(choiceContext, Owner.Creature,DynamicVars.Cards.IntValue, Owner.Creature, this);
					break;
			}

			NotYC = false;
		}

		protected override void firstUpgrade()
		{
			RemoveKeyword(CardKeyword.Ethereal);
			DynamicVars.Cards.UpgradeValueBy(2);
			base.EnergyCost.UpgradeBy(1);
		}

		protected override void secondUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(12);
			base.EnergyCost.UpgradeBy(6);
		}
	}
}
