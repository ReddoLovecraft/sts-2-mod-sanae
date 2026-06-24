using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Patchoulib.Scrpits.Main;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scrpits.Cards
{
	[Pool(typeof(SanaeCardPool))]
	public sealed class NineStab : YCCardModel
	{
		public override int MaxUpgradeLevel => 1;

		public override int YC_count
		{
			get => 3;
			set { }
		}

		protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9, ValueProp.Move), new CardsVar(3)];

		protected override IEnumerable<IHoverTip> ExtraHoverTips
		{
			get
			{
				return [Tools.GetStaticKeyword("Chant")];
			}
		}

		public NineStab() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
		{
		}

		protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			await base.OnPlay(choiceContext, cardPlay);
			if (!NotYC)
			{
				int immediateTriggerCount = await QueueMultiChantWithPreview(
					choiceContext,
					GetChantStages());

				if (immediateTriggerCount <= 0)
				{
					return;
				}

				NotYC = true;
				try
				{
					for (int i = 0; i < immediateTriggerCount; i++)
					{
						await ExecuteResolvedEffect(choiceContext);
					}
				}
				finally
				{
					NotYC = false;
				}

				return;
			}

			await ExecuteResolvedEffect(choiceContext);
			NotYC = false;
		}

		private IEnumerable<(int ChantCount, string PreviewId)> GetChantStages()
		{
			for (int i = 1; i <= YC_count; i++)
			{
				yield return (i, $"yc-{CurrentUpgradeLevel}-{i}");
			}
		}

		private async Task ExecuteResolvedEffect(PlayerChoiceContext choiceContext)
		{
			for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
			{
				ToolBox.playWindSfx(DynamicVars.Cards.IntValue, new Color("f0d46279"));
				await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitFx("vfx/vfx_starry_impact").FromCard(this).TargetingAllOpponents(CombatState!).Execute(choiceContext);
			}
		}

		protected override void firstUpgrade()
		{
			DynamicVars.Damage.UpgradeValueBy(3);
		}
	}
}
