using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
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
	public sealed class WindRider : YCCardModel
	{
		public override int YC_count
		{
			get => CurrentUpgradeLevel >= 2 ? 1 : 1;
			set { }
		}

		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				RefreshUpgradeExtraHoverTipsIfNeeded();
				List<IHoverTip> tips = new List<IHoverTip>(UpgradeExtraHoverTips)
				{
					Tools.GetStaticKeyword("Spellcard"),
					Tools.GetStaticKeyword("Chant")
				};
				return tips;
			}
		}

		public WindRider() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await base.OnPlay(choiceContext, cardPlay);
			if (!NotYC)
			{
				await QueueChantWithPreview(choiceContext, YC_count, $"yc-{CurrentUpgradeLevel}");
				return;
			}
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			ToolBox.playWindSfx( DynamicVars.Cards.IntValue, new Color("9fdfff56"));
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
			await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext,Owner.Creature,DynamicVars.Cards.IntValue,Owner.Creature,this);
			NotYC = false;
		}

		protected override void firstUpgrade()
		{
			this.DynamicVars.Cards.UpgradeValueBy(1);
		}

		protected override void secondUpgrade()
		{
			this.EnergyCost.UpgradeBy(-1);
		}
	}
}
