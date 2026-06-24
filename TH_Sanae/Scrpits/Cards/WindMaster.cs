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
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class WindMaster : YCCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardModifier.WindStepKeyword];
		public override int YC_count
		{
			get => CurrentUpgradeLevel >= 1 ? 1 : 2;
			set { }
		}

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				RefreshUpgradeExtraHoverTipsIfNeeded();
				return new List<IHoverTip>(UpgradeExtraHoverTips)
				{
					Tools.GetStaticKeyword("Spellcard"),
					Tools.GetStaticKeyword("Chant"),
					HoverTipFactory.FromPower<WindPower>()
				};
			}
		}

		public WindMaster() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
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
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await ToolBox.DoubleWind(choiceContext, Owner.Creature);
			if (Owner.Creature.HasPower<WindPower>())
			{
				int damage = Owner.Creature.GetPowerAmount<WindPower>();
				for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
				{
					await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, damage, ValueProp.Unpowered, Owner.Creature, this);
				}
			}
			NotYC = false;
		}

		protected override void firstUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
		}

		protected override void secondUpgrade()
		{
			this.EnergyCost.UpgradeBy(-1);
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}


