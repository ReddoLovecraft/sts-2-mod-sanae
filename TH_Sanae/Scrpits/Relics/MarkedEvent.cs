using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace TH_Sanae.Scripts.Main
{
	[Pool(typeof(SanaeRelicPool))]
	public sealed class MarkedEvent : SanaeRelicModel
	{
		private static readonly Color WoodTint = new Color("45d72cff");

		public override string PackedIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
    	protected override string PackedIconOutlinePath => $"res://TH_Sanae/ArtWorks/Relics/Outlines/{Id.Entry}.png";
    	protected override string BigIconPath => $"res://TH_Sanae/ArtWorks/Relics/{Id.Entry}.png";
		public override RelicRarity Rarity => RelicRarity.Ancient;
		public override bool HasUponPickupEffect => true;

		private int _markedActIndex = -1;

		[SavedProperty]
		public int MarkedActIndex
		{
			get => _markedActIndex;
			set
			{
				AssertMutable();
				_markedActIndex = value;
			}
		}

		[SavedProperty]
		private int[] WoodCoordCols { get; set; } = Array.Empty<int>();

		[SavedProperty]
		private int[] WoodCoordRows { get; set; } = Array.Empty<int>();

		[SavedProperty]
		private int[] StoneCoordCols { get; set; } = Array.Empty<int>();

		[SavedProperty]
		private int[] StoneCoordRows { get; set; } = Array.Empty<int>();

		[SavedProperty]
		private bool MarkedCoordsSet { get; set; }

		public override Task AfterObtained()
		{
			MarkedActIndex = base.Owner.RunState.CurrentActIndex;
			AddMarkedRooms(base.Owner.RunState.Map);
			return Task.CompletedTask;
		}

		public override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
		{
			return AddMarkedRooms(map);
		}

		public override async Task AfterRoomEntered(AbstractRoom room)
		{
			if (!MarkedCoordsSet || base.Owner.RunState.CurrentActIndex != MarkedActIndex)
			{
				return;
			}

			MapCoord coord = base.Owner.RunState.CurrentMapPoint.coord;
			if (ContainsCoord(WoodCoordCols, WoodCoordRows, coord))
			{
				Flash();
				await CreatureCmd.GainMaxHp(base.Owner.Creature, 10m);
			}
			else if (ContainsCoord(StoneCoordCols, StoneCoordRows, coord))
			{
				Flash();
				await PlayerCmd.GainGold(100m, base.Owner);
			}
		}

		internal bool TryGetQuestIconTint(MapCoord coord, out Color tint)
		{
			if (!MarkedCoordsSet || base.Owner.RunState.CurrentActIndex != MarkedActIndex)
			{
				tint = Colors.White;
				return false;
			}

			if (ContainsCoord(WoodCoordCols, WoodCoordRows, coord))
			{
				tint = WoodTint;
				return true;
			}

			if (ContainsCoord(StoneCoordCols, StoneCoordRows, coord))
			{
				tint = StsColors.gold;
				return true;
			}

			tint = Colors.White;
			return false;
		}

		private ActMap AddMarkedRooms(ActMap map)
		{
			if (base.Owner.RunState.CurrentActIndex != MarkedActIndex)
			{
				return map;
			}

			List<MapCoord>? woodCoords = GetMarkedCoords(WoodCoordCols, WoodCoordRows);
			List<MapCoord>? stoneCoords = GetMarkedCoords(StoneCoordCols, StoneCoordRows);
			bool invalid = woodCoords == null || stoneCoords == null;
			if (!invalid)
			{
				invalid = !woodCoords.TrueForAll(c => map.HasPoint(c)) || !stoneCoords.TrueForAll(c => map.HasPoint(c));
			}

			if (invalid)
			{
				Rng rng = new Rng(base.Owner, base.Id);
				List<MapPoint> candidates = map.GetAllMapPoints()
					.Where(p => p.PointType != MapPointType.Unassigned
						&& p.PointType != MapPointType.Unknown
						&& p.PointType != MapPointType.Boss
						&& !p.Quests.Any(q => q is MarkedEvent))
					.ToList();

				candidates.UnstableShuffle(rng);

				int woodCount = 7;
				int stoneCount = 7;
				int total = Math.Min(candidates.Count, woodCount + stoneCount);
				List<MapPoint> picked = candidates.Take(total).ToList();

				List<MapPoint> woodPicked = picked.Take(int.Min(woodCount, picked.Count)).ToList();
				List<MapPoint> stonePicked = picked.Skip(woodPicked.Count).Take(int.Min(stoneCount, picked.Count - woodPicked.Count)).ToList();

				WoodCoordCols = new int[woodPicked.Count];
				WoodCoordRows = new int[woodPicked.Count];
				for (int i = 0; i < woodPicked.Count; i++)
				{
					WoodCoordCols[i] = woodPicked[i].coord.col;
					WoodCoordRows[i] = woodPicked[i].coord.row;
				}

				StoneCoordCols = new int[stonePicked.Count];
				StoneCoordRows = new int[stonePicked.Count];
				for (int i = 0; i < stonePicked.Count; i++)
				{
					StoneCoordCols[i] = stonePicked[i].coord.col;
					StoneCoordRows[i] = stonePicked[i].coord.row;
				}

				MarkedCoordsSet = true;
				foreach (MapPoint p in picked)
				{
					p.AddQuest(this);
				}
			}
			else
			{
				foreach (MapCoord c in woodCoords!)
				{
					map.GetPoint(c)?.AddQuest(this);
				}

				foreach (MapCoord c in stoneCoords!)
				{
					map.GetPoint(c)?.AddQuest(this);
				}
			}

			return map;
		}

		private static bool ContainsCoord(int[] cols, int[] rows, MapCoord coord)
		{
			for (int i = 0; i < cols.Length && i < rows.Length; i++)
			{
				if (cols[i] == coord.col && rows[i] == coord.row)
				{
					return true;
				}
			}

			return false;
		}

		private static List<MapCoord>? GetMarkedCoords(int[] cols, int[] rows)
		{
			if (cols.Length == 0 || rows.Length == 0)
			{
				return null;
			}

			int count = Math.Min(cols.Length, rows.Length);
			List<MapCoord> coords = new List<MapCoord>(count);
			for (int i = 0; i < count; i++)
			{
				coords.Add(new MapCoord { col = cols[i], row = rows[i] });
			}

			return coords;
		}
	}
}

namespace TH_Sanae.Scripts.Patches
{
	[HarmonyPatch(typeof(NNormalMapPoint), "RefreshMarkedIconVisibility")]
	internal static class MarkedEventQuestIconTintPatch
	{
		private static void Postfix(NNormalMapPoint __instance)
		{
			TextureRect? questIcon = __instance.GetNodeOrNull<TextureRect>("%QuestIcon");
			if (questIcon == null || !questIcon.Visible)
			{
				return;
			}

			foreach (AbstractModel quest in __instance.Point.Quests)
			{
				if (quest is TH_Sanae.Scripts.Main.MarkedEvent markedEvent && markedEvent.TryGetQuestIconTint(__instance.Point.coord, out Color tint))
				{
					questIcon.SelfModulate = tint;
					return;
				}
			}

			questIcon.SelfModulate = Colors.White;
		}
	}
}
