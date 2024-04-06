using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(FactionDialogMaker), nameof(FactionDialogMaker.AmountSendableSilver))]
public static class Patch_FactionDialogMaker_AmountSendableSilver
{
    private static void Prefix(Map map)
    {
        foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(map))
        {
            if (!infiniteStorage.TryRemove(ThingDefOf.Silver, out var removed))
            {
                continue;
            }

            foreach (var item in removed)
            {
                BuildingUtil.DropThing(item, item.stackCount, infiniteStorage, infiniteStorage.Map);
            }
        }
    }
}