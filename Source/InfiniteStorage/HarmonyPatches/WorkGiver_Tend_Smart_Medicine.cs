using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch]
public static class WorkGiver_Tend_Smart_Medicine
{
    private static MethodBase target;

    private static bool Prepare()
    {
        var modContentPack = LoadedModManager.RunningMods.FirstOrDefault(m => m.Name == "Smart Medicine");
        if (modContentPack == null)
        {
            return false;
        }

        var type = Enumerable
            .FirstOrDefault(modContentPack.assemblies.loadedAssemblies, a => a.GetName().Name == "SmartMedicine")
            ?.GetType("SmartMedicine.FindBestMedicine");
        if (type == null)
        {
            Log.Warning("InfiniteStorage can't patch 'Smart Medicine'");
            return false;
        }

        target = AccessTools.DeclaredMethod(type, "Find");
        if (target != null)
        {
            return true;
        }

        Log.Warning("InfiniteStorage can't patch 'Smart Medicine' Find");
        return false;
    }

    private static MethodBase TargetMethod()
    {
        return target;
    }

    private static void Postfix(ref List<ThingCount> __result, Pawn healer, Pawn patient)
    {
        if (healer.Map != patient.Map)
        {
            return;
        }

        foreach (var item in WorldComp.GetInfiniteStoragesWithinRadius(healer.Map, patient.Position, 20f))
        {
            foreach (var medicalThing in item.GetMedicalThings(false, true))
            {
                var list = new List<Thing>();
                BuildingUtil.DropThing(medicalThing, medicalThing.stackCount, item, item.Map, list);
                foreach (var item2 in list)
                {
                    __result.Add(new ThingCount(item2, item2.stackCount));
                    item2.Map.reservationManager.CanReserveStack(healer, item2, 1, ReservationLayerDefOf.Floor,
                        true);
                }
            }
        }
    }
}