using HarmonyLib;
using RimWorld;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.Reset))]
internal static class Patch_TradeDeal_Reset
{
    private static void Prefix()
    {
        TradeUtil.ReclaimThings();
    }
}