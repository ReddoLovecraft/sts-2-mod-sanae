using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Patches
{
	[HarmonyPatch(typeof(CardModel), "get_Description")]
	public static class YCCardDescriptionLocPatch
	{
		public static void Postfix(CardModel __instance, ref LocString __result)
		{
			if (__instance is YCCardModel ycCard)
			{
				__result = ycCard.GetDescriptionLocString(ycCard.CurrentUpgradeLevel);
			}
		}
	}

	[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForUpgradePreview))]
	public static class YCCardUpgradePreviewDescriptionPatch
	{
		public static void Postfix(CardModel __instance, ref string __result)
		{
			if (__instance is not YCCardModel ycCard)
			{
				return;
			}

			__result = ycCard.GetDescriptionForPile(PileType.None);
		}
	}
}
