using System.Collections.Generic;
using Verse;

namespace InfiniteStorage;

internal static class TradeUtil
{
    public static IEnumerable<Thing> EmptyStorages(Map map)
    {
        var list = new List<Thing>();
        foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(map))
        {
            if (infiniteStorage.Map == map && infiniteStorage.Spawned && infiniteStorage.IncludeInTradeDeals)
            {
                infiniteStorage.Empty(list);
            }
        }

        return list;
    }

    public static void ReclaimThings(bool force = false)
    {
        foreach (var allInfiniteStorage in WorldComp.GetAllInfiniteStorages())
        {
            if (allInfiniteStorage.Map == null || !allInfiniteStorage.Spawned)
            {
                continue;
            }

            if (force)
            {
                allInfiniteStorage.ForceReclaim();
                continue;
            }

            allInfiniteStorage.Reclaim();
        }
    }
}