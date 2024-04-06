using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(HealthAIUtility), nameof(HealthAIUtility.FindBestMedicine))]
public static class Patch_HealthAIUtility_FindBestMedicine
{
    [HarmonyPriority(800)]
    private static void Prefix(Pawn patient)
    {
        foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(patient.Map))
        {
            foreach (var medicalThing in infiniteStorage.GetMedicalThings(false, true))
            {
                BuildingUtil.DropThing(medicalThing, medicalThing.stackCount, infiniteStorage, infiniteStorage.Map);
            }
        }
    }
}