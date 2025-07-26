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
    }
}