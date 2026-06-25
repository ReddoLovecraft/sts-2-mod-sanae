using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Powers;
using Patchouib.Scrpits.Main;
using SanaeCardModifier = TH_Sanae.Scrpits.Cards.CardModifier;

namespace TH_Sanae.Scripts.Main
{
	public abstract class SanaeCardModel : CustomCardModel//,IRightClickableCardModel
	{
		public bool ShouldGlowGreen => ShouldGlowGreenInternal;

		protected virtual bool ShouldGlowGreenInternal => false;

		public virtual string DefaultPortraitPath => $"res://TH_Sanae/ArtWorks/Cards/{GetType().Name}.png";

		public virtual string NSFWPath => DefaultPortraitPath;

		public override string PortraitPath => SanaeModConfig.NsfwCardArt ? NSFWPath : DefaultPortraitPath;

		protected override bool IsPlayable
		{
			get
			{
				if (!base.IsPlayable)
				{
					return false;
				}

				if (!Keywords.Contains(SanaeCardModifier.WindStepKeyword))
				{
					return true;
				}

				if (!IsMutable || Owner == null)
				{
					return true;
				}

				return Owner.Creature.HasPower<WindStatePower>();
			}
		}
	
		public SanaeCardModel(int baseCost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true, bool autoAdd = true)
	 	: base(baseCost, type, rarity, target, showInCardLibrary)
		{
			if (autoAdd)
			{
				CustomContentDictionary.AddModel(GetType());
			}
		}
	}
  
}
