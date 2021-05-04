using System.IO;

namespace RSDiagnostics.Settings
{
    public static class Settings
    {
        public static string SETTINGS_Rocksmith = "Rocksmith.ini";

        public static bool HasValidSettingsFile(string SettingsFile) => File.Exists(SettingsFile) && File.ReadAllText(SettingsFile).Length > 0;
    }
}
