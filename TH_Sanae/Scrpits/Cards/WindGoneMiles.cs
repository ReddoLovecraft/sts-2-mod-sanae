using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class WindGoneMiles : SanaeCardModel
	{
		protected override bool ShouldGlowGoldInternal => Owner.Creature.HasPower<WindStatePower>();
		protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Power", 4), new CardsVar(2), new IntVar("ExtraCards", 1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WindPower>(), Tools.GetStaticKeyword("WindState")];

		public WindGoneMiles() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			ToolBox.playWindSfx(DynamicVars["Power"].IntValue, new Color("FFFFFF80"));
			await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
			await PowerCmd.Apply<WindPower>(choiceContext, Owner.Creature, DynamicVars["Power"].IntValue, Owner.Creature, this);
			if (Owner.Creature.HasPower<WindStatePower>())
			{
				await CardPileCmd.Draw(choiceContext, DynamicVars["ExtraCards"].IntValue, Owner);
			}
		}

		protected override void OnUpgrade()
		{
			DynamicVars["Power"].UpgradeValueBy(3);
			DynamicVars["ExtraCards"].UpgradeValueBy(1);
		}
	}
}
