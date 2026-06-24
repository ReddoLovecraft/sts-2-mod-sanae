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
	public sealed class PersonOfWindSacrifice : SanaeCardModel
	{
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WindPower>(), HoverTipFactory.FromPower<BeliefPower>()];

		public PersonOfWindSacrifice() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int windAmount = Owner.Creature.HasPower<WindPower>() ? Owner.Creature.GetPowerAmount<WindPower>() : 0;
			int faithAmount = windAmount / DynamicVars.Cards.IntValue;
			if (faithAmount > 0)
			{
				await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, faithAmount, Owner.Creature, this);
			}

			if (!IsUpgraded && Owner.Creature.HasPower<WindPower>())
			{
				await PowerCmd.Remove<WindPower>(Owner.Creature);
			}
		}

		protected override void OnUpgrade()
		{
		}
	}
}
