using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Powers
{
	public sealed class BoilingBloodPower : SanaePowerModel
	{
		public override PowerType Type => PowerType.Buff;

		public override PowerStackType StackType => PowerStackType.Single;
		public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

		public override string? CustomPackedIconPath => "res://TH_Sanae/ArtWorks/Powers/BBP32.png";
		public override string? CustomBigIconPath => "res://TH_Sanae/ArtWorks/Powers/BBP64.png";

		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VigorPower>()];

		public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			if (target != Owner || result.UnblockedDamage <= 0)
			{
				return;
			}
			Flash();
			await PowerCmd.Apply<VigorPower>(choiceContext, Owner, result.UnblockedDamage, null, null);
		}
	}
}
