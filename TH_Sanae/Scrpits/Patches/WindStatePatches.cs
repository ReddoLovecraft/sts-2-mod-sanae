using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using TH_Sanae.Scripts.Main;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Patches
{
	[HarmonyPatch(typeof(Creature), "get_HoverTips")]
	public static class WindStateCreatureHoverTipPatch
	{
		public static void Postfix(Creature __instance, ref IEnumerable<IHoverTip> __result)
		{
			if (!__instance.HasPower<WindStatePower>())
			{
				return;
			}

			List<IHoverTip> tips = __result.ToList();
			tips.Add(new HoverTip(ToolBox.L10NStatic("WIND_STATE.title"), ToolBox.L10NStatic("WIND_STATE.description")));
			__result = IHoverTip.RemoveDupes(tips).ToList();
		}
	}
}
