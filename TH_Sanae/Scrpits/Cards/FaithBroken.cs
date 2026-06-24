using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(CurseCardPool))]
	public sealed class FaithBroken : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];
		protected override bool IsPlayable => false;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>()];

		public FaithBroken() : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
		{
		}

		protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
		{
			await base.OnTurnEndInHand(choiceContext);
			if (Owner.Creature.HasPower<BeliefPower>())
			{
				await PowerCmd.Remove<BeliefPower>(Owner.Creature);
			}
		}

		protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			return Task.CompletedTask;
		}

		protected override void OnUpgrade()
		{
		}
	}
}
