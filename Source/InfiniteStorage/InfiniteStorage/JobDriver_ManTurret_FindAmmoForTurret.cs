using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage;

[HarmonyPatch(typeof(JobDriver_ManTurret), "FindAmmoForTurret")]
internal static class JobDriver_ManTurret_FindAmmoForTurret
{
    private static void Prefix(Pawn pawn, Building_TurretGun gun)
    {
        if (!pawn.IsColonist || pawn.Map != gun.Map)
        {
            return;
        }

        var allowedShellsSettings = gun.gun.TryGetComp<CompChangeableProjectile>().allowedShellsSettings;
        foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(gun.Map))
        {
            if (!infiniteStorage.TryRemove(allowedShellsSettings.filter, out var removed))
            {
                continue;
            }

            foreach (var item in removed)
            {
                BuildingUtil.DropThing(droppedThings: new List<Thing>(), toDrop: item, amountToDrop: item.stackCount,
                    from: infiniteStorage, map: infiniteStorage.Map);
            }
        }
    }
}