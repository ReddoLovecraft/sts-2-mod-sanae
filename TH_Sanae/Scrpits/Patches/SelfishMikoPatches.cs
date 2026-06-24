using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using TH_Sanae.Scripts.Powers;

namespace TH_Sanae.Scripts.Patches
{
	[HarmonyPatch(typeof(CardModel), "get_IsPlayable")]
	public static class SelfishMikoIsPlayablePatch
	{
		public static void Postfix(CardModel __instance, ref bool __result)
		{
			if (__result)
			{
				return;
			}

			Player? owner = __instance.Owner;
			if (owner?.Creature == null)
			{
				return;
			}

			if (owner.Creature.HasPower<SelfishMikoPower>() || owner.Creature.HasPower<SelfishMikoDamagePower>())
			{
				__result = true;
			}
		}
	}

	[HarmonyPatch]
	public static class SelfishMikoCanPlayPatch
	{
		public static MethodBase? TargetMethod()
		{
			return AccessTools.Method(typeof(CardModel), nameof(CardModel.CanPlay), [typeof(UnplayableReason).MakeByRefType(), typeof(AbstractModel).MakeByRefType()]);
		}

		public static void Postfix(CardModel __instance, ref bool __result, ref UnplayableReason reason, ref AbstractModel? preventer)
		{
			if (!SelfishMikoPlayHelper.HasSelfishMikoOverride(__instance))
			{
				return;
			}

			__result = true;
			reason = UnplayableReason.None;
			preventer = null;
		}
	}

	internal static class SelfishMikoPlayHelper
	{
		internal static bool HasSelfishMikoOverride(CardModel card)
		{
			Player? owner = card.Owner;
			if (owner?.Creature == null)
			{
				return false;
			}

			return owner.Creature.HasPower<SelfishMikoPower>() || owner.Creature.HasPower<SelfishMikoDamagePower>();
		}
	}
}
