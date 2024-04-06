using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(Dialog_FormCaravan), nameof(Dialog_FormCaravan.PostOpen))]
internal static class Patch_Dialog_FormCaravan_PostOpen
{
    private static void Prefix(Window __instance, Map ___map)
    {
        if (!(__instance.GetType() == typeof(Dialog_FormCaravan)))
        {
            return;
        }

        foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(___map))
        {
            infiniteStorage.Empty();
        }
    }
}