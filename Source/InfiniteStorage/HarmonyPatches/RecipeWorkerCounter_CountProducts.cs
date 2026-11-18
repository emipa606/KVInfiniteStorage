using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[HarmonyPatch(typeof(RecipeWorkerCounter), nameof(RecipeWorkerCounter.CountProducts))]
internal static class RecipeWorkerCounter_CountProducts
{
    public static void Postfix(ref int __result, RecipeWorkerCounter __instance, Bill_Production bill)
    {
        var products = __instance.recipe.products;
        if (!WorldComp.HasInfiniteStorages(bill.Map) || products == null)
        {
            return;
        }

        foreach (var thingDef in products.Select(p => p.thingDef))
        {
            foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(bill.Map))
            {
                __result += infiniteStorage.StoredThingCount(thingDef, bill.ingredientFilter);
            }
        }
    }
}