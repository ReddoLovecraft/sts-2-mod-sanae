using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class FrogTalisman : SanaeRelicModel
	{
		protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<OnceArmourPower>()];

		public override RelicRarity Rarity => RelicRarity.Event;

		public override async Task BeforeCombatStart()
		{
			Flash();
			await PowerCmd.Apply<OnceArmourPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 10, Owner.Creature, null);
		}
	}
}
