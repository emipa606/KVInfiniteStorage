using HarmonyLib;
using RimWorld;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(MoveColonyUtility), nameof(MoveColonyUtility.MoveColonyAndReset))]
internal static class MoveColonyUtility_MoveColonyAndReset
{
    [HarmonyPriority(800)]
    private static void Prefix()
    {
        WorldComp.ClearAll();
    }
}