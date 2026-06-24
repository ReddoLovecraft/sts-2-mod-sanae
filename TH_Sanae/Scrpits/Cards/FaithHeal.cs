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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class FaithHeal : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
		public override bool CanBeGeneratedInCombat => false;
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(6)];

		protected override bool ShouldGlowGoldInternal => ToolBox.IsPiety(Owner.Creature, 8);

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<OnceHealPower>(), HoverTipFactory.FromPower<BeliefPower>(), Tools.GetStaticKeyword("Piety")];

		public FaithHeal() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			if (ToolBox.IsPiety(Owner.Creature, 8))
			{
				PlayerFullscreenHealVfx.Play(Owner, DynamicVars.Cards.IntValue, NCombatRoom.Instance);
				await PowerCmd.Apply<OnceHealPower>(choiceContext, Owner.Creature,DynamicVars.Cards.IntValue, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(1);
			RemoveKeyword(CardKeyword.Exhaust);
		}
	}
}



