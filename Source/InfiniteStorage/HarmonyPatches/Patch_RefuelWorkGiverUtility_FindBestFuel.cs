using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(RefuelWorkGiverUtility), nameof(RefuelWorkGiverUtility.FindBestFuel))]
public static class Patch_RefuelWorkGiverUtility_FindBestFuel
{
    private static void Prefix(Pawn pawn, Thing refuelable)
    {
        RefuelPatchUtil.Prefix(pawn, refuelable);
    }

    private static void Postfix(Thing __result)
    {
        RefuelPatchUtil.Postfix(__result);
    }
}