using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using TH_Sanae.Scripts.Events;
using TH_Sanae.Scripts.Main;

namespace TH_Sanae.Scripts.Patches
{
	[HarmonyPatch(typeof(ActModel), nameof(ActModel.PullNextEvent))]
	public static class SanaeEventAvailabilityPatches
	{
		private static int? GetActNumber(ActModel act)
		{
			if (act is Underdocks)
			{
				return 1;
			}

			if (act is Overgrowth)
			{
				return 2;
			}

			if (act is Hive)
			{
				return 3;
			}

			return null;
		}

		private static bool IsSanae(Player player) => player.Character is SanaeCharacter;

		private static bool IsAllSanaeParty(RunState runState) => runState.Players.Count > 0 && runState.Players.All(IsSanae);

		private static bool HasSanaeInParty(RunState runState) => runState.Players.Any(IsSanae);

		public static void Prefix(ActModel __instance, RunState runState)
		{
			int? actNumber = GetActNumber(__instance);
			if (actNumber == null)
			{
				return;
			}

			bool allSanae = IsAllSanaeParty(runState);
			bool hasSanae = HasSanaeInParty(runState);

			foreach (EventModel ev in __instance.AllEvents.OfType<EventModel>())
			{
				if (!IsEventAllowed(ev, actNumber.Value, allSanae, hasSanae))
				{
					__instance.RemoveEventFromSet(ev);
				}
			}
		}

		private static bool IsEventAllowed(EventModel ev, int actNumber, bool allSanae, bool hasSanae)
		{
			return ev switch
			{
				AbandonedMoriyaShrine => actNumber == 2 && allSanae,
				AnotherMe => (actNumber == 1 || actNumber == 3) && allSanae,
				SuwaFoughtenField => actNumber == 3 && allSanae,
				WindGodLake => actNumber is 1 or 2,
				NewspaperSale => actNumber is 1 or 2,
				SimilarShrine => actNumber is 2 or 3,
				CollectBadLuckHina => actNumber is 2 or 3 && hasSanae,
				SearchForNewsTengu => actNumber is 2 or 3 && hasSanae,
				_ => true
			};
		}
	}
}
