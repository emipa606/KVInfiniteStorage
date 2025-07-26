using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(TradeShip), nameof(TradeShip.ColonyThingsWillingToBuy))]
internal static class PassingShip_TryOpenComms
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