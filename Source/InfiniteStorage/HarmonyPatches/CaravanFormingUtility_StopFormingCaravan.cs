using HarmonyLib;
using RimWorld.Planet;

namespace InfiniteStorage;

[HarmonyPatch(typeof(CaravanFormingUtility), nameof(CaravanFormingUtility.StopFormingCaravan))]
internal static class CaravanFormingUtility_StopFormingCaravan
{
    [HarmonyPriority(800)]
    private static void Postfix()
    {
        foreach (var allInfiniteStorage in WorldComp.GetAllInfiniteStorages())
        {
            allInfiniteStorage.Reclaim();
        }
    }
}