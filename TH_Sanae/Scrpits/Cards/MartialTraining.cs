using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(ColorlessCardPool))]
	public sealed class MartialTraining : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(8)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VigorPower>(), HoverTipFactory.FromPower<StrengthPower>()];

		public MartialTraining() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this);

			int vigorAmount = Owner.Creature.GetPowerAmount<VigorPower>();
			if (vigorAmount > 0)
			{
				await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, vigorAmount, Owner.Creature, this);
				if (!IsUpgraded)
				{
					await PowerCmd.Remove<VigorPower>(Owner.Creature);
				}
			}
		}

		protected override void OnUpgrade()
		{
		}
	}
}


