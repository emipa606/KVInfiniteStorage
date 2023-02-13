using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace InfiniteStorage;

[HarmonyPatch(typeof(Designator_Build), "ProcessInput")]
internal static class Patch_Designator_Build_ProcessInput
{
    private static bool Prefix(Designator_Build __instance, Event ev)
    {
        var currentMap = Find.CurrentMap;
        if (__instance.entDef is not ThingDef { MadeFromStuff: true } thingDef ||
            !WorldComp.HasInfiniteStorages(currentMap))
        {
            return true;
        }

        var list = new List<FloatMenuOption>();
        foreach (var current2 in currentMap.resourceCounter.AllCountedAmounts.Keys)
        {
            if (!current2.IsStuff || !current2.stuffProps.CanMake(thingDef) ||
                !DebugSettings.godMode && currentMap.listerThings.ThingsOfDef(current2).Count <= 0)
            {
                continue;
            }

            string label = current2.LabelCap;
            list.Add(new FloatMenuOption(label, delegate
            {
                __instance.ProcessInput(ev);
                Find.DesignatorManager.Select(__instance);
                __instance.stuffDef = current2;
                __instance.writeStuff = true;
            }));
        }

        foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(currentMap))
        {
            if (!infiniteStorage.Spawned)
            {
                continue;
            }

            foreach (var storedThing in infiniteStorage.StoredThings)
            {
                var current = storedThing.def;
                if (!current.IsStuff || !current.stuffProps.CanMake(thingDef) ||
                    !DebugSettings.godMode && storedThing.stackCount <= 0)
                {
                    continue;
                }

                string label2 = current.LabelCap;
                list.Add(new FloatMenuOption(label2, delegate
                {
                    __instance.ProcessInput(ev);
                    Find.DesignatorManager.Select(__instance);
                    __instance.stuffDef = current;
                    __instance.writeStuff = true;
                }));
            }
        }

        if (list.Count == 0)
        {
            Messages.Message("NoStuffsToBuildWith".Translate(), MessageTypeDefOf.RejectInput);
        }
        else
        {
            var floatMenu = new FloatMenu(list)
            {
                vanishIfMouseDistant = true
            };
            Find.WindowStack.Add(floatMenu);
            Find.DesignatorManager.Select(__instance);
        }

        return false;
    }
}