using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using TH_Sanae.Scripts.Events;

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
}
