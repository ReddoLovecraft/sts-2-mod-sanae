using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(StatusCardPool))]
	public sealed class SanaeMilk : SanaeCardModel
	{
		public override string NSFWPath => $"res://TH_Sanae/ArtWorks/Cards/NSFW/{GetType().Name}.png";

		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1),new CardsVar(4)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

		public SanaeMilk() : base(0, CardType.Status, CardRarity.Status, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			PlayerFullscreenHealVfx.Play(Owner, DynamicVars.Cards.IntValue, NCombatRoom.Instance);
			await CreatureCmd.Heal(Owner.Creature,  DynamicVars.Cards.IntValue);
			ToolBox.PlayMiracleVfx(Owner);
			await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Cards.UpgradeValueBy(2);
			DynamicVars.Energy.UpgradeValueBy(1);
		}
	}
}


