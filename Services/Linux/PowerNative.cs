namespace EndfieldCharge.Services;

internal static class PowerNative
{
    public static bool TryGetAcOnline(out bool acOnline)
    {
        var snapshot = BatteryService.GetSnapshot();
        acOnline = snapshot?.AcOnline ?? false;
        return snapshot is not null;
    }
}
