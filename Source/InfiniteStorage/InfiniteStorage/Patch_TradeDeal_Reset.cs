using HarmonyLib;
using RimWorld;

namespace InfiniteStorage;

[HarmonyPatch(typeof(TradeDeal), "Reset")]
internal static class Patch_TradeDeal_Reset
{
    private static void Prefix()
    {
        TradeUtil.ReclaimThings();
    }
}