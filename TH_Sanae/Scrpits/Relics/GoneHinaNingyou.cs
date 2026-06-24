using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.Relics;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class GoneHinaNingyou : SanaeRelicModel
	{
		public override RelicRarity Rarity => RelicRarity.Event;
	}
}
