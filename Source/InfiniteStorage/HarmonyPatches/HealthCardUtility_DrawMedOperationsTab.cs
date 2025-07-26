using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(HealthCardUtility), nameof(HealthCardUtility.DrawMedOperationsTab))]
public static class HealthCardUtility_DrawMedOperationsTab
{
    private static long lastUpdate;

    private static readonly List<Thing> cache = [];

    [HarmonyPriority(800)]
    private static void Prefix()
    {
        ListerThings_ThingsInGroup.AvailableMedicalThing.Clear();
        var currentMap = Find.CurrentMap;
        if (currentMap == null)
        {
            return;
        }

        var ticks = DateTime.Now.Ticks;
        if (cache == null || ticks - lastUpdate > 10000000)
        {
            cache?.Clear();
            foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(currentMap))
            {
                if (infiniteStorage.def.defName.Equals("IS_BodyPartStorage") ||
                    infiniteStorage.def.defName.Equals("InfiniteStorage"))
                {
                    cache?.AddRange(infiniteStorage.GetMedicalThings());
                }
            }

            lastUpdate = ticks;
        }

        if (cache != null)
        {
            ListerThings_ThingsInGroup.AvailableMedicalThing.AddRange(cache);
        }
    }

    private static void Postfix()
    {
        ListerThings_ThingsInGroup.AvailableMedicalThing.Clear();
    }
}