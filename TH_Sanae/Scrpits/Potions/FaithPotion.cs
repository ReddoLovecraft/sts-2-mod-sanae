using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Potions
{
	[Pool(typeof(SanaePotionPool))]
	public sealed class FaithPotion : SanaePotionModel
	{
		private const string AmountVarKey = "Amount";

		public override PotionRarity Rarity => PotionRarity.Uncommon;

		public override PotionUsage Usage => PotionUsage.CombatOnly;

		public override TargetType TargetType => TargetType.AnyPlayer;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar(AmountVarKey, 12)];

		public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BeliefPower>()];

		protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
		{
			PotionModel.AssertValidForTargetedPotion(target);
			Creature resolvedTarget = target!;
			NCombatRoom.Instance?.PlaySplashVfx(resolvedTarget, new Color("8fd3ff"));
			await PowerCmd.Apply<BeliefPower>(choiceContext, resolvedTarget, DynamicVars[AmountVarKey].IntValue, Owner.Creature, null);
		}
	}
}
