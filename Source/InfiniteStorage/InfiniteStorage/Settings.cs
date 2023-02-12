using RimWorld;
using UnityEngine;
using Verse;

namespace InfiniteStorage;

public class Settings : ModSettings
{
    private const int DEFAULT_ENERGY_BUFFER = 100;

    private const float DEFAULT_ENERGY_FACTOR = 1f;

    private const long DEFAULT_TIME_BETWEEN_COLLECTS_TICKS = 100000000L;

    private static bool enableEnergyBuffer = true;

    private static int desiredEnergyBuffer = 100;

    private static float energyFactor = 1f;

    private static bool emptyOnPowerLoss;

    private static bool collectThingsAutomatically = true;

    private static long timeBetweenAutoCollects = 100000000L;

    private static string desiredEnergyBufferUserInput = desiredEnergyBuffer.ToString();

    private static string energyFactorUserInput = energyFactor.ToString();

    private static string timeBetweenAutoCollectsUserInput = TimeBetweenAutoCollectsSeconds.ToString();

    public static bool EnableEnergyBuffer => enableEnergyBuffer;

    public static int DesiredEnergyBuffer => desiredEnergyBuffer;

    public static float EnergyFactor => energyFactor;

    public static bool EmptyOnPowerLoss => emptyOnPowerLoss;

    public static bool CollectThingsAutomatically => collectThingsAutomatically;

    public static long TimeBetweenAutoCollectsTicks => timeBetweenAutoCollects;

    private static long TimeBetweenAutoCollectsSeconds => timeBetweenAutoCollects / 10000000;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref enableEnergyBuffer, "InfiniteStorage.EnableEnergyBuffer", true, true);
        Scribe_Values.Look(ref desiredEnergyBuffer, "InfiniteStorage.DesiredEnergyBuffer", 100, true);
        Scribe_Values.Look(ref energyFactor, "InfiniteStorage.EnergyFactor", 1f, true);
        Scribe_Values.Look(ref emptyOnPowerLoss, "InfiniteStorage.EmptyOnPowerLoss", false, true);
        Scribe_Values.Look(ref collectThingsAutomatically, "InfiniteStorage.CollectThingsAutomatically", true, true);
        Scribe_Values.Look(ref timeBetweenAutoCollects, "InfiniteStorage.TimeBetweenAutoCollects", 100000000L, true);
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            return;
        }

        desiredEnergyBufferUserInput = desiredEnergyBuffer.ToString();
        energyFactorUserInput = energyFactor.ToString();
        timeBetweenAutoCollectsUserInput = TimeBetweenAutoCollectsSeconds.ToString();
    }

    public static void DoSettingsWindowContents(Rect rect)
    {
        var num = 60;
        Widgets.CheckboxLabeled(new Rect(0f, num, 300f, 32f), "InfiniteStorage.EnableEnergyBuffer".Translate(),
            ref enableEnergyBuffer);
        if (enableEnergyBuffer)
        {
            num += 40;
            desiredEnergyBufferUserInput = Widgets.TextEntryLabeled(new Rect(0f, num, 300f, 32f),
                "InfiniteStorage.EnergyBuffer".Translate() + ":   ", desiredEnergyBufferUserInput);
            num += 50;
            if (Widgets.ButtonText(new Rect(50f, num, 100f, 32f), "Confirm".Translate()))
            {
                if (!int.TryParse(desiredEnergyBufferUserInput, out var result) || result < 0)
                {
                    Messages.Message("InfiniteStorage.NumberGreaterThanZero".Translate(), MessageTypeDefOf.RejectInput);
                }
                else
                {
                    desiredEnergyBuffer = result;
                    Messages.Message(
                        "InfiniteStorage.EnergyBufferSet".Translate().Replace("{v}", desiredEnergyBuffer.ToString()),
                        MessageTypeDefOf.PositiveEvent);
                }
            }

            if (Widgets.ButtonText(new Rect(175f, num, 100f, 32f), "default".Translate().CapitalizeFirst()))
            {
                desiredEnergyBuffer = 100;
                desiredEnergyBufferUserInput = desiredEnergyBuffer.ToString();
                Messages.Message(
                    "InfiniteStorage.EnergyBufferSet".Translate().Replace("{v}", desiredEnergyBuffer.ToString()),
                    MessageTypeDefOf.PositiveEvent);
            }
        }

        num += 50;
        Widgets.Label(new Rect(25f, num, rect.width - 50f, 32f), "InfiniteStorage.EnergyBufferDesc".Translate());
        num += 29;
        Widgets.DrawLineHorizontal(0f, num, rect.width);
        num += 29;
        energyFactorUserInput = Widgets.TextEntryLabeled(new Rect(0f, num, 300f, 32f),
            "InfiniteStorage.EnergyFactor".Translate() + ":   ", energyFactorUserInput);
        num += 50;
        if (Widgets.ButtonText(new Rect(50f, num, 100f, 32f), "Confirm".Translate()))
        {
            if (!float.TryParse(energyFactorUserInput, out var result2) || result2 < 0f)
            {
                Messages.Message("InfiniteStorage.NumberGreaterThanZero".Translate(), MessageTypeDefOf.RejectInput);
            }
            else
            {
                energyFactor = result2;
                Messages.Message("InfiniteStorage.EnergyFactorSet".Translate().Replace("{v}", energyFactor.ToString()),
                    MessageTypeDefOf.PositiveEvent);
            }
        }

        if (Widgets.ButtonText(new Rect(175f, num, 100f, 32f), "default".Translate().CapitalizeFirst()))
        {
            energyFactor = 1f;
            energyFactorUserInput = energyFactor.ToString();
            Messages.Message("InfiniteStorage.EnergyFactorSet".Translate().Replace("{v}", energyFactor.ToString()),
                MessageTypeDefOf.PositiveEvent);
        }

        num += 50;
        Widgets.Label(new Rect(25f, num, rect.width - 50f, 32f), "InfiniteStorage.EnergyFactorDesc".Translate());
        num += 29;
        Widgets.CheckboxLabeled(new Rect(25f, num, 300f, 32f), "InfiniteStorage.EmptyOnPowerLoss".Translate(),
            ref emptyOnPowerLoss);
        num += 29;
        Widgets.DrawLineHorizontal(0f, num, rect.width);
        num += 29;
        Widgets.CheckboxLabeled(new Rect(25f, num, 200f, 32f), "InfiniteStorage.CollectThingsAutomatically".Translate(),
            ref collectThingsAutomatically);
        num += 40;
        if (!collectThingsAutomatically)
        {
            return;
        }

        Widgets.Label(new Rect(25f, num, 300f, 32f), "InfiniteStorage.TimeBetweenAutoCollects".Translate() + ":");
        timeBetweenAutoCollectsUserInput =
            Widgets.TextField(new Rect(310f, num - 6, 75f, 32f), timeBetweenAutoCollectsUserInput);
        num += 40;
        if (Widgets.ButtonText(new Rect(50f, num, 100f, 32f), "Confirm".Translate()))
        {
            if (!long.TryParse(timeBetweenAutoCollectsUserInput, out var result3) || result3 < 0)
            {
                Messages.Message("InfiniteStorage.NumberGreaterThanZero".Translate(), MessageTypeDefOf.RejectInput);
            }
            else
            {
                timeBetweenAutoCollects = result3 * 10000000;
                Messages.Message(
                    "InfiniteStorage.TimeBetweenAutoCollectsSet".Translate()
                        .Replace("{v}", TimeBetweenAutoCollectsSeconds.ToString()), MessageTypeDefOf.PositiveEvent);
            }
        }

        if (Widgets.ButtonText(new Rect(175f, num, 100f, 32f), "default".Translate().CapitalizeFirst()))
        {
            timeBetweenAutoCollects = 100000000L;
            timeBetweenAutoCollectsUserInput = TimeBetweenAutoCollectsSeconds.ToString();
            Messages.Message(
                "InfiniteStorage.TimeBetweenAutoCollectsSet".Translate()
                    .Replace("{v}", TimeBetweenAutoCollectsSeconds.ToString()), MessageTypeDefOf.PositiveEvent);
        }

        if (SettingsController.currentVersion == null)
        {
            return;
        }

        num += 40;
        GUI.contentColor = Color.gray;
        Widgets.Label(new Rect(25f, num, 300f, 32f),
            "InfiniteStorage.CurrentModVersion".Translate(SettingsController.currentVersion));
        GUI.contentColor = Color.white;
    }
}