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
	public sealed class CollectFaith : SanaeCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>()];

		public CollectFaith() : base(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Target is not { } target || !target.HasPower<BeliefPower>())
			{
				return;
			}

			int totalFaith = target.GetPowerAmount<BeliefPower>();
			await PowerCmd.Remove<BeliefPower>(target);
			await CreatureCmd.Damage(choiceContext, target, totalFaith, ValueProp.Unblockable, Owner.Creature, this);
			if (IsUpgraded)
			{
				await PowerCmd.Apply<BeliefPower>(choiceContext, Owner.Creature, totalFaith, Owner.Creature, this);
			}
		}
	}
}
