using HarmonyLib;
using RimWorld;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(TradeDeal), nameof(TradeDeal.Reset))]
internal static class TradeDeal_Reset
{
    private static void Prefix()
    {
        TradeUtil.ReclaimThings();
    }
}