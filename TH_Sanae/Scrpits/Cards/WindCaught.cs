using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class WindCaught : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
		protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(6)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromPower<WindPower>()];

		public WindCaught() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			foreach (Creature mos in CombatState.HittableEnemies.ToList())
			{
				ToolBox.playWindSfx(DynamicVars.Cards.IntValue, new Color("FFFFFF80"));
				//ToolBox.playWindSfx( DynamicVars.Cards.IntValue, new Color("9fdfff56"));
				await PowerCmd.Apply<PiercingWailPower>(choiceContext, mos, DynamicVars.Cards.IntValue, Owner.Creature, this);
				await PowerCmd.Apply<WindPower>(choiceContext, Owner.Creature, DynamicVars.Cards.IntValue, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
			RemoveKeyword(CardKeyword.Exhaust);
		}
	}
}


