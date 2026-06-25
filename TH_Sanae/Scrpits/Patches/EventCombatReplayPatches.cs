using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace TH_Sanae.Scripts.Patches
{
	[HarmonyPatch(typeof(RunManager), nameof(RunManager.EnterRoomWithoutExitingCurrentRoom))]
	public static class EventCombatReplayInitializationPatch
	{
		public static void Prefix(RunManager __instance, AbstractRoom room)
		{
			if (room is not CombatRoom)
			{
				return;
			}

			if (__instance.CombatReplayWriter is not { IsEnabled: true } replayWriter || replayWriter.IsRecordingReplay)
			{
				return;
			}

			try
			{
				AbstractRoom? currentRoom = Traverse.Create(__instance).Property("State").GetValue<RunState?>()?.CurrentRoom;
				AbstractRoom snapshotRoom = currentRoom ?? room;
				replayWriter.RecordInitialState(__instance.ToSave(snapshotRoom));
			}
			catch (Exception e)
			{
				Log.Error($"Failed to initialize replay state for event combat chaining: {e}");
			}
		}
	}
}
