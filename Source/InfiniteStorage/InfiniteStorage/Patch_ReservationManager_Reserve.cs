using HarmonyLib;
using Verse;
using Verse.AI;

namespace InfiniteStorage;

[HarmonyPatch(typeof(ReservationManager), "Reserve")]
internal static class Patch_ReservationManager_Reserve
{
    private static bool Prefix(ref bool __result, ReservationManager __instance, Pawn claimant, LocalTargetInfo target)
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