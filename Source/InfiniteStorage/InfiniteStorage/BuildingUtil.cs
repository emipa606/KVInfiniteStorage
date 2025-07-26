using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace InfiniteStorage;

internal class BuildingUtil
{
    public static IEnumerable<Thing> FindThingsOfTypeNextTo(Map map, IntVec3 position, int distance)
    {
        var num = Math.Max(0, position.x - distance);
        var num2 = Math.Min(map.info.Size.x, position.x + distance);
        var num3 = Math.Max(0, position.z - distance);
        var num4 = Math.Min(map.info.Size.z, position.z + distance);
        var reservationManager = map.reservationManager;
        var list = new List<Thing>();
        for (var i = num - 1; i <= num2; i++)
        {
            for (var j = num3 - 1; j <= num4; j++)
            {
                foreach (var item in map.thingGrid.ThingsAt(new IntVec3(i, position.y, j)))
                {
                    if (reservationManager == null ||
                        !reservationManager.IsReservedByAnyoneOf(new LocalTargetInfo(item), Faction.OfPlayer))
                    {
                        list.Add(item);
                    }
                }
            }
        }

        return list;
    }

    public static void DropThing(Thing toDrop, int amountToDrop, Building_InfiniteStorage from, Map map,
        List<Thing> droppedThings = null)
    {
        if (toDrop.stackCount == 0)
        {
            Log.Warning($"To Drop Thing {toDrop.Label} had stack count of 0");
            return;
        }

        try
        {
            from.AllowAdds = false;
            var thingsLeft = false;
            while (!thingsLeft)
            {
                var num = toDrop.def.stackLimit;
                if (num > amountToDrop)
                {
                    num = amountToDrop;
                    thingsLeft = true;
                }

                if (num >= toDrop.stackCount)
                {
                    if (amountToDrop > num)
                    {
                        Log.Error($"        ThingStorage: Unable to drop {amountToDrop - num} of {toDrop.def.label}");
                    }

                    num = toDrop.stackCount;
                    thingsLeft = true;
                }

                if (num <= 0)
                {
                    continue;
                }

                amountToDrop -= num;
                if (!DropSingleThing(toDrop.SplitOff(num), from, map, out var result2))
                {
                    continue;
                }

                droppedThings?.Add(result2);
            }
        }
        finally
        {
            from.AllowAdds = true;
        }
    }

    public static bool DropThing(Thing toDrop, int amountToDrop, IntVec3 from, Map map,
        List<Thing> droppedThings = null)
    {
        if (toDrop.stackCount == 0)
        {
            Log.Warning($"To Drop Thing {toDrop.Label} had stack count of 0");
            return false;
        }

        var result = false;
        var thingsLeft = false;
        while (!thingsLeft)
        {
            var num = toDrop.def.stackLimit;
            if (num > amountToDrop)
            {
                num = amountToDrop;
                thingsLeft = true;
            }

            if (num >= toDrop.stackCount)
            {
                if (amountToDrop > num)
                {
                    Log.Error(
                        $"        ThingStorage: Unable to drop {amountToDrop - num} of {toDrop.def.label}");
                }

                num = toDrop.stackCount;
                thingsLeft = true;
            }

            if (num <= 0)
            {
                continue;
            }

            amountToDrop -= num;
            if (!dropSingleThing(toDrop.SplitOff(num), from, map, out var result2))
            {
                continue;
            }

            droppedThings?.Add(result2);
            result = true;
        }

        return result;
    }

    public static bool DropSingleThing(Thing toDrop, Building_InfiniteStorage from, Map map, out Thing result)
    {
        result = null;
        if (toDrop.stackCount == 0)
        {
            Log.Warning($"To Drop Thing {toDrop.Label} had stack count of 0");
            return false;
        }

        try
        {
            from.AllowAdds = false;
            return dropSingleThing(toDrop, from.InteractionCell, map, out result);
        }
        finally
        {
            from.AllowAdds = true;
        }
    }

    private static bool dropSingleThing(Thing toDrop, IntVec3 from, Map map, out Thing result)
    {
        result = null;
        if (toDrop.stackCount == 0)
        {
            Log.Warning($"To Drop Thing {toDrop.Label} had stack count of 0");
            return false;
        }

        try
        {
            if (!toDrop.Spawned)
            {
                GenThing.TryDropAndSetForbidden(toDrop, from, map, ThingPlaceMode.Near, out result, false);
                if (!result.Spawned)
                {
                    if (!GenPlace.TryPlaceThing(toDrop, from, map, ThingPlaceMode.Near))
                    {
                        result = null;
                        Log.Error($"Failed to spawn {toDrop.Label} x{toDrop.stackCount}");
                        return false;
                    }

                    result = toDrop;
                }
            }

            toDrop.Position = from;
        }
        catch (Exception ex)
        {
            Log.Error($"{nameof(BuildingUtil)}.DropApparel\n{ex.GetType().Name} {ex.Message}\n{ex.StackTrace}");
        }

        return result is { Spawned: true };
    }
}