using System.Collections.Generic;
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
	public sealed class ILikeIt : SanaeCardModel
	{
		public override bool GainsBlock => true;

		protected override bool ShouldGlowGoldInternal => HasPietyRequirementMet;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8, ValueProp.Move), new CardsVar(5)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>(), HoverTipFactory.FromPower<VulnerablePower>(), Tools.GetStaticKeyword("Piety")];

		public ILikeIt() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			if (ToolBox.IsPiety(Owner.Creature, 4))
			{
				await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
			}

			await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature,DynamicVars.Cards.IntValue, Owner.Creature, this);
			await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(4);
			DynamicVars.Cards.UpgradeValueBy(2);
		}

		private bool HasPietyRequirementMet => IsMutable && ToolBox.IsPiety(Owner.Creature, 4);
	}
}
