using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class MoriyaProtection : SanaeCardModel,ITranscendenceCard
	{
		public CardModel GetTranscendenceTransformedCard() => ModelDb.Card<MoriyaTwoGodBlessing>();
		protected override bool ShouldGlowRedInternal => GetPreviousPlayedCardType() == CardType.Attack;
		protected override bool ShouldGlowGreenInternal => GetPreviousPlayedCardType() != null && GetPreviousPlayedCardType() != CardType.Attack;

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move), new BlockVar(12, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move), new CardsVar(1), new EnergyVar(1)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				List<IHoverTip> tips =
				[
					HoverTipFactory.FromPower<WeakPower>(),
					HoverTipFactory.FromPower<VulnerablePower>()
				];
				if (IsUpgraded)
				{
					tips.Add(HoverTipFactory.ForEnergy(this));
				}
				return tips;
			}
		}

		public MoriyaProtection() : base(2, CardType.Skill, CardRarity.Basic, TargetType.AnyEnemy)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			

			CardType? previousCardType = GetPreviousPlayedCardType();
			if (previousCardType == null)
			{
				return;
			}
			if (previousCardType != CardType.Attack)
			{
				await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target,DynamicVars.Cards.IntValue, Owner.Creature, this);
				await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
				return;
			}
			if (IsUpgraded)
			{
				await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
			}
			await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, 2, Owner.Creature, this);
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitFx("vfx/vfx_attack_blunt").Targeting(cardPlay.Target).Execute(choiceContext);
		}

		protected override void OnUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(4);
			DynamicVars.Block.UpgradeValueBy(4);
			DynamicVars.Cards.UpgradeValueBy(1);
		}

		private CardType? GetPreviousPlayedCardType()
		{
			if (CombatState == null)
			{
				return null;
			}

			CardPlayFinishedEntry? previousPlay = CombatManager.Instance.History.CardPlaysFinished
				.Where(entry => entry.CardPlay.Card.Owner == Owner && entry.CardPlay.Card != this)
				.LastOrDefault();
			return previousPlay?.CardPlay.Card.Type;
		}
	}
}
