using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using Patchouib.Scrpits.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Main
{
	public abstract class YCCardModel : SanaeCardModel,IRightClickableCardModel
	{
		private int _upgradeHoverTipCacheLevel = -1;

		public override int MaxUpgradeLevel => 2;

        List<PileType> IRightClickableCardModel.Pile => [PileType.Hand];

		protected override bool ShouldGlowGoldInternal => NotYC;

		protected List<IHoverTip> UpgradeExtraHoverTips = new List<IHoverTip>();

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				RefreshUpgradeExtraHoverTipsIfNeeded();
				return UpgradeExtraHoverTips;
			}
		}

		public override string Title => GetTitleTextForLevel(CurrentUpgradeLevel);

		public override string PortraitPath => GetPortraitPathForLevel(CurrentUpgradeLevel);

		public override IEnumerable<string> AllPortraitPaths
		{
			get
			{
				List<string> allPortraitPaths = new List<string> { base.PortraitPath };
				for (int level = 1; level <= MaxUpgradeLevel; level++)
				{
					string customPath = GetCustomPortraitPath(level);
					if (!string.IsNullOrEmpty(customPath) && ResourceLoader.Exists(customPath) && !allPortraitPaths.Contains(customPath))
					{
						allPortraitPaths.Add(customPath);
					}
				}
				return allPortraitPaths;
			}
		}

        public bool IsCombat => true;

        public bool NotYC;

    	public virtual int YC_count { get; set; }
		public YCCardModel(int baseCost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true, bool autoAdd = true)
	 	: base(baseCost, type, rarity, target, showInCardLibrary)
		{
			if (autoAdd)
			{
				CustomContentDictionary.AddModel(GetType());
			}
		}

		protected override void DeepCloneFields()
		{
			base.DeepCloneFields();
			UpgradeExtraHoverTips = new List<IHoverTip>();
			_upgradeHoverTipCacheLevel = -1;
		}

		protected sealed override void OnUpgrade()
		{
			switch (CurrentUpgradeLevel)
			{
			case 1:
				firstUpgrade();
				break;
			case 2:
				secondUpgrade();
				break;
			}
			_upgradeHoverTipCacheLevel = -1;
		}

		protected virtual void firstUpgrade()
		{
		}

		protected virtual void secondUpgrade()
		{
		}

		public override async Task OnEnqueuePlayVfx(Creature? target)
		{
			if (!NotYC)
			{
				if (Owner.Character is SanaeCharacter)
				{
					await CreatureCmd.TriggerAnim(base.Owner.Creature, "Spell", base.Owner.Character.CastAnimDelay);
				}
				else
				{
					await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
				}
			}
		}

        public async Task OnRightClick(PlayerChoiceContext context)
        {
			if(!NotYC)
			{
				int hpLoss = YC_count * 5;
				if (Owner.Creature.HasPower<ReduceYCHpLosePower>() && Owner.Creature.GetPowerAmount<BeliefPower>() >= 10)
				{
					hpLoss = YC_count;
				}

				await CreatureCmd.Damage(context,Owner.Creature,new DamageVar(hpLoss,ValueProp.Unpowered|ValueProp.Unblockable),this);
				ToolBox.UpgradeCard(this);
				this.NotYC=true;
			}
           
        }

		protected void RefreshUpgradeExtraHoverTipsIfNeeded()
		{
			if (_upgradeHoverTipCacheLevel == CurrentUpgradeLevel)
			{
				return;
			}

			UpgradeExtraHoverTips.Clear();
			for (int targetUpgradeLevel = CurrentUpgradeLevel + 1; targetUpgradeLevel <= MaxUpgradeLevel; targetUpgradeLevel++)
			{
				YCCardModel previewCard = CreateUpgradePreviewCard(targetUpgradeLevel);
				UpgradeExtraHoverTips.Add(new YCPreviewCardHoverTip(previewCard, targetUpgradeLevel.ToString()));
			}
			_upgradeHoverTipCacheLevel = CurrentUpgradeLevel;
		}

		internal YCCardModel CreateUpgradePreviewCard(int targetUpgradeLevel)
		{
			YCCardModel previewCard = (YCCardModel)ModelDb.GetById<CardModel>(Id).ToMutable();
			for (int currentLevel = 0; currentLevel < targetUpgradeLevel; currentLevel++)
			{
				previewCard.UpgradeInternal();
				previewCard.FinalizeUpgradeInternal();
			}
			return previewCard;
		}

		internal string GetTitleTextForLevel(int level)
		{
			if (level <= 0)
			{
				return TitleLocString.GetFormattedText();
			}

			string postfix = level == 1 ? ".upgradetitle" : ".upgrade2title";
			LocString locString = ToolBox.GetCustomText("cards", Id.Entry, postfix);
			return locString.Exists() ? locString.GetFormattedText() : TitleLocString.GetFormattedText();
		}

		internal LocString GetDescriptionLocString(int level)
		{
			string postfix = level switch
			{
				<= 0 => ".description",
				1 => ".upgradedescription",
				_ => ".upgrade2description"
			};
			LocString locString = ToolBox.GetCustomText("cards", Id.Entry, postfix);
			if (locString.Exists())
			{
				return locString;
			}
			if (level >= 2)
			{
				LocString fallbackUpgrade = ToolBox.GetCustomText("cards", Id.Entry, ".upgradedescription");
				if (fallbackUpgrade.Exists())
				{
					return fallbackUpgrade;
				}
			}
			return new LocString("cards", $"{Id.Entry}.description");
		}

		private string GetPortraitPathForLevel(int level)
		{
			if (level <= 0)
			{
				return base.PortraitPath;
			}

			string customPath = GetCustomPortraitPath(level);
			return !string.IsNullOrEmpty(customPath) && ResourceLoader.Exists(customPath) ? customPath : base.PortraitPath;
		}

		protected virtual string GetCustomPortraitPath(int level)
		{
			return $"res://TH_Sanae/ArtWorks/Cards/{Id.Entry}_upgrade{level}.png";
		}
    }
}
