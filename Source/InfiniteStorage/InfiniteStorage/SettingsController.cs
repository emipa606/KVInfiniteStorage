using Mlie;
using UnityEngine;
using Verse;

namespace InfiniteStorage;

public class SettingsController : Mod
{
    public static string currentVersion;

    public SettingsController(ModContentPack content)
        : base(content)
    {
        GetSettings<Settings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    public override string SettingsCategory()
    {
        return "InfiniteStorage".Translate();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.DoSettingsWindowContents(inRect);
    }
}