namespace EndfieldCharge.Services;

public static class AutoStart
{
    private static string ConfigHome
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (!string.IsNullOrWhiteSpace(configured) && Path.IsPathRooted(configured))
                return configured;

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config");
        }
    }

    private static string FilePath => Path.Combine(ConfigHome, "autostart", "endfield-charge.desktop");
    public static bool IsEnabled() => File.Exists(FilePath);
    public static string CurrentExePath => Path.Combine(AppContext.BaseDirectory, "EndfieldCharge");
    public static void Enable(string exePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        // Desktop-entry escaping, followed by Exec argument quoting.
        var escaped = exePath.Replace("\\", "\\\\").Replace("\"", "\\\"")
            .Replace("`", "\\`").Replace("$", "\\$").Replace("%", "%%");
        File.WriteAllText(FilePath, "[Desktop Entry]\nType=Application\nName=EndfieldCharge\n"
            + $"Exec=\"{escaped}\"\nTerminal=false\nHidden=false\nX-GNOME-Autostart-enabled=true\n");
    }
    public static void Disable() { if (File.Exists(FilePath)) File.Delete(FilePath); }
}
