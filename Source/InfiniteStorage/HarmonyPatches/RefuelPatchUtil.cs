using System.Collections.Generic;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

public static class RefuelPatchUtil
{
    private static Dictionary<Thing, Building_InfiniteStorage> droppedAndStorage;

    internal static void Prefix(Pawn pawn, Thing refuelable)
    {
        if (!WorldComp.HasInfiniteStorages(refuelable.Map))
        {
            return;
        }

        droppedAndStorage = new Dictionary<Thing, Building_InfiniteStorage>();
        var fuelFilter = refuelable.TryGetComp<CompRefuelable>().Props.fuelFilter;
        foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(refuelable.Map))
        {
            if (!infiniteStorage.Spawned || infiniteStorage.Map != pawn.Map || !infiniteStorage.IsOperational ||
                !infiniteStorage.TryRemove(fuelFilter, out var removed))
            {
                continue;
            }

            var list = new List<Thing>();
            foreach (var item in removed)
            {
                BuildingUtil.DropThing(item, item.def.stackLimit, infiniteStorage, infiniteStorage.Map, list);
            }

            if (list.Count > 0)
            {
                droppedAndStorage.Add(list[0], infiniteStorage);
            }
        }
    }

    internal static void Postfix(Thing __result)
    {
        if (droppedAndStorage == null)
        {
            return;
        }

        foreach (var item in droppedAndStorage)
        {
            if (item.Key != __result)
            {
                item.Value.Add(item.Key);
            }
        }

        droppedAndStorage.Clear();
    }

    internal static void Postfix(List<Thing> __result)
    {
        if (droppedAndStorage == null || __result == null)
        {
            return;
        }

        var hashSet = new HashSet<Thing>();
        foreach (var item in __result)
        {
            hashSet.Add(item);
        }

        foreach (var item2 in droppedAndStorage)
        {
            if (!hashSet.Contains(item2.Key))
            {
                item2.Value.Add(item2.Key);
            }
        }

        hashSet.Clear();
        droppedAndStorage.Clear();
    }
}