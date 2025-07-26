using HarmonyLib;
using Verse;
using Verse.AI;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(ReservationManager), nameof(ReservationManager.Reserve))]
internal static class ReservationManager_Reserve
{
    private static bool Prefix(ref bool __result, ReservationManager __instance, LocalTargetInfo target)
    {
        if (__result || target == null || target is not { IsValid: true, ThingDestroyed: false } ||
            !ReservationManagerUtil.IsInfiniteStorageAt(__instance.map, target.Cell))
        {
            return true;
        }

        __result = true;
        return false;
    }
}