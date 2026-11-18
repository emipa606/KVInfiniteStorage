using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(RefuelWorkGiverUtility), nameof(RefuelWorkGiverUtility.FindBestFuel))]
public static class RefuelWorkGiverUtility_FindBestFuel
{
    public static void Prefix(Pawn pawn, Thing refuelable)
    {
        RefuelPatchUtil.Prefix(pawn, refuelable);
    }

    public static void Postfix(Thing __result)
    {
        RefuelPatchUtil.Postfix(__result);
    }
}