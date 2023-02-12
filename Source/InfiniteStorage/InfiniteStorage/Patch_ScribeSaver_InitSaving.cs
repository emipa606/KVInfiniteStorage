using System;
using HarmonyLib;
using Verse;

namespace InfiniteStorage;

[HarmonyPatch(typeof(ScribeSaver), "InitSaving")]
internal static class Patch_ScribeSaver_InitSaving
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