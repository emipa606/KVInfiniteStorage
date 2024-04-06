using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace InfiniteStorage.HarmonyPatches;

[StaticConstructorOnStartup]
internal class HarmonyPatches
{
    static HarmonyPatches()
    {
        new Harmony("com.InfiniteStorage.rimworld.mod").PatchAll(Assembly.GetExecutingAssembly());
        Log.Message(
            $"InfiniteStorage Harmony Patches:{Environment.NewLine}  Prefix:{Environment.NewLine}    Designator_Build.ProcessInput - will block if looking for things.{Environment.NewLine}    ScribeSaver.InitSaving{Environment.NewLine}    SettlementAbandonUtility.Abandon{Environment.NewLine}  Postfix:{Environment.NewLine}    Pawn_TraderTracker.DrawMedOperationsTab{Environment.NewLine}    Pawn_TraderTracker.ThingsInGroup{Environment.NewLine}    Pawn_TraderTracker.ColonyThingsWillingToBuy{Environment.NewLine}    TradeShip.ColonyThingsWillingToBuy{Environment.NewLine}    Window.PreClose{Environment.NewLine}    WorkGiver_DoBill.TryFindBestBillIngredients");
    }
}