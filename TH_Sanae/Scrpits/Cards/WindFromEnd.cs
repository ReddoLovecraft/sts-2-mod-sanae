using System.Collections.Generic;
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
	public sealed class WindFromEnd : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
		protected override bool IsPlayable => base.IsPlayable && (!IsMutable || !Owner.Creature.HasPower<WindStatePower>());

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(4)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WindPower>(), Tools.GetStaticKeyword("WindState"), Tools.GetStaticKeyword("WindSummon"), HoverTipFactory.FromCard<WindEndGrass>()];

		public WindFromEnd() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await ToolBox.SummonWind(choiceContext, Owner.Creature);
			if (Owner.Creature.HasPower<WindPower>())
			{
				int currentWind = Owner.Creature.GetPowerAmount<WindPower>();
				int extraWind = currentWind * (DynamicVars["Cards"].IntValue - 1);
				if (extraWind > 0)
				{
					await PowerCmd.Apply<WindPower>(choiceContext, Owner.Creature, extraWind, Owner.Creature, this);
				}
			}

			WindEndGrass grass = Owner.RunState.CreateCard<WindEndGrass>(Owner);
			await CardPileCmd.AddGeneratedCardToCombat(grass, PileType.Discard, Owner, CardPilePosition.Random);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
			AddKeyword(CardKeyword.Retain);
		}
	}
}
