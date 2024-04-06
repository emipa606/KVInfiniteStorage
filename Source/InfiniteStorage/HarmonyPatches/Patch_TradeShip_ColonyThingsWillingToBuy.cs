using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(Pawn_TraderTracker), nameof(Pawn_TraderTracker.ColonyThingsWillingToBuy))]
internal static class Patch_TradeShip_ColonyThingsWillingToBuy
{
    private static void Postfix(ref IEnumerable<Thing> __result, Pawn playerNegotiator)
    {
        if (playerNegotiator is not { Map: not null })
        {
            return;
        }

        var list = new List<Thing>(__result);
        list.AddRange(TradeUtil.EmptyStorages(playerNegotiator.Map));
        __result = list;
    }
}