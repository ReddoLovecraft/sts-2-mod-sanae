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
	public sealed class WindLakeTheurgist : SanaeCardModel
	{
		public override bool GainsBlock => true;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2), new BlockVar(5, ValueProp.Move)];

		protected override bool ShouldGlowGoldInternal => 	 (ToolBox.IsWindControl(Owner.Creature, 8) || ToolBox.IsPiety(Owner.Creature, 4));

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>(), Tools.GetStaticKeyword("WindControl"), Tools.GetStaticKeyword("Piety")];

		public WindLakeTheurgist() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
			if (ToolBox.IsWindControl(Owner.Creature, 8))
			{
				await CardPileCmd.Draw(choiceContext, 1, Owner);
			}
			if (ToolBox.IsPiety(Owner.Creature, 4))
			{
				await CardPileCmd.Draw(choiceContext, 1, Owner);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(3);
		}
	}
}
