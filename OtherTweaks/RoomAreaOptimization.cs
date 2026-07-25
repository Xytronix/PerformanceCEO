using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PerformanceCEO.OtherTweaks;

[HarmonyPatch]
internal static class RoomAreaOptimization
{
    private struct RoomInfo
    {
        internal Vector2 FloorPos { get; private set; }
        internal Vector2 RoomSize { get; private set; }
        internal int ZoneAreaNumber { get; private set; }

        internal RoomInfo(Vector2 floorPos, Vector2 roomSize, int zoneAreaNumber)
        {
            FloorPos = floorPos;
            RoomSize = roomSize;
            ZoneAreaNumber = zoneAreaNumber;
        }

        internal void UpdateInfo(Vector2 floorPos, Vector2 roomSize, int zoneAreaNumber)
        {
            FloorPos = floorPos;
            RoomSize = roomSize;
            ZoneAreaNumber = zoneAreaNumber;
        }

        internal bool HasChangedFrom(Vector2 floorPos, Vector2 roomSize)
        {
            return floorPos == FloorPos && roomSize == RoomSize;
        }
    }

    private static readonly Dictionary<PlaceableRoom, RoomInfo> roomCache = new();
    private static bool letThrough = false;

    [HarmonyPatch(typeof(PlaceableRoom), nameof(PlaceableRoom.ZoneAreaNumber), MethodType.Getter)]
    [HarmonyPrefix]
    internal static bool ZoneAreaNumberPatch(PlaceableRoom __instance, ref int __result)
    {
        if (letThrough)
        {
            return true;
        }

        PerformanceCEO.LogInfo("In zone area number");

        if (roomCache.TryGetValue(__instance, out RoomInfo info))
        {
            if (info.HasChangedFrom(__instance.floorPosition, __instance.roomSize))
            {
                letThrough = true;
                int newAreaNumber = __instance.ZoneAreaNumber;
                letThrough = false;
                info.UpdateInfo(__instance.floorPosition, __instance.roomSize, newAreaNumber);
                __result = newAreaNumber;
                return false;
            }

            __result = info.ZoneAreaNumber;
            return false;
        }

        letThrough = true;
        int newAreaNumber2 = __instance.ZoneAreaNumber;
        letThrough = false;
        roomCache.Add(__instance, new RoomInfo(__instance.floorPosition, __instance.roomSize, newAreaNumber2));
        __result = newAreaNumber2;
        return false;
    }

    internal static void ClearAreaCache()
    {
        roomCache.Clear();
    }

    internal static IEnumerator ClearAreaCacheCoroutine()
    {
        ClearAreaCache();
        yield break;
    }
}