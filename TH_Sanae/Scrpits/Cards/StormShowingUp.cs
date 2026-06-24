using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class StormShowingUp : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
		protected override bool HasEnergyCostX => true;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WindPower>(), Tools.GetStaticKeyword("WindSummon")];

		public StormShowingUp() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await ToolBox.SummonWind(choiceContext, Owner.Creature);
			int hitCount = ResolveEnergyXValue();
			if (IsUpgraded)
			{
				hitCount++;
			}

			if (hitCount <= 0 || !Owner.Creature.HasPower<WindPower>() || CombatState == null)
			{
				return;
			}

			decimal windDamage = Owner.Creature.GetPowerAmount<WindPower>();
			foreach (int _ in Enumerable.Range(0, hitCount))
			{
				ToolBox.playWindSfx((float)windDamage, new Color("FFFFFF80"));
				await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, windDamage, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, Owner.Creature, this);
			}
		}

		protected override void OnUpgrade()
		{
		}
	}
}


