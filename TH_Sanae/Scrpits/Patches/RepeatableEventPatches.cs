using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using TH_Sanae.Main.Ancients;
using TH_Sanae.Scripts.Events;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Patches
{
	[HarmonyPatch(typeof(RunState), nameof(RunState.AddVisitedEvent))]
	public static class RepeatableEventPatches
	{
		public static bool Prefix(EventModel eventModel)
		{
			return eventModel is not MoriyaBranch;
		}
	}

	[HarmonyPatch(typeof(ActModel), nameof(ActModel.RemoveEventFromSet))]
	public static class RepeatableEventWithinActPatches
	{
		public static bool Prefix(EventModel eventModel)
		{
			return eventModel is not MoriyaBranch;
		}
	}

	[HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
	public static class TwoGodProtectionAncientPatches
	{
		public static void Postfix(ActModel __instance, RoomSet ____rooms)
		{
			if (!SanaeModConfig.TwoGodProtection)
			{
				return;
			}

			if (__instance.Index == 1)
			{
				____rooms.Ancient = ModelDb.GetById<AncientEventModel>(ModelDb.GetId(typeof(Suwako)));
				return;
			}

			if (__instance.Index == 2)
			{
				____rooms.Ancient = ModelDb.GetById<AncientEventModel>(ModelDb.GetId(typeof(Kanako)));
			}
		}
	}
}
