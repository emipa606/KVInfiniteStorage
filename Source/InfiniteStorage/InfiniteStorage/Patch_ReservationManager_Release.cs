using HarmonyLib;
using Verse;
using Verse.AI;

namespace InfiniteStorage;

[HarmonyPatch(typeof(ReservationManager), "Release")]
internal static class Patch_ReservationManager_Release
{
    private static bool Prefix(ReservationManager __instance, Pawn claimant, LocalTargetInfo target)
    {
        return target == null || target is not { IsValid: true, ThingDestroyed: false } ||
               !ReservationManagerUtil.IsInfiniteStorageAt(__instance.map, target.Cell);
    }
}