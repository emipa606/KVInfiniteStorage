using System;
using HarmonyLib;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(ScribeSaver), nameof(ScribeSaver.InitSaving))]
internal static class ScribeSaver_InitSaving
{
    private static void Prefix()
    {
        try
        {
            foreach (var allInfiniteStorage in WorldComp.GetAllInfiniteStorages())
            {
                try
                {
                    allInfiniteStorage.ForceReclaim();
                }
                catch (Exception ex)
                {
                    Log.Warning($"Error while reclaiming apparel for infinite storage\n{ex.Message}");
                }
            }
        }
        catch (Exception ex2)
        {
            Log.Warning($"Error while reclaiming items\n{ex2.Message}");
        }
    }
}