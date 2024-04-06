using Verse;

namespace InfiniteStorage;

internal static class ReservationManagerUtil
{
    public static bool IsInfiniteStorageAt(Map map, IntVec3 position)
    {
        var enumerable = map.thingGrid.ThingsAt(position);
        if (enumerable == null)
        {
            return false;
        }

        foreach (var item in enumerable)
        {
            if (item.GetType() == typeof(Building_InfiniteStorage))
            {
                return true;
            }
        }

        return false;
    }
}