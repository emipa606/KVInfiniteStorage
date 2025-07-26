using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(ListerThings), nameof(ListerThings.ThingsInGroup))]
public static class ListerThings_ThingsInGroup
{
    public static readonly List<Thing> AvailableMedicalThing = [];

    private static void Postfix(ref List<Thing> __result)
    {
        if (AvailableMedicalThing.Count > 0)
        {
            __result.AddRange(AvailableMedicalThing);
        }
    }
}