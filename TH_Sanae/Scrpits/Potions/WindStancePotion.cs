using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Potions
{
	[Pool(typeof(SanaePotionPool))]
	public sealed class WindStancePotion : SanaePotionModel
	{
		public override PotionRarity Rarity => PotionRarity.Uncommon;

		public override PotionUsage Usage => PotionUsage.CombatOnly;

		public override TargetType TargetType => TargetType.AnyPlayer;
		
		public override string? CustomPackedImagePath => "res://TH_Sanae/ArtWorks/Potions/WIND_STANCE_POTION.png";
     	public override string? CustomPackedOutlinePath => "res://TH_Sanae/ArtWorks/Potions/Outlines/WIND_STANCE_POTION.png"; 

		public override IEnumerable<IHoverTip> ExtraHoverTips => [Tools.GetStaticKeyword("WindSummon"), Tools.GetStaticKeyword("WindState")];

		protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
		{
			PotionModel.AssertValidForTargetedPotion(target);
			Creature resolvedTarget = target!;
			NCombatRoom.Instance?.PlaySplashVfx(resolvedTarget, new Color("b7fff0"));
			await ToolBox.SummonWind(choiceContext, resolvedTarget);
		}
	}
}
