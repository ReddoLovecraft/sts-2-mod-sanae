using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
[Pool(typeof(SanaeCardPool))]
	public sealed class MiracleGenesis : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardModifier.MiracleKeyword)];

		public MiracleGenesis() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
			CardPile? handPile = PileType.Hand.GetPile(Owner);
			if (handPile == null)
			{
				return;
			}

			foreach (CardModel card in handPile.Cards.Where(card => !card.Keywords.Contains(CardModifier.MiracleKeyword)).ToList())
			{
				card.AddKeyword(CardModifier.MiracleKeyword);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Block.UpgradeValueBy(4);
		}
	}
}

