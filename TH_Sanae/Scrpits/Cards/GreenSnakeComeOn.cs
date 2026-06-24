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
			get => 1;
			set { }
		}

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move), new CardsVar(3)];

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
				await QueueChantWithPreview(choiceContext, YC_count, $"yc-{CurrentUpgradeLevel}", cardPlay.Target);
				return;
			}
			if (cardPlay.Target != null)
			{
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitFx("vfx/vfx_scratch").Targeting(cardPlay.Target).Execute(choiceContext);
			}
			if(CurrentUpgradeLevel<=1)
			{
				await PowerCmd.Apply<FlexPotionPower>(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
			}
			else
			{
				await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
			}
			NotYC = false;
		}

		protected override void firstUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(2);
			DynamicVars.Cards.UpgradeValueBy(2);
		}

		protected override void secondUpgrade()
		{
			
		}
	}
}
