using HarmonyLib;
using RimWorld;

namespace InfiniteStorage;

[HarmonyPatch(typeof(Dialog_Trade), "Close")]
internal static class Patch_Window_PreClose
{
    [HarmonyPriority(800)]
    private static void Postfix(bool doCloseSound)
    {
        TradeUtil.ReclaimThings();
    }
}