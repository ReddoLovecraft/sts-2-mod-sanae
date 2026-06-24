using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
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
}
