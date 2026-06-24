using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(StatusCardPool))]
	public sealed class WindEndGrass : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Ethereal];
		protected override bool IsPlayable => false;

		protected override IEnumerable<MegaCrit.Sts2.Core.HoverTips.IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("StopWind")];

		public WindEndGrass() : base(-2, CardType.Status, CardRarity.Common, TargetType.None, showInCardLibrary: false)
		{
		}

		public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromStartOfTurn)
		{
			await base.AfterCardDrawn(choiceContext, card, fromStartOfTurn);
			if (card == this)
			{
				int windAmount = Owner.Creature.GetPowerAmount<TH_Sanae.Scripts.Powers.WindPower>();
				if (windAmount > 0)
				{
					await CreatureCmd.Damage(choiceContext, Owner.Creature, windAmount, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered | MegaCrit.Sts2.Core.ValueProps.ValueProp.Unblockable, null, this);
				}
				await ToolBox.StopWind(choiceContext, Owner.Creature);
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


