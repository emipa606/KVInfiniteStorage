using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage;

[HarmonyPatch(typeof(ResourceCounter), nameof(ResourceCounter.UpdateResourceCounts))]
internal static class Patch_ResourceCounter_UpdateResourceCounts
{
    private static void Postfix(ResourceCounter __instance)
    {
        foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(Find.CurrentMap))
        {
            foreach (var storedThing in infiniteStorage.StoredThings)
            {
                if (!storedThing.def.EverStorable(true) || !storedThing.def.CountAsResource || storedThing.IsNotFresh())
                {
                    continue;
                }

                int value = !__instance.countedAmounts.TryGetValue(storedThing.def, out value)
                    ? storedThing.stackCount
                    : value + storedThing.stackCount;
                __instance.countedAmounts[storedThing.def] = value;
            }
        }
    }
}