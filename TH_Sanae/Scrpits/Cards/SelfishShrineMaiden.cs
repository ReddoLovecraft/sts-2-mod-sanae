using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class SelfishShrineMaiden : SanaeCardModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [];

		public SelfishShrineMaiden() : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			Tools.Talk("在这个幻想乡，可不能按常理出牌！", Owner.Creature);
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			if (IsUpgraded)
			{
				await PowerCmd.Apply<SelfishMikoDamagePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
				return;
			}

			await PowerCmd.Apply<SelfishMikoPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
		}

		protected override void OnUpgrade()
		{
		}
	}
}

