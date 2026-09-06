using EndfieldCharge.Services;

var root = Path.Combine(Path.GetTempPath(), "endfield-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try
{
    void Put(string device, string name, string value)
    {
        Directory.CreateDirectory(Path.Combine(root, device));
        File.WriteAllText(Path.Combine(root, device, name), value);
    }
    void Check(bool ok, string name)
    {
        if (!ok) throw new Exception(name);
        Console.WriteLine("PASS: " + name);
    }
    Check(BatteryService.ReadSnapshot(root) is null, "No battery");
    Put("BAT0", "type", "Battery");
    Put("BAT0", "energy_now", "30000000");
    Put("BAT0", "energy_full", "60000000");
    Put("BAT0", "status", "Charging");
    Put("AC", "type", "Mains");
    Put("AC", "online", "1");
    var s = BatteryService.ReadSnapshot(root)!;
    Check(s is { Percent: 50, RemainingWh: 30, FullWh: 60, AcOnline: true, Charging: true }, "Energy units and AC");
    Put("AC", "online", "0");
    Put("BAT0", "status", "Discharging");
    Check(BatteryService.ReadSnapshot(root) is { AcOnline: false, Charging: false }, "Unplug");
    Put("BAT0", "energy_now", "invalid");
    Put("BAT0", "energy_full", "invalid");
    Put("BAT0", "charge_now", "2500000");
    Put("BAT0", "charge_full", "5000000");
    Put("BAT0", "voltage_min_design", "12000000");
    Check(BatteryService.ReadSnapshot(root) is { RemainingWh: 30, FullWh: 60, Percent: 50 }, "Charge conversion");
    Put("BAT1", "type", "Battery");
    Put("BAT1", "energy_now", "10000000");
    Put("BAT1", "energy_full", "40000000");
    Check(BatteryService.ReadSnapshot(root) is { RemainingWh: 40, FullWh: 100, Percent: 40 }, "Weighted multi-battery");
    Put("BAT1", "scope", "Device");
    Check(BatteryService.ReadSnapshot(root) is { Percent: 50 }, "Ignore peripheral batteries");
    Put("BAT0", "voltage_min_design", "invalid");
    Put("BAT0", "capacity", "73");
    Check(BatteryService.ReadSnapshot(root) is { Percent: 73, FullWh: 0, HasBattery: true }, "Percentage-only battery");
    Put("BAT0", "present", "0");
    Check(BatteryService.ReadSnapshot(root) is null, "Absent battery");
}
finally { Directory.Delete(root, recursive: true); }
