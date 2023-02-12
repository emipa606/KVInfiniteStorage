using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace InfiniteStorage;

[StaticConstructorOnStartup]
internal class HarmonyPatches
{
    static HarmonyPatches()
    {
        new Harmony("com.InfiniteStorage.rimworld.mod").PatchAll(Assembly.GetExecutingAssembly());
        Log.Message(
            $"InfiniteStorage Harmony Patches:{Environment.NewLine}  Prefix:{Environment.NewLine}    Designator_Build.ProcessInput - will block if looking for things.{Environment.NewLine}    ScribeSaver.InitSaving{Environment.NewLine}    SettlementAbandonUtility.Abandon{Environment.NewLine}  Postfix:{Environment.NewLine}    Pawn_TraderTracker.DrawMedOperationsTab{Environment.NewLine}    Pawn_TraderTracker.ThingsInGroup{Environment.NewLine}    Pawn_TraderTracker.ColonyThingsWillingToBuy{Environment.NewLine}    TradeShip.ColonyThingsWillingToBuy{Environment.NewLine}    Window.PreClose{Environment.NewLine}    WorkGiver_DoBill.TryFindBestBillIngredients");
    }

    [HarmonyPatch(typeof(RefuelWorkGiverUtility), "FindBestFuel")]
    private static class Patch_RefuelWorkGiverUtility_FindBestFuel
    {
        private static void Prefix(Pawn pawn, Thing refuelable)
        {
            RefuelPatchUtil.Prefix(pawn, refuelable);
        }

        private static void Postfix(Thing __result)
        {
            RefuelPatchUtil.Postfix(__result);
        }
    }

    [HarmonyPatch(typeof(RefuelWorkGiverUtility), "FindAllFuel")]
    private static class Patch_RefuelWorkGiverUtility_FindAllFuel
    {
        private static void Prefix(Pawn pawn, Thing refuelable)
        {
            RefuelPatchUtil.Prefix(pawn, refuelable);
        }

        private static void Postfix(List<Thing> __result)
        {
            RefuelPatchUtil.Postfix(__result);
        }
    }

    internal static class RefuelPatchUtil
    {
        private static Dictionary<Thing, Building_InfiniteStorage> droppedAndStorage;

        internal static void Prefix(Pawn pawn, Thing refuelable)
        {
            if (!WorldComp.HasInfiniteStorages(refuelable.Map))
            {
                return;
            }

            droppedAndStorage = new Dictionary<Thing, Building_InfiniteStorage>();
            var fuelFilter = refuelable.TryGetComp<CompRefuelable>().Props.fuelFilter;
            foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(refuelable.Map))
            {
                if (!infiniteStorage.Spawned || infiniteStorage.Map != pawn.Map || !infiniteStorage.IsOperational ||
                    !infiniteStorage.TryRemove(fuelFilter, out var removed))
                {
                    continue;
                }

                var list = new List<Thing>();
                foreach (var item in removed)
                {
                    BuildingUtil.DropThing(item, item.def.stackLimit, infiniteStorage, infiniteStorage.Map, list);
                }

                if (list.Count > 0)
                {
                    droppedAndStorage.Add(list[0], infiniteStorage);
                }
            }
        }

        internal static void Postfix(Thing __result)
        {
            if (droppedAndStorage == null)
            {
                return;
            }

            foreach (var item in droppedAndStorage)
            {
                if (item.Key != __result)
                {
                    item.Value.Add(item.Key);
                }
            }

            droppedAndStorage.Clear();
        }

        internal static void Postfix(List<Thing> __result)
        {
            if (droppedAndStorage == null || __result == null)
            {
                return;
            }

            var hashSet = new HashSet<Thing>();
            foreach (var item in __result)
            {
                hashSet.Add(item);
            }

            foreach (var item2 in droppedAndStorage)
            {
                if (!hashSet.Contains(item2.Key))
                {
                    item2.Value.Add(item2.Key);
                }
            }

            hashSet.Clear();
            droppedAndStorage.Clear();
        }
    }

    [HarmonyPatch(typeof(HistoryAutoRecorder), "ExposeData")]
    private static class Patch_HistoryAutoRecorder_ExposeData
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

    [HarmonyPatch(typeof(WealthWatcher), "ForceRecount")]
    private static class Patch_WealthWatcher_ForceRecount
    {
        private static float lastItemWealth = -1f;

        [HarmonyPriority(0)]
        private static void Postfix(WealthWatcher __instance, Map ___map, ref float ___wealthItems)
        {
            var num = TallyWealth(wealthItems: ___wealthItems, storages: WorldComp.GetInfiniteStorages(___map));
            if (lastItemWealth < 1f)
            {
                lastItemWealth = num;
            }
            else if (num > lastItemWealth * 5f)
            {
                var num2 = num;
                num = lastItemWealth;
                lastItemWealth = num2;
            }
            else
            {
                lastItemWealth = num;
            }

            ___wealthItems = num;
        }

        private static float TallyWealth(IEnumerable<Building_InfiniteStorage> storages, float wealthItems)
        {
            foreach (var storage in storages)
            {
                foreach (var storedThing in storage.StoredThings)
                {
                    wealthItems = storedThing is not ThingWithComps
                        ? wealthItems + (storedThing.stackCount * storedThing.def.BaseMarketValue)
                        : wealthItems + (storedThing.stackCount * storedThing.MarketValue);
                }
            }

            return wealthItems;
        }
    }

    [HarmonyPatch]
    private static class WorkGiver_Tend_Smart_Medicine_Patch
    {
        private static MethodBase target;

        private static bool Prepare()
        {
            var modContentPack = LoadedModManager.RunningMods.FirstOrDefault(m => m.Name == "Smart Medicine");
            if (modContentPack == null)
            {
                return false;
            }

            var type = Enumerable
                .FirstOrDefault(modContentPack.assemblies.loadedAssemblies, a => a.GetName().Name == "SmartMedicine")
                ?.GetType("SmartMedicine.FindBestMedicine");
            if (type == null)
            {
                Log.Warning("InfiniteStorage can't patch 'Smart Medicine'");
                return false;
            }

            target = AccessTools.DeclaredMethod(type, "Find");
            if (target != null)
            {
                return true;
            }

            Log.Warning("InfiniteStorage can't patch 'Smart Medicine' Find");
            return false;
        }

        private static MethodBase TargetMethod()
        {
            return target;
        }

        private static void Postfix(ref List<ThingCount> __result, Pawn healer, Pawn patient)
        {
            if (healer.Map != patient.Map)
            {
                return;
            }

            foreach (var item in WorldComp.GetInfiniteStoragesWithinRadius(healer.Map, patient.Position, 20f))
            {
                foreach (var medicalThing in item.GetMedicalThings(false, true))
                {
                    var list = new List<Thing>();
                    BuildingUtil.DropThing(medicalThing, medicalThing.stackCount, item, item.Map, list);
                    foreach (var item2 in list)
                    {
                        __result.Add(new ThingCount(item2, item2.stackCount));
                        item2.Map.reservationManager.CanReserveStack(healer, item2, 1, ReservationLayerDefOf.Floor,
                            true);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(HealthAIUtility), "FindBestMedicine")]
    private static class Patch_HealthAIUtility_FindBestMedicine
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

    [HarmonyPatch(typeof(HealthCardUtility), "DrawMedOperationsTab")]
    private static class Patch_HealthCardUtility_DrawMedOperationsTab
    {
        private static long lastUpdate;

        private static readonly List<Thing> cache = new List<Thing>();

        [HarmonyPriority(800)]
        private static void Prefix()
        {
            Patch_ListerThings_ThingsInGroup.AvailableMedicalThing.Clear();
            var currentMap = Find.CurrentMap;
            if (currentMap == null)
            {
                return;
            }

            var ticks = DateTime.Now.Ticks;
            if (cache == null || ticks - lastUpdate > 10000000)
            {
                cache?.Clear();
                foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(currentMap))
                {
                    if (infiniteStorage.def.defName.Equals("IS_BodyPartStorage") ||
                        infiniteStorage.def.defName.Equals("InfiniteStorage"))
                    {
                        cache?.AddRange(infiniteStorage.GetMedicalThings());
                    }
                }

                lastUpdate = ticks;
            }

            if (cache != null)
            {
                Patch_ListerThings_ThingsInGroup.AvailableMedicalThing.AddRange(cache);
            }
        }

        private static void Postfix()
        {
            Patch_ListerThings_ThingsInGroup.AvailableMedicalThing.Clear();
        }
    }

    [HarmonyPatch(typeof(ListerThings), "ThingsInGroup")]
    private static class Patch_ListerThings_ThingsInGroup
    {
        public static readonly List<Thing> AvailableMedicalThing = new List<Thing>();

        private static void Postfix(ref List<Thing> __result)
        {
            if (AvailableMedicalThing.Count > 0)
            {
                __result.AddRange(AvailableMedicalThing);
            }
        }
    }

    [HarmonyPatch(typeof(FactionDialogMaker), "AmountSendableSilver")]
    private static class Patch_FactionDialogMaker_AmountSendableSilver
    {
        private static void Prefix(Map map)
        {
            foreach (var infiniteStorage in WorldComp.GetInfiniteStorages(map))
            {
                if (!infiniteStorage.TryRemove(ThingDefOf.Silver, out var removed))
                {
                    continue;
                }

                foreach (var item in removed)
                {
                    BuildingUtil.DropThing(item, item.stackCount, infiniteStorage, infiniteStorage.Map);
                }
            }
        }
    }
}