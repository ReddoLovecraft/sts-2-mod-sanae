using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class ShrineMaidenPrayer : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Power", 3), new CardsVar(4)];

		protected override bool ShouldGlowGoldInternal => ToolBox.IsPiety(Owner.Creature, 8);

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>(), Tools.GetStaticKeyword("Piety"), ..HoverTipFactory.FromCardWithCardHoverTips<Congratulation>()];

		public ShrineMaidenPrayer() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
			List<MegaCrit.Sts2.Core.Models.CardModel> cards = Enumerable.Range(0, DynamicVars["Power"].IntValue)
				.Select(_ => (MegaCrit.Sts2.Core.Models.CardModel)CombatState!.CreateCard<Congratulation>(Owner))
				.ToList();
			CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Draw, Owner, CardPilePosition.Random));

			if (ToolBox.IsPiety(Owner.Creature, 8))
			{
				await ToolBox.UpgradeCardsInHand(Owner);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars["Power"].UpgradeValueBy(2);
			DynamicVars.Cards.UpgradeValueBy(1);
		}
	}
}
