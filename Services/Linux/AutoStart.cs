namespace EndfieldCharge.Services;

public static class AutoStart
{
    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "autostart", "endfield-charge.desktop");
    public static bool IsEnabled() => File.Exists(FilePath);
    public static string CurrentExePath => Path.Combine(AppContext.BaseDirectory, "EndfieldCharge");
    public static void Enable(string exePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        // Desktop-entry escaping, followed by Exec argument quoting.
        var escaped = exePath.Replace("\\", "\\\\").Replace("\"", "\\\"")
            .Replace("`", "\\`").Replace("$", "\\$").Replace("%", "%%");
        File.WriteAllText(FilePath, "[Desktop Entry]\nType=Application\nName=EndfieldCharge\n"
            + $"Exec=\"{escaped}\"\nTerminal=false\n");
    }
    public static void Disable() { if (File.Exists(FilePath)) File.Delete(FilePath); }
}
