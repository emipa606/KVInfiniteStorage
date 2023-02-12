using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage;

[HarmonyPatch(typeof(TradeShip), "ColonyThingsWillingToBuy")]
internal static class Patch_PassingShip_TryOpenComms
{
    private static void Postfix(ref IEnumerable<Thing> __result, Pawn playerNegotiator)
    {
        if (playerNegotiator is not { Map: { } })
        {
            return;
        }

        var list = new List<Thing>(__result);
        list.AddRange(TradeUtil.EmptyStorages(playerNegotiator.Map));
        __result = list;
    }
}