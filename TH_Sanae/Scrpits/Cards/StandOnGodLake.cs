using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class StandOnGodLake : SanaeCardModel
	{
		protected override bool ShouldGlowGoldInternal => !Owner.Creature.HasPower<WindStatePower>();
		protected override bool ShouldGlowRedInternal => Owner.Creature.HasPower<WindStatePower>();
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(5), new IntVar("Power", 1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips =>
		[
			HoverTipFactory.FromPower<WindPower>(),
			HoverTipFactory.FromPower<StrengthPower>(),
			HoverTipFactory.FromPower<DexterityPower>(),
			Tools.GetStaticKeyword("WindState"),
			Tools.GetStaticKeyword("StopWind"),
			Tools.GetStaticKeyword("WindSummon")
		];

		public StandOnGodLake() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			if (Owner.Creature.HasPower<WindPower>())
			{
				int threshold = DynamicVars.Cards.IntValue;
				int count = threshold > 0 ? Owner.Creature.GetPowerAmount<WindPower>() / threshold : 0;
				if (count > 0)
				{
					await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, count, Owner.Creature, this);
					await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, count, Owner.Creature, this);
				}
			}

			if (Owner.Creature.HasPower<WindStatePower>())
			{
				await ToolBox.StopWind(choiceContext, Owner.Creature);
				return;
			}

			await ToolBox.SummonWind(choiceContext, Owner.Creature);
			await CardPileCmd.Draw(choiceContext, DynamicVars["Power"].IntValue, Owner);
		}

		protected override void OnUpgrade()
		{
			this.AddKeyword(CardKeyword.Retain);
		}
	}
}
