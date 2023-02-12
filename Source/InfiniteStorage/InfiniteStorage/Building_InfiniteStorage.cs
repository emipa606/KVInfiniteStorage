using System;
using System.Collections.Generic;
using System.Text;
using InfiniteStorage.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace InfiniteStorage;

public class Building_InfiniteStorage : Building_Storage
{
    private CompPowerTrader compPowerTrader;

    private bool includeInTradeDeals = true;

    private long lastAutoReclaim;

    [Unsaved] private int storedCount;

    internal SortedDictionary<string, LinkedList<Thing>> storedThings =
        new SortedDictionary<string, LinkedList<Thing>>();

    [Unsaved] private float storedWeight;

    public List<Thing> temp;

    private List<Thing> ToDumpOnSpawn;

    public Building_InfiniteStorage()
    {
        AllowAdds = true;
        CanAutoCollect = true;
    }

    public IEnumerable<Thing> StoredThings
    {
        get
        {
            foreach (var value in storedThings.Values)
            {
                foreach (var item in value)
                {
                    yield return item;
                }
            }
        }
    }

    public int DefsCount
    {
        get
        {
            var num = 0;
            foreach (var value in storedThings.Values)
            {
                num += value.Count;
            }

            return num;
        }
    }

    public bool AllowAdds { get; set; }

    private Map CurrentMap { get; set; }

    public bool IncludeInTradeDeals => includeInTradeDeals;

    public bool UsesPower => compPowerTrader != null;

    public bool IsOperational => compPowerTrader == null || compPowerTrader.PowerOn;

    public bool CanAutoCollect { get; set; }

    public bool IncludeInWorldLookup { get; private set; }

    public void ResetAutoReclaimTime()
    {
        lastAutoReclaim = DateTime.Now.Ticks;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        CurrentMap = map;
        if (settings == null)
        {
            settings = new StorageSettings(this);
            settings.CopyFrom(def.building.defaultStorageSettings);
            settings.filter.SetDisallowAll();
        }

        IncludeInWorldLookup = GetIncludeInWorldLookup();
        WorldComp.Add(map, this);
        compPowerTrader = GetComp<CompPowerTrader>();
        if (ToDumpOnSpawn == null)
        {
            return;
        }

        foreach (var item in ToDumpOnSpawn)
        {
            BuildingUtil.DropThing(item, item.stackCount, this, Map);
        }

        ToDumpOnSpawn.Clear();
        ToDumpOnSpawn = null;
    }

    public bool TryGetFirstFilteredItemForMending(Bill bill, ThingFilter filter, bool remove, out Thing gotten)
    {
        gotten = null;
        foreach (var value2 in storedThings.Values)
        {
            if (value2 == null || value2.Count <= 0)
            {
                continue;
            }

            var linkedListNode = value2.First;
            while (linkedListNode.Next != null)
            {
                var value = linkedListNode.Value;
                if (!bill.IsFixedOrAllowedIngredient(value.def) || !filter.Allows(value.def))
                {
                    break;
                }

                if (bill.IsFixedOrAllowedIngredient(value) && filter.Allows(value) &&
                    value.HitPoints != value.MaxHitPoints)
                {
                    value2.Remove(linkedListNode);
                    BuildingUtil.DropSingleThing(value, this, Map, out gotten);
                    return true;
                }

                linkedListNode = linkedListNode.Next;
            }
        }

        return gotten != null;
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        try
        {
            storedCount = 0;
            Dispose();
            Destroy(mode);
        }
        catch (Exception ex)
        {
            Log.Error($"{GetType().Name}.Destroy\n{ex.GetType().Name} {ex.Message}\n{ex.StackTrace}");
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        try
        {
            storedCount = 0;
            Dispose();
            DeSpawn(mode);
        }
        catch (Exception ex)
        {
            Log.Error($"{GetType().Name}.DeSpawn\n{ex.GetType().Name} {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void Dispose()
    {
        try
        {
            AllowAdds = false;
            foreach (var value in storedThings.Values)
            {
                foreach (var item in value)
                {
                    BuildingUtil.DropThing(item, item.stackCount, this, CurrentMap);
                }
            }

            storedThings.Clear();
        }
        catch (Exception ex)
        {
            Log.Error($"{GetType().Name}.Dispose\n{ex.GetType().Name} {ex.Message}\n{ex.StackTrace}");
        }

        WorldComp.Remove(CurrentMap, this);
    }

    private void DropThing(Thing t, List<Thing> result)
    {
        BuildingUtil.DropThing(t, t.stackCount, this, CurrentMap, result);
    }

    public void Empty(List<Thing> droppedThings = null, bool force = false)
    {
        if (!force && !IsOperational && !Settings.EmptyOnPowerLoss)
        {
            return;
        }

        try
        {
            AllowAdds = false;
            foreach (var value in storedThings.Values)
            {
                foreach (var item in value)
                {
                    BuildingUtil.DropThing(item, item.stackCount, this, CurrentMap, droppedThings);
                }

                value.Clear();
            }

            storedCount = 0;
            storedWeight = 0f;
        }
        finally
        {
            ResetAutoReclaimTime();
            AllowAdds = true;
        }
    }

    public void Reclaim(bool respectReserved = true, List<ThingCount> chosen = null)
    {
        if (!IsOperational || !CanAutoCollect)
        {
            return;
        }

        var num = 0f;
        if (UsesPower && Settings.EnableEnergyBuffer)
        {
            num = compPowerTrader.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick;
            if (num <= Settings.DesiredEnergyBuffer)
            {
                return;
            }

            num -= Settings.DesiredEnergyBuffer;
        }

        foreach (var item in BuildingUtil.FindThingsOfTypeNextTo(Map, Position, 1))
        {
            if ((chosen == null || !ChosenContains(item, chosen)) && (!UsesPower || !Settings.EnableEnergyBuffer ||
                                                                      !((storedWeight +
                                                                         GetThingWeight(item, item.stackCount)) *
                                                                          Settings.EnergyFactor > num)))
            {
                Add(item);
            }
        }
    }

    public void ReclaimFaster(bool respectReserved = true, Dictionary<int, int> chosen = null)
    {
        if (!IsOperational || !CanAutoCollect)
        {
            return;
        }

        var num = 0f;
        if (UsesPower && Settings.EnableEnergyBuffer)
        {
            num = compPowerTrader.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick;
            if (num <= Settings.DesiredEnergyBuffer)
            {
                return;
            }

            num -= Settings.DesiredEnergyBuffer;
        }

        foreach (var item in BuildingUtil.FindThingsOfTypeNextTo(Map, Position, 1))
        {
            if (chosen != null && item != null && !chosen.ContainsKey(item.thingIDNumber) && (!UsesPower ||
                    !Settings.EnableEnergyBuffer ||
                    !((storedWeight + GetThingWeight(item,
                            item.stackCount)) *
                        Settings.EnergyFactor > num)))
            {
                Add(item);
            }
        }
    }

    public void ForceReclaim()
    {
        if (Map == null)
        {
            return;
        }

        foreach (var item in BuildingUtil.FindThingsOfTypeNextTo(Map, Position, 1))
        {
            if (item is not Building_InfiniteStorage && item != this && item.def.category == ThingCategory.Item)
            {
                Add(item, true);
            }
        }
    }

    private bool ChosenContains(Thing t, List<ThingCount> chosen)
    {
        if (chosen == null)
        {
            return false;
        }

        foreach (var item in chosen)
        {
            if (item.Thing == t)
            {
                return true;
            }
        }

        return false;
    }

    public int StoredThingCount(ThingDef expectedDef, ThingFilter ingrediantFilter)
    {
        var num = 0;
        if (!storedThings.TryGetValue(expectedDef.ToString(), out var value))
        {
            return num;
        }

        foreach (var item in value)
        {
            if (Allows(item, expectedDef, ingrediantFilter))
            {
                num += item.stackCount;
            }
        }

        return num;
    }

    private bool Allows(Thing t, ThingDef expectedDef, ThingFilter filter)
    {
        if (filter == null)
        {
            return true;
        }

        if (t.def != expectedDef)
        {
            return false;
        }

        if (expectedDef.useHitPoints && filter.AllowedHitPointsPercents.min != 0f &&
            filter.AllowedHitPointsPercents.max != 100f)
        {
            var f = t.HitPoints / (float)t.MaxHitPoints;
            f = GenMath.RoundedHundredth(f);
            if (!filter.AllowedHitPointsPercents.IncludesEpsilon(Mathf.Clamp01(f)))
            {
                return false;
            }
        }

        if (filter.AllowedQualityLevels == QualityRange.All || !t.def.FollowQualityThingFilter())
        {
            return true;
        }

        if (!t.TryGetQuality(out var qc))
        {
            qc = QualityCategory.Normal;
        }

        return filter.AllowedQualityLevels.Includes(qc);
    }

    public override void Notify_ReceivedThing(Thing newItem)
    {
        if (!AllowAdds)
        {
            BuildingUtil.DropSingleThing(newItem, this, Map, out var _);
        }
        else if (!Add(newItem))
        {
            DropThing(newItem, null);
        }
    }

    public bool DoesAccept(Thing thing)
    {
        if (!AllowAdds || thing == null || !IsOperational || !settings.AllowedToAccept(thing))
        {
            return false;
        }

        if (!UsesPower)
        {
            return true;
        }

        var num = Settings.EnableEnergyBuffer ? Settings.DesiredEnergyBuffer : 10;
        return !(compPowerTrader.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick <
                 num + GetThingWeight(thing, thing.stackCount));
    }

    public bool Add(Thing thing, bool force = false)
    {
        if (force)
        {
            if (thing == null || !settings.AllowedToAccept(thing))
            {
                return false;
            }
        }
        else
        {
            if (!DoesAccept(thing))
            {
                return false;
            }

            if (thing.stackCount == 0)
            {
                return true;
            }
        }

        if (thing.Spawned)
        {
            thing.DeSpawn();
        }

        var count = thing.stackCount;
        if (storedThings.TryGetValue(thing.def.ToString(), out var value))
        {
            var absorbedStack = false;
            if (thing.def.stackLimit > 1)
            {
                foreach (var item in value)
                {
                    if (item.TryAbsorbStack(thing, false))
                    {
                        absorbedStack = true;
                    }
                }
            }

            if (!absorbedStack)
            {
                foreach (var item2 in value)
                {
                    if (item2 == thing)
                    {
                        return true;
                    }
                }

                value.AddLast(thing);
            }
        }
        else
        {
            value = new LinkedList<Thing>();
            value.AddFirst(thing);
            storedThings.Add(thing.def.ToString(), value);
        }

        UpdateStoredStats(thing, count, true);
        return true;
    }

    private float GetThingWeight(Thing thing, int count)
    {
        return thing.GetStatValue(StatDefOf.Mass) * count;
    }

    private static bool IsBodyPart(ThingDef td)
    {
        foreach (var thingCategory in td.thingCategories)
        {
            if (thingCategory.defName.Contains("BodyPart"))
            {
                return true;
            }
        }

        return false;
    }

    public IEnumerable<Thing> GetMedicalThings(bool includeBodyParts = true, bool remove = false)
    {
        var list = new List<Thing>();
        foreach (var item in new List<LinkedList<Thing>>(storedThings.Values))
        {
            if (item.Count <= 0)
            {
                continue;
            }

            var thingDef = item.First.Value.def;
            if ((thingDef == null || !thingDef.IsMedicine) && (!includeBodyParts || !IsBodyPart(thingDef)))
            {
                continue;
            }

            if (remove)
            {
                if (TryRemove(thingDef, out var _))
                {
                    list.AddRange(item);
                }
            }
            else
            {
                list.AddRange(item);
            }
        }

        return list;
    }

    public bool TryGetFilteredThings(Bill bill, ThingFilter filter, out List<Thing> gotten)
    {
        gotten = null;
        foreach (var value in storedThings.Values)
        {
            if (value.Count <= 0 || !bill.IsFixedOrAllowedIngredient(value.First.Value.def))
            {
                continue;
            }

            foreach (var item in value)
            {
                if (!filter.Allows(item))
                {
                    continue;
                }

                if (gotten == null)
                {
                    gotten = new List<Thing>();
                }

                gotten.Add(item);
            }
        }

        return gotten != null;
    }

    public bool TryDropThings(IngredientCount ic, out List<Thing> dropped)
    {
        dropped = null;
        foreach (var value in storedThings.Values)
        {
            if (value.Count <= 0 || !ic.filter.Allows(value.First.Value.def))
            {
                continue;
            }

            foreach (var item in value)
            {
                if (!ic.filter.Allows(item))
                {
                    continue;
                }

                DropThing(item, null);
                if (dropped == null)
                {
                    dropped = new List<Thing>();
                }

                dropped.Add(item);
            }
        }

        var obj = dropped;
        if (obj == null)
        {
            return false;
        }

        return obj.Count > 0;
    }

    public bool TryGetValue(ThingDef def, out Thing t)
    {
        if (def != null && storedThings.TryGetValue(def.ToString(), out var value) && value.Count > 0)
        {
            t = value.First.Value;
            return true;
        }

        t = null;
        return false;
    }

    public bool TryRemove(ThingFilter filter, out List<Thing> removed)
    {
        foreach (var value2 in storedThings.Values)
        {
            if (value2.Count <= 0 || !filter.Allows(value2.First.Value.def))
            {
                continue;
            }

            var value = value2.First.Value;
            var count = Math.Min(value.stackCount, value.def.stackLimit);
            return TryRemove(value, count, out removed);
        }

        removed = null;
        return false;
    }

    public bool TryRemove(Thing thing)
    {
        if (!storedThings.TryGetValue(thing.def.ToString(), out var value) || !value.Remove(thing))
        {
            return false;
        }

        UpdateStoredStats(thing, thing.stackCount, false);
        return true;
    }

    public bool TryRemove(ThingDef def, out IEnumerable<Thing> removed)
    {
        if (storedThings.TryGetValue(def.ToString(), out var value))
        {
            storedThings.Remove(def.ToString());
            removed = value;
            foreach (var item in value)
            {
                UpdateStoredStats(item, item.stackCount, false);
            }

            return true;
        }

        removed = null;
        return false;
    }

    public bool TryRemove(Thing thing, int count, out List<Thing> removed)
    {
        return TryRemove(thing.def, count, out removed);
    }

    public bool TryRemove(ThingDef def, int count, out List<Thing> removed)
    {
        removed = null;
        if (!storedThings.TryGetValue(def.ToString(), out var value))
        {
            return false;
        }

        var num = count;
        var num2 = 0;
        var linkedListNode = value.First;
        while (linkedListNode != null && num > 0)
        {
            var value2 = linkedListNode.Value;
            var next = linkedListNode.Next;
            if (removed == null)
            {
                removed = new List<Thing>();
            }

            if (value2.stackCount == 0 || value2.Destroyed)
            {
                value.Remove(linkedListNode);
            }
            else if (num >= value2.stackCount)
            {
                num -= value2.stackCount;
                num2 += value2.stackCount;
                value.Remove(linkedListNode);
                removed.Add(value2);
            }
            else
            {
                num2 += num;
                while (num > 0)
                {
                    var num3 = Math.Min(num, value2.def.stackLimit);
                    num -= num3;
                    var item = value2.SplitOff(num3);
                    removed.Add(item);
                }
            }

            linkedListNode = next;
        }

        if (removed == null || removed.Count <= 0 || num2 <= 0)
        {
            return false;
        }

        UpdateStoredStats(removed[0], num2, false);
        return true;
    }

    private void UpdateStoredStats(Thing thing, int count, bool isAdding)
    {
        var num = GetThingWeight(thing, count);
        if (!isAdding)
        {
            num *= -1f;
            count *= -1;
        }

        storedCount += count;
        storedWeight += num;
        if (!(storedWeight < 0f))
        {
            return;
        }

        storedCount = 0;
        storedWeight = 0f;
        foreach (var value in storedThings.Values)
        {
            foreach (var item in value)
            {
                _ = item;
                UpdateStoredStats(thing, count, true);
            }
        }
    }

    public void HandleThingsOnTop()
    {
        if (!Spawned)
        {
            return;
        }

        foreach (var item in Map.thingGrid.ThingsAt(Position))
        {
            if (item == null || item == this || item is Blueprint || item is Building || Add(item) ||
                !item.Spawned)
            {
                continue;
            }

            var position = item.Position;
            position.x++;
            item.Position = position;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            temp = new List<Thing>();
            foreach (var value in storedThings.Values)
            {
                foreach (var item in value)
                {
                    temp.Add(item);
                }
            }
        }

        Scribe_Collections.Look(ref temp, "storedThings", LookMode.Deep);
        Scribe_Values.Look(ref includeInTradeDeals, "includeInTradeDeals", true);
        if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs)
        {
            return;
        }

        storedThings.Clear();
        if (temp != null)
        {
            foreach (var item2 in temp)
            {
                if (Add(item2))
                {
                    continue;
                }

                if (ToDumpOnSpawn == null)
                {
                    ToDumpOnSpawn = new List<Thing>();
                }

                ToDumpOnSpawn.Add(item2);
            }
        }

        temp?.Clear();
        temp = null;
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder(base.GetInspectString());
        if (stringBuilder.Length > 0)
        {
            stringBuilder.Append(Environment.NewLine);
        }

        stringBuilder.Append("InfiniteStorage.StoragePriority".Translate());
        stringBuilder.Append(": ");
        stringBuilder.Append(("StoragePriority" + settings.Priority).Translate());
        stringBuilder.Append(Environment.NewLine);
        if (compPowerTrader != null)
        {
            stringBuilder.Append("InfiniteStorage.StoredWeight".Translate());
            stringBuilder.Append(": ");
            stringBuilder.Append(storedWeight.ToString("N1"));
        }
        else
        {
            stringBuilder.Append("InfiniteStorage.Count".Translate());
            stringBuilder.Append(": ");
            stringBuilder.Append(storedCount);
        }

        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("InfiniteStorage.IncludeInTradeDeals".Translate());
        stringBuilder.Append(": ");
        stringBuilder.Append(includeInTradeDeals.ToString());
        return stringBuilder.ToString();
    }

    public override void TickRare()
    {
        base.TickRare();
        if (Spawned && Map != null && compPowerTrader != null)
        {
            compPowerTrader.PowerOutput = -1f * Settings.EnergyFactor * storedWeight;
            compPowerTrader.Props.basePowerConsumption = Settings.EnergyFactor * storedWeight;
        }

        var ticks = DateTime.Now.Ticks;
        if (Settings.CollectThingsAutomatically && ticks - lastAutoReclaim > Settings.TimeBetweenAutoCollectsTicks)
        {
            Reclaim();
            lastAutoReclaim = ticks;
        }

        if (!IsOperational && Settings.EmptyOnPowerLoss && storedCount > 0 && Settings.EnergyFactor > 1E-05f)
        {
            Empty(null, true);
        }
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        var gizmos = base.GetGizmos();
        var list = gizmos == null ? new List<Gizmo>(1) : new List<Gizmo>(gizmos);
        var hashCode = "InfiniteStorage".GetHashCode();
        list.Add(new Command_Action
        {
            icon = GetGizmoViewTexture(),
            defaultDesc = "InfiniteStorage.ViewDesc".Translate(),
            defaultLabel = "InfiniteStorage.View".Translate(),
            activateSound = SoundDef.Named("Click"),
            action = delegate { Find.WindowStack.Add(new ViewUI(this)); },
            groupKey = hashCode
        });
        hashCode++;
        list.Add(new Command_Action
        {
            icon = ViewUI.emptyTexture,
            defaultDesc = "InfiniteStorage.EmptyDesc".Translate(),
            defaultLabel = "InfiniteStorage.Empty".Translate(),
            activateSound = SoundDef.Named("Click"),
            action = delegate { Empty(null, true); },
            groupKey = hashCode
        });
        hashCode++;
        if (IsOperational)
        {
            list.Add(new Command_Action
            {
                icon = ViewUI.collectTexture,
                defaultDesc = "InfiniteStorage.CollectDesc".Translate(),
                defaultLabel = "InfiniteStorage.Collect".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate
                {
                    CanAutoCollect = true;
                    Reclaim();
                },
                groupKey = hashCode
            });
            hashCode++;
        }

        if (IncludeInWorldLookup)
        {
            list.Add(new Command_Action
            {
                icon = includeInTradeDeals ? ViewUI.yesSellTexture : ViewUI.noSellTexture,
                defaultDesc = "InfiniteStorage.IncludeInTradeDealsDesc".Translate(),
                defaultLabel = "InfiniteStorage.IncludeInTradeDeals".Translate(),
                activateSound = SoundDef.Named("Click"),
                action = delegate { includeInTradeDeals = !includeInTradeDeals; },
                groupKey = hashCode
            });
            hashCode++;
        }

        list.Add(new Command_Action
        {
            icon = ViewUI.applyFiltersTexture,
            defaultDesc = "InfiniteStorage.ApplyFiltersDesc".Translate(),
            defaultLabel = "InfiniteStorage.ApplyFilters".Translate(),
            activateSound = SoundDef.Named("Click"),
            action = ApplyFilters,
            groupKey = hashCode
        });
        return list;
    }

    private bool GetIncludeInWorldLookup()
    {
        var modExtension = def.GetModExtension<InfiniteStorageType>();
        if (modExtension == null || modExtension.IncludeInWorldLookup.Length <= 0 ||
            modExtension.IncludeInWorldLookup.ToLower()[0] != 'f')
        {
            return true;
        }

        Log.Warning("    return false");
        return false;
    }

    private Texture2D GetGizmoViewTexture()
    {
        var modExtension = def.GetModExtension<InfiniteStorgeGizmoViewTexture>();
        if (modExtension == null)
        {
            return ViewUI.InfiniteStorageViewTexture;
        }

        switch (modExtension.GizmoViewTexture)
        {
            case "viewbodyparts":
                return ViewUI.BodyPartViewTexture;
            case "viewtextile":
                return ViewUI.TextileViewTexture;
            case "viewtrough":
                return ViewUI.TroughViewTexture;
        }

        return ViewUI.InfiniteStorageViewTexture;
    }

    public void ApplyFilters()
    {
        Empty();
        Reclaim();
    }
}