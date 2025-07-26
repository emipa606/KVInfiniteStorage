using HarmonyLib;
using RimWorld.Planet;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(SettlementAbandonUtility), nameof(SettlementAbandonUtility.Abandon))]
internal static class SettlementAbandonUtility_Abandon
{
    [HarmonyPriority(800)]
    private static void Prefix(MapParent settlement)
    {
        WorldComp.Remove(settlement.Map);
    }
}