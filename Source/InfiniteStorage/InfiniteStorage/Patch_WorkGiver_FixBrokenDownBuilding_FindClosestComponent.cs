using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace InfiniteStorage;

[HarmonyPatch(typeof(WorkGiver_FixBrokenDownBuilding), "FindClosestComponent")]
internal static class Patch_WorkGiver_FixBrokenDownBuilding_FindClosestComponent
{
    private static void Postfix(ref Thing __result, Pawn pawn)
    {
        var foundComponent = false;
        if (pawn == null || __result != null)
        {
            return;
        }

        foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(pawn.Map))
        {
            if (!infiniteStorage.TryRemove(ThingDefOf.ComponentIndustrial, 1, out var removed))
            {
                continue;
            }

            foundComponent = true;
            foreach (var item in removed)
            {
                BuildingUtil.DropThing(item, 1, infiniteStorage, infiniteStorage.Map);
            }
        }

        if (foundComponent)
        {
            __result = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForDef(ThingDefOf.ComponentIndustrial), PathEndMode.InteractionCell,
                TraverseParms.For(pawn, pawn.NormalMaxDanger()), 9999f,
                x => !x.IsForbidden(pawn) && pawn.CanReserve(x));
        }
    }
}