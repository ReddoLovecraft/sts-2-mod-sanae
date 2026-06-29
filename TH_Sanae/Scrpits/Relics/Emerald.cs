using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class Emerald : SanaeRelicModel
	{
		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

		public override RelicRarity Rarity => RelicRarity.Ancient;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

		public void Reset()
		{
			this.DynamicVars.Cards.BaseValue=3;
		}
		public void Add(int amt)
		{
			this.DynamicVars.Cards.BaseValue+=amt;
		}
		  public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
            if (!CombatManager.Instance.IsInProgress||!props.IsPoweredAttack_())
            {
                return;
            }
            if (target == null || target != Owner.Creature)
            {
                return;
            }
            if(dealer==null||dealer==Owner.Creature)
            {
                return;
            }
			this.Flash();
            await CreatureCmd.GainBlock(base.Owner.Creature, new BlockVar(this.DynamicVars.Cards.IntValue,ValueProp.Unpowered), null);
			this.Add(2);
    }
	public override async Task AfterRoomEntered(AbstractRoom _)
	{
		if (!base.Owner.Creature.IsDead)
		{
			Reset();
		}
	}
		//CardCmd.Upgrade(PileType.Hand.GetPile(base.Owner).Cards, CardPreviewStyle.HorizontalLayout);
	}
}
