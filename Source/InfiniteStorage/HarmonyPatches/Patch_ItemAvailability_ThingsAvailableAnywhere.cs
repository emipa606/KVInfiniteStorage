using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(ItemAvailability), nameof(ItemAvailability.ThingsAvailableAnywhere))]
internal static class Patch_ItemAvailability_ThingsAvailableAnywhere
{
    private static void Postfix(ref bool __result, ItemAvailability __instance, ThingCountClass need, Pawn pawn)
    {
        if (__result || pawn == null || pawn.Faction != Faction.OfPlayer)
        {
            return;
        }

        foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(pawn.Map))
        {
            if (!infiniteStorage.IsOperational || !infiniteStorage.Spawned || need == null || need.thing == null ||
                !infiniteStorage.TryGetValue(need.thing.def, out var t) || t.stackCount < need.Count)
            {
                continue;
            }

            var count = need.Count < t.def.stackLimit ? t.def.stackLimit : need.Count;
            if (!infiniteStorage.TryRemove(t, count, out var removed))
            {
                break;
            }

            foreach (var item in removed)
            {
                BuildingUtil.DropThing(item, item.stackCount, infiniteStorage, infiniteStorage.Map);
            }

            __result = true;

            __instance.cachedResults[Gen.HashCombine(need.GetHashCode(), pawn.Faction)] = __result;
            break;
        }
    }
}