using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse;

namespace InfiniteStorage.UI;

[StaticConstructorOnStartup]
public class ViewUI : Window
{
    private const int HEIGHT = 30;

    private const int BUFFER = 2;

    public static Texture2D BodyPartViewTexture;

    public static Texture2D TextileViewTexture;

    public static Texture2D InfiniteStorageViewTexture;

    public static Texture2D TroughViewTexture;

    public static Texture2D DropTexture;

    public static Texture2D emptyTexture;

    public static Texture2D collectTexture;

    public static Texture2D yesSellTexture;

    public static Texture2D noSellTexture;

    public static Texture2D applyFiltersTexture;

    private static ThingCategoryDef WeaponsMeleeCategoryDef;

    private static ThingCategoryDef WeaponsRangedCategoryDef;

    private readonly List<Thing> Apparel = new List<Thing>();

    private readonly Dictionary<string, Thing> Chunks = new Dictionary<string, Thing>();

    private readonly Building_InfiniteStorage InfiniteStorage;

    private readonly List<Thing> Minified = new List<Thing>();

    private readonly Dictionary<string, Thing> Misc = new Dictionary<string, Thing>();

    private readonly List<TabRecord> tabs = new List<TabRecord>();

    private readonly List<Thing> Weapons = new List<Thing>();

    private bool itemsDropped;

    private Vector2 scrollPosition = new Vector2(0f, 0f);

    private string searchText = "";

    private Tabs selectedTab = Tabs.InfiniteStorage_Misc;

    static ViewUI()
    {
        DropTexture = ContentFinder<Texture2D>.Get("InfiniteStorage/drop");
        BodyPartViewTexture = ContentFinder<Texture2D>.Get("InfiniteStorage/viewbodyparts");
        TextileViewTexture = ContentFinder<Texture2D>.Get("InfiniteStorage/viewtextile");
        InfiniteStorageViewTexture = ContentFinder<Texture2D>.Get("InfiniteStorage/viewif");
        TroughViewTexture = ContentFinder<Texture2D>.Get("InfiniteStorage/viewtrough");
        emptyTexture = ContentFinder<Texture2D>.Get("InfiniteStorage/empty");
        collectTexture = ContentFinder<Texture2D>.Get("InfiniteStorage/collect");
        yesSellTexture = ContentFinder<Texture2D>.Get("InfiniteStorage/yessell");
        noSellTexture = ContentFinder<Texture2D>.Get("InfiniteStorage/nosell");
        applyFiltersTexture = ContentFinder<Texture2D>.Get("InfiniteStorage/filter");
    }

    public ViewUI(Building_InfiniteStorage thingStorage)
    {
        InfiniteStorage = thingStorage;
        closeOnClickedOutside = true;
        doCloseButton = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        forcePause = true;
        PopulateDisplayThings();
    }

    public override Vector2 InitialSize => new Vector2(550f, 650f);

    private void PopulateDisplayThings()
    {
        if (WeaponsMeleeCategoryDef == null)
        {
            foreach (var item in DefDatabase<ThingCategoryDef>.AllDefsListForReading)
            {
                if (WeaponsMeleeCategoryDef != null && WeaponsRangedCategoryDef != null)
                {
                    break;
                }

                if (item.defName.EqualsIgnoreCase("WeaponsMelee"))
                {
                    WeaponsMeleeCategoryDef = item;
                }
                else if (item.defName.EqualsIgnoreCase("WeaponsRanged"))
                {
                    WeaponsRangedCategoryDef = item;
                }
            }
        }

        Misc.Clear();
        Minified.Clear();
        Apparel.Clear();
        Weapons.Clear();
        Chunks.Clear();
        foreach (var storedThing in InfiniteStorage.StoredThings)
        {
            if (storedThing.def.IsApparel)
            {
                Apparel.Add(storedThing);
            }
            else if (storedThing.def.thingCategories.Contains(WeaponsMeleeCategoryDef) ||
                     storedThing.def.thingCategories.Contains(WeaponsRangedCategoryDef))
            {
                Weapons.Add(storedThing);
            }
            else if (storedThing is MinifiedThing)
            {
                Minified.Add(storedThing);
            }
            else if (IsChunk(storedThing.def))
            {
                AddSpecial(storedThing, Chunks);
            }
            else
            {
                AddSpecial(storedThing, Misc);
            }
        }

        if (selectedTab == Tabs.InfiniteStorage_Misc && Misc.Count == 0 ||
            selectedTab == Tabs.InfiniteStorage_Minified && Minified.Count == 0 ||
            selectedTab == Tabs.InfiniteStorage_Apparel && Apparel.Count == 0 ||
            selectedTab == Tabs.InfiniteStorage_Weapons && Weapons.Count == 0 ||
            selectedTab == Tabs.InfiniteStorage_Chunks && Chunks.Count == 0)
        {
            selectedTab = Tabs.Unknown;
        }

        if (selectedTab == Tabs.Unknown)
        {
            if (Misc.Count > 0)
            {
                selectedTab = Tabs.InfiniteStorage_Misc;
            }
            else if (Minified.Count > 0)
            {
                selectedTab = Tabs.InfiniteStorage_Minified;
            }
            else if (Apparel.Count > 0)
            {
                selectedTab = Tabs.InfiniteStorage_Apparel;
            }
            else if (Weapons.Count > 0)
            {
                selectedTab = Tabs.InfiniteStorage_Weapons;
            }
            else if (Chunks.Count > 0)
            {
                selectedTab = Tabs.InfiniteStorage_Chunks;
            }
        }
    }

    private void AddSpecial(Thing t, Dictionary<string, Thing> d)
    {
        if (t.def.stackLimit == 1)
        {
            if (d.TryGetValue(t.def.defName, out var value) && value is MatchedThings things)
            {
                things.Add(t);
            }
            else
            {
                d[t.def.defName] = new MatchedThings(t);
            }
        }
        else
        {
            d[t.def.defName] = t;
        }
    }

    private bool IsChunk(ThingDef def)
    {
        foreach (var thingCategory in def.thingCategories)
        {
            if (thingCategory == ThingCategoryDefOf.Chunks || thingCategory == ThingCategoryDefOf.StoneChunks)
            {
                return true;
            }
        }

        return false;
    }

    public override void PreClose()
    {
        base.PreClose();
        Misc.Clear();
        Chunks.Clear();
        Minified.Clear();
        Apparel.Clear();
        Weapons.Clear();
        if (itemsDropped && InfiniteStorage != null)
        {
            InfiniteStorage.ResetAutoReclaimTime();
        }
    }

    public override void DoWindowContents(Rect inRect)
    {
        GUI.color = Color.white;
        Text.Font = GameFont.Small;
        try
        {
            var num = 90;
            int rows;
            var thingsToShow = GetThingsToShow(out rows);
            if (rows == 0)
            {
                return;
            }

            searchText = Widgets.TextEntryLabeled(new Rect(20f, 15f, 300f, 32f),
                "InfiniteStorage.Search".Translate() + ": ", searchText).ToLower().Trim();
            searchText = Regex.Replace(searchText, "\\t|\\n|\\r", "");
            if (searchText.Length == 0)
            {
                TabDrawer.DrawTabs(new Rect(0f, num, inRect.width, inRect.height - num), tabs);
                num += 32;
            }

            var rect = new Rect(0f, num, 368f, (rows + 1) * 32);
            scrollPosition = GUI.BeginScrollView(new Rect(50f, num, rect.width + 18f, inRect.height - num - 75f),
                scrollPosition, rect);
            var num2 = 0;
            foreach (var item in thingsToShow)
            {
                if (item == null)
                {
                    continue;
                }

                var text = FormatLabel(item);
                if (searchText.Length != 0 && !text.ToLower().Contains(searchText))
                {
                    continue;
                }

                if (DrawRow(item, text, num, num2, rect))
                {
                    break;
                }

                num2++;
            }

            GUI.EndScrollView();
        }
        catch (Exception ex)
        {
            var text2 = $"{GetType().Name} closed due to: {ex.GetType().Name} {ex.Message}";
            Log.Error(text2);
            Messages.Message(text2, MessageTypeDefOf.NegativeEvent);
            base.Close();
        }
        finally
        {
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }

    private bool DrawRow(Thing thing, string label, float y, int i, Rect r)
    {
        var thing2 = thing is MatchedThings things ? things.First : thing;
        GUI.BeginGroup(new Rect(0f, y + (i * 32), r.width, 30f));
        Widgets.ThingIcon(new Rect(0f, 0f, 30f, 30f), thing2);
        if (Widgets.InfoCardButton(40f, 0f, thing2))
        {
            Find.WindowStack.Add(new Dialog_InfoCard(thing2));
        }

        Widgets.Label(new Rect(70f, 0f, r.width - 110f, 30f), label);
        if (InfiniteStorage.IsOperational && Widgets.ButtonImage(new Rect(r.xMax - 20f, 0f, 20f, 20f), DropTexture))
        {
            InfiniteStorage.AllowAdds = false;
            if (InfiniteStorage.TryRemove(thing2))
            {
                BuildingUtil.DropThing(thing2, thing2.stackCount, InfiniteStorage, InfiniteStorage.Map);
                itemsDropped = true;
            }

            PopulateDisplayThings();
            return true;
        }

        GUI.EndGroup();
        return false;
    }

    private string FormatLabel(Thing thing)
    {
        var thing2 = thing is MatchedThings matchedThings ? matchedThings.First : thing;
        if (thing2 is MinifiedThing || thing2.def.IsApparel ||
            thing2.def.thingCategories.Contains(WeaponsMeleeCategoryDef) ||
            thing2.def.thingCategories.Contains(WeaponsRangedCategoryDef))
        {
            return thing2.Label;
        }

        var stringBuilder = new StringBuilder(thing2.def.label);
        if (thing2.Stuff != null)
        {
            stringBuilder.Append(" (");
            stringBuilder.Append(thing2.Stuff.LabelAsStuff);
            stringBuilder.Append(")");
        }

        if (thing is MatchedThings things)
        {
            stringBuilder.Append(" x");
            stringBuilder.Append(things.Count);
        }
        else if (thing2.stackCount > 0)
        {
            stringBuilder.Append(" x");
            stringBuilder.Append(thing2.stackCount);
        }

        return stringBuilder.ToString();
    }

    private IEnumerable<Thing> GetThingsToShow(out int rows)
    {
        IEnumerable<Thing> result;
        if (searchText.Length > 0)
        {
            result = InfiniteStorage.StoredThings;
            rows = InfiniteStorage.DefsCount;
        }
        else
        {
            tabs.Clear();
            if (Misc.Count > 0)
            {
                tabs.Add(new TabRecord(Tabs.InfiniteStorage_Misc.ToString().Translate(),
                    delegate { selectedTab = Tabs.InfiniteStorage_Misc; }, selectedTab == Tabs.InfiniteStorage_Misc));
            }

            if (Minified.Count > 0)
            {
                tabs.Add(new TabRecord(Tabs.InfiniteStorage_Minified.ToString().Translate(),
                    delegate { selectedTab = Tabs.InfiniteStorage_Minified; },
                    selectedTab == Tabs.InfiniteStorage_Minified));
            }

            if (Apparel.Count > 0)
            {
                tabs.Add(new TabRecord(Tabs.InfiniteStorage_Apparel.ToString().Translate(),
                    delegate { selectedTab = Tabs.InfiniteStorage_Apparel; },
                    selectedTab == Tabs.InfiniteStorage_Apparel));
            }

            if (Weapons.Count > 0)
            {
                tabs.Add(new TabRecord(Tabs.InfiniteStorage_Weapons.ToString().Translate(),
                    delegate { selectedTab = Tabs.InfiniteStorage_Weapons; },
                    selectedTab == Tabs.InfiniteStorage_Weapons));
            }

            if (Chunks.Count > 0)
            {
                tabs.Add(new TabRecord(Tabs.InfiniteStorage_Chunks.ToString().Translate(),
                    delegate { selectedTab = Tabs.InfiniteStorage_Chunks; },
                    selectedTab == Tabs.InfiniteStorage_Chunks));
            }

            if (selectedTab == Tabs.InfiniteStorage_Misc)
            {
                result = Misc.Values;
                rows = Misc.Count;
            }
            else if (selectedTab == Tabs.InfiniteStorage_Minified)
            {
                result = Minified;
                rows = Minified.Count;
            }
            else if (selectedTab == Tabs.InfiniteStorage_Apparel)
            {
                result = Apparel;
                rows = Apparel.Count;
            }
            else if (selectedTab == Tabs.InfiniteStorage_Weapons)
            {
                result = Weapons;
                rows = Weapons.Count;
            }
            else
            {
                result = Chunks.Values;
                rows = Chunks.Count;
            }
        }

        return result;
    }

    private class MatchedThings : Thing
    {
        public readonly List<Thing> Things = new List<Thing>();
        public int Count;

        public MatchedThings()
        {
        }

        public MatchedThings(Thing t)
        {
            Add(t);
        }

        public Thing First => Things[0];

        public void Add(Thing t)
        {
            Things.Add(t);
            Count++;
        }
    }

    private enum Tabs
    {
        Unknown,
        InfiniteStorage_Misc,
        InfiniteStorage_Minified,
        InfiniteStorage_Apparel,
        InfiniteStorage_Weapons,
        InfiniteStorage_Chunks
    }
}