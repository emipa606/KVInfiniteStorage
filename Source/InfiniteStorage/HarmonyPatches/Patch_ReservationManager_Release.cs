using HarmonyLib;
using Verse;
using Verse.AI;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(ReservationManager), nameof(ReservationManager.Release))]
internal static class Patch_ReservationManager_Release
{
    private static bool Prefix(ReservationManager __instance, LocalTargetInfo target)
    {
        return target == null || target is not { IsValid: true, ThingDestroyed: false } ||
               !ReservationManagerUtil.IsInfiniteStorageAt(__instance.map, target.Cell);
    }
}