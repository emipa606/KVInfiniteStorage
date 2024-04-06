using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace InfiniteStorage;

public class WorldComp : WorldComponent
{
    [Unsaved] private static readonly Dictionary<Map, LinkedList<Building_InfiniteStorage>> ifStorages =
        new Dictionary<Map, LinkedList<Building_InfiniteStorage>>();

    [Unsaved] private static readonly Dictionary<Map, LinkedList<Building_InfiniteStorage>> ifNonGlobalStorages =
        new Dictionary<Map, LinkedList<Building_InfiniteStorage>>();

    public WorldComp(World world)
        : base(world)
    {
        foreach (var value in ifStorages.Values)
        {
            value.Clear();
        }

        ifStorages.Clear();
    }

    public static void Add(Map map, Building_InfiniteStorage storage)
    {
        if (storage == null)
        {
            Log.Error("Tried to add a null storage");
        }
        else if (map == null || storage.Map == null)
        {
            Log.Error($"Tried to add {storage.Label} to a null map. Please let me know if this ever happens!");
        }
        else if (!storage.IncludeInWorldLookup)
        {
            Add(map, storage, ifNonGlobalStorages);
        }
        else
        {
            Add(map, storage, ifStorages);
        }
    }

    private static void Add(Map map, Building_InfiniteStorage storage,
        Dictionary<Map, LinkedList<Building_InfiniteStorage>> storages)
    {
        if (!storages.TryGetValue(map, out var value))
        {
            value = [];
            storages.Add(map, value);
        }

        if (!value.Contains(storage))
        {
            value.AddLast(storage);
        }
    }

    public static IEnumerable<Building_InfiniteStorage> GetAllInfiniteStorages()
    {
        if (ifStorages == null)
        {
            yield break;
        }

        foreach (var value in ifStorages.Values)
        {
            foreach (var item in value)
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<Building_InfiniteStorage> GetInfiniteStorages(Map map)
    {
        if (map != null && ifStorages.TryGetValue(map, out var value))
        {
            return value;
        }

        return new List<Building_InfiniteStorage>(0);
    }

    public static IEnumerable<Building_InfiniteStorage> GetInfiniteStoragesWithinRadius(Map map, IntVec3 position,
        float ingredientSearchRadius)
    {
        var list = new List<Building_InfiniteStorage>();
        if (map == null || !ifStorages.TryGetValue(map, out var value))
        {
            return list;
        }

        var num = ingredientSearchRadius * ingredientSearchRadius;
        {
            foreach (var item in value)
            {
                if ((item.Position - position).LengthHorizontalSquared < num)
                {
                    list.Add(item);
                }
            }

            return list;
        }
    }

    public static bool HasInfiniteStorages(Map map)
    {
        if (map != null && ifStorages.TryGetValue(map, out var value))
        {
            return value.Count > 0;
        }

        return false;
    }

    public static bool HasNonGlobalInfiniteStorages(Map map)
    {
        if (map != null && ifNonGlobalStorages.TryGetValue(map, out var value))
        {
            return value.Count > 0;
        }

        return false;
    }

    public static void Remove(Map map)
    {
        Log.Warning($"IS Map to remove: {map.uniqueID}");
        if (ifStorages.TryGetValue(map, out var value))
        {
            if (value.Count > 0)
            {
                Log.Warning($"ifStorages map: {value.First.Value.Map.uniqueID}");
            }

            value.Clear();
            Log.Warning("removing ifStorages");
            ifStorages.Remove(map);
        }

        if (!ifNonGlobalStorages.TryGetValue(map, out value))
        {
            return;
        }

        if (value.Count > 0)
        {
            Log.Warning($"ifNonGlobalStorages map: {value.First.Value.Map.uniqueID}");
        }

        value.Clear();
        Log.Warning("removing ifNonGlobalStorages");
        ifNonGlobalStorages.Remove(map);
    }

    public static void Remove(Map map, Building_InfiniteStorage storage)
    {
        Remove(map, storage, !storage.IncludeInWorldLookup ? ifNonGlobalStorages : ifStorages);
    }

    private static void Remove(Map map, Building_InfiniteStorage storage,
        Dictionary<Map, LinkedList<Building_InfiniteStorage>> storages)
    {
        if (map == null || !storages.TryGetValue(map, out var value))
        {
            return;
        }

        value.Remove(storage);
        if (value.Count == 0)
        {
            ifStorages.Remove(map);
        }
    }

    internal static void ClearAll()
    {
        ifStorages.Clear();
        ifNonGlobalStorages.Clear();
    }
}