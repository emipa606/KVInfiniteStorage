using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPriority(800)]
[HarmonyPatch(typeof(WorkGiver_DoBill), nameof(WorkGiver_DoBill.TryFindBestIngredientsHelper))]
internal static class WorkGiver_DoBill_TryFindBestIngredientsHelper
{
    private static void Prefix(List<IngredientCount> ingredients, Pawn pawn,
        ref List<Pair<Building_InfiniteStorage, List<Thing>>> __state)
    {
        if (ingredients == null || pawn == null)
        {
            return;
        }

        foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(pawn.Map))
        {
            List<Thing> list = null;
            foreach (var ingredient in ingredients)
            {
                if (!infiniteStorage.TryDropThings(ingredient, out var dropped))
                {
                    continue;
                }

                list ??= [];

                list.AddRange(dropped);
            }

            if (list is not { Count: > 0 })
            {
                continue;
            }

            __state ??= [];

            __state.Add(new Pair<Building_InfiniteStorage, List<Thing>>(infiniteStorage, list));
        }
    }

    private static void Postfix(ref bool __result, Pawn pawn, List<IngredientCount> ingredients,
        List<ThingCount> chosen, ref List<Pair<Building_InfiniteStorage, List<Thing>>> __state)
    {
        if (!__result)
        {
            if (__state == null)
            {
                return;
            }

            {
                foreach (var item in __state)
                {
                    foreach (var item2 in item.Second)
                    {
                        item.First.Add(item2);
                    }
                }

                return;
            }
        }

        if (pawn == null || ingredients == null || chosen == null)
        {
            return;
        }

        var dictionary = new Dictionary<int, int>();
        foreach (var item3 in chosen)
        {
            dictionary.Add(item3.Thing.thingIDNumber, item3.Count);
        }

        foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(pawn.Map))
        {
            infiniteStorage.ReclaimFaster(true, dictionary);
        }
    }
}