using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(WealthWatcher), nameof(WealthWatcher.ForceRecount))]
public static class WealthWatcher_ForceRecount
{
    private static float lastItemWealth = -1f;

    [HarmonyPriority(0)]
    private static void Postfix(Map ___map, ref float ___wealthItems)
    {
        var num = tallyWealth(wealthItems: ___wealthItems, storages: WorldComp.GetInfiniteStorages(___map));
        if (lastItemWealth < 1f)
        {
            lastItemWealth = num;
        }
        else if (num > lastItemWealth * 5f)
        {
            (num, lastItemWealth) = (lastItemWealth, num);
        }
        else
        {
            lastItemWealth = num;
        }

        ___wealthItems = num;
    }

    private static float tallyWealth(IEnumerable<Building_InfiniteStorage> storages, float wealthItems)
    {
        foreach (var storage in storages)
        {
            foreach (var storedThing in storage.StoredThings)
            {
                wealthItems = storedThing is not ThingWithComps
                    ? wealthItems + (storedThing.stackCount * storedThing.def.BaseMarketValue)
                    : wealthItems + (storedThing.stackCount * storedThing.MarketValue);
            }
        }

        return wealthItems;
    }
}