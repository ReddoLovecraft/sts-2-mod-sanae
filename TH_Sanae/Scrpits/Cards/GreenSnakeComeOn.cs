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
	public sealed class GreenSnakeComeOn : YCCardModel
	{
		public override int YC_count
		{
			get => CurrentUpgradeLevel >= 2 ? 1 : 2;
			set { }
		}

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move), new CardsVar(4)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				RefreshUpgradeExtraHoverTipsIfNeeded();
				return new List<IHoverTip>(UpgradeExtraHoverTips)
				{
					Tools.GetStaticKeyword("Spellcard"),
					Tools.GetStaticKeyword("Chant"),
					HoverTipFactory.FromPower<StrengthPower>()
				};
			}
		}

		public GreenSnakeComeOn() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
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

			if (cardPlay.Target != null)
			{
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
			}
			await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars["Cards"].IntValue, Owner.Creature, this);
			await PowerCmd.Apply<TemporaryStrengthPower>(choiceContext, Owner.Creature, DynamicVars["Cards"].IntValue, Owner.Creature, this);
			NotYC = false;
		}

		protected override void firstUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(4);
			DynamicVars.Cards.UpgradeValueBy(2);
		}

		protected override void secondUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(4);
			DynamicVars.Cards.UpgradeValueBy(2);
		}
	}
}
