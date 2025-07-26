using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(HistoryAutoRecorder), nameof(HistoryAutoRecorder.ExposeData))]
public static class HistoryAutoRecorder_ExposeData
{
    private static void Postfix(HistoryAutoRecorder __instance)
    {
        if (Scribe.mode != LoadSaveMode.PostLoadInit)
        {
            return;
        }

        var records = __instance.records;
        for (var i = 0; i < records.Count; i++)
        {
            if (i <= 0 || !(records[i] > 10000f))
            {
                continue;
            }

            var num = records[i - 1] * 1.5f;
            if (!(records[i] > num))
            {
                continue;
            }

            int j;
            for (j = i + 1; j < records.Count && !(records[j] < num); j++)
            {
            }

            var value = j == records.Count ? records[i] : (records[i - 1] + records[j]) * 0.5f;
            for (; i < j; i++)
            {
                records[i] = value;
            }
        }

        __instance.records = records;
    }
}