using HarmonyLib;
using RimWorld;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(Dialog_Trade), nameof(Dialog_Trade.Close))]
internal static class Window_PreClose
{
    [HarmonyPriority(800)]
    private static void Postfix()
    {
        TradeUtil.ReclaimThings(true);
    }
}