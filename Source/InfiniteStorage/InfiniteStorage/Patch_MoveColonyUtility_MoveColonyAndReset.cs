using HarmonyLib;
using RimWorld;

namespace InfiniteStorage;

[HarmonyPatch(typeof(MoveColonyUtility), "MoveColonyAndReset")]
internal static class Patch_MoveColonyUtility_MoveColonyAndReset
{
    [HarmonyPriority(800)]
    private static void Prefix()
    {
        WorldComp.ClearAll();
    }
}