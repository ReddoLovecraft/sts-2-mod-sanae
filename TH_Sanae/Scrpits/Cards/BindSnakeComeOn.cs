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
	public sealed class BindSnakeComeOn : YCCardModel
	{
		public override int YC_count
		{
			get => 1;
			set { }
		}

		public override bool GainsBlock => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move), new CardsVar(4)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				RefreshUpgradeExtraHoverTipsIfNeeded();
				return new List<IHoverTip>(UpgradeExtraHoverTips)
				{
					Tools.GetStaticKeyword("Spellcard"),
					Tools.GetStaticKeyword("Chant"),
					HoverTipFactory.FromPower<ConstrictPower>()
				};
			}
		}

		public BindSnakeComeOn() : base(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await base.OnPlay(choiceContext, cardPlay);
			if (!NotYC)
			{
				YCPower? yc = await PowerCmd.Apply<YCPower>(choiceContext, Owner.Creature, YC_count, Owner.Creature, this);
				yc?.SetCardAndHoverTip(new YCPreviewCardHoverTip((YCCardModel)CreateDupe(), $"yc-{CurrentUpgradeLevel}"), this);
				return;
			}

			if (cardPlay.Target != null)
			{
				await PowerCmd.Apply<ConstrictPower>(choiceContext, cardPlay.Target, DynamicVars["Cards"].IntValue, Owner.Creature, this);
			}

			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
			NotYC = false;
		}

		protected override void firstUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(4);
		}

		protected override void secondUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
			DynamicVars.Block.UpgradeValueBy(3);
		}
	}
}
