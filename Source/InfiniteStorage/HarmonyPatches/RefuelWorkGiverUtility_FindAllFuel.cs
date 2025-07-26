using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(RefuelWorkGiverUtility), nameof(RefuelWorkGiverUtility.FindAllFuel))]
public static class RefuelWorkGiverUtility_FindAllFuel
{
    private static void Prefix(Pawn pawn, Thing refuelable)
    {
        RefuelPatchUtil.Prefix(pawn, refuelable);
    }

    private static void Postfix(List<Thing> __result)
    {
        RefuelPatchUtil.Postfix(__result);
    }
}