using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class MysteryFruit : SanaeCardModel
	{
		public override IEnumerable<CardKeyword> CanonicalKeywords => [CardModifier.MiracleKeyword, CardKeyword.Ethereal];
		public override int MaxUpgradeLevel => 0;
		 public override bool CanBeGeneratedInCombat => false;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [.. HoverTipFactory.FromRelic<MysteryFruitRelic>(), Tools.GetStaticKeyword("Remove")];

		public MysteryFruit() : base(4, CardType.Skill, CardRarity.Rare, TargetType.None)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			int healAmount = int.Max(1, (int)(Owner.Creature.MaxHp * 0.25m));
			PlayerFullscreenHealVfx.Play(Owner, healAmount, NCombatRoom.Instance);
			await CreatureCmd.Heal(Owner.Creature, healAmount);
			MysteryFruitRelic? relic = Owner.GetRelic<MysteryFruitRelic>();
			if (relic == null)
			{
				await RelicCmd.Obtain<MysteryFruitRelic>(Owner);
			}
			else
			{
				relic.AddCounter();
			}

			if (DeckVersion != null)
			{
				await CardPileCmd.RemoveFromDeck(DeckVersion);
				DeckVersion = null;
			}
		}

		public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			if (cardPlay.Card == this)
			{
				await CardPileCmd.RemoveFromCombat(this);
			}
		}

		protected override void OnUpgrade()
		{
		}
	}
}
