using System.Globalization;

namespace EndfieldCharge.Services;

public sealed record BatterySnapshot(double RemainingWh, double FullWh, int Percent, bool AcOnline, bool Charging)
{
    public double? RateWatts { get; init; }
    public double? DesignCapacityWh { get; init; }
    public double? HealthPercent => DesignCapacityWh > 0 ? Math.Round(FullWh / DesignCapacityWh.Value * 100, 1) : null;
    public TimeSpan? EstimatedRemaining { get; init; }
    public bool HasBattery { get; init; } = true;
}

/// <summary>Read-only sysfs backend. Energy is µWh; charge is µAh, not energy.</summary>
public static class BatteryService
{
    public static BatterySnapshot? GetSnapshot() => ReadSnapshot("/sys/class/power_supply");

    public static BatterySnapshot? ReadSnapshot(string root)
    {
        try
        {
            var devices = Directory.GetDirectories(root);
            var batteries = devices.Where(p => Read(p, "type") == "Battery"
                && Read(p, "present") != "0" && Read(p, "scope") != "Device").ToArray();
            if (batteries.Length == 0) return null;
            var online = devices.Where(p => Read(p, "type") != "Battery")
                .Select(p => Number(p, "online")).Where(v => v.HasValue).ToArray();
            var ac = online.Length > 0 ? online.Any(v => v > 0)
                : batteries.Any(p => Read(p, "status") is "Charging" or "Full" or "Not charging");
            double remaining = 0, full = 0, percentages = 0;
            bool allEnergy = true, charging = false;
            foreach (var battery in batteries)
            {
                var voltage = Number(battery, "voltage_min_design") ?? Number(battery, "voltage_now");
                var energyFull = Number(battery, "energy_full") / 1e6
                    ?? Number(battery, "charge_full") * voltage / 1e12;
                var energyNow = Number(battery, "energy_now") / 1e6
                    ?? Number(battery, "charge_now") * voltage / 1e12;
                var pct = Number(battery, "capacity")
                    ?? (energyFull > 0 ? energyNow / energyFull * 100 : null);
                if (!pct.HasValue) return null; // Don't fabricate missing readings.
                percentages += Math.Clamp(pct.Value, 0, 100);
                if (energyFull > 0)
                {
                    full += energyFull.Value;
                    remaining += energyNow ?? energyFull.Value * pct.Value / 100;
                }
                else allEnergy = false;
                charging |= Read(battery, "status") == "Charging";
            }
            int percent = (int)Math.Round(allEnergy && full > 0
                ? remaining / full * 100 : percentages / batteries.Length);
            return new BatterySnapshot(allEnergy ? remaining : 0, allEnergy ? full : 0,
                Math.Clamp(percent, 0, 100), ac, charging);
        }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }

    private static string? Read(string directory, string name)
    {
        try { return File.ReadAllText(Path.Combine(directory, name)).Trim(); }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }

    private static double? Number(string directory, string name) =>
        double.TryParse(Read(directory, name), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            && double.IsFinite(value) && value >= 0 ? value : null;
}
