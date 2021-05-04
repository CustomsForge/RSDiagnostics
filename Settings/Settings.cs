using System.IO;

namespace RSDiagnostics.Settings
{
    public static class Settings
    {
        public static string RocksmithLocation = "";

        public static string SETTINGS_Rocksmith = string.Empty;
        public static string CDLC_DLL = string.Empty;

        public static bool HasValidSettingsFile(string SettingsFile) => File.Exists(SettingsFile) && File.ReadAllText(SettingsFile).Length > 0;
        public static void RefreshLocations()
        {
            RocksmithLocation = Util.GenUtil.GetRSDirectory();
            SETTINGS_Rocksmith = Path.Combine(RocksmithLocation, "Rocksmith.ini");
            CDLC_DLL = Path.Combine(RocksmithLocation, "D3DX9_42.dll");
        }
    }
}
