using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(Building_Storage), nameof(Building_Storage.Accepts))]
internal static class Building_Storage_Accepts
{
    [HarmonyPriority(800)]
    private static bool Prefix(Building_Storage __instance, ref bool __result, Thing t)
    {
        if (__instance is not Building_InfiniteStorage buildingInfiniteStorage)
        {
            return true;
        }

        __result = buildingInfiniteStorage.DoesAccept(t);
        return false;
    }
}