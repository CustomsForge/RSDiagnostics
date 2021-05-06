using System.IO;

namespace RSDiagnostics.Settings
{
    public static class Settings
    {
        public static string RocksmithLocation = "";

        // Standard Rocksmith Information
        public static string SETTINGS_Rocksmith = string.Empty;
        public static string DLL_CDLC = string.Empty;

        // RS_ASIO
        public static string DLL_Asio_AVRT = string.Empty;
        public static string DLL_Asio_RSASIO = string.Empty;
        public static string SETTINGS_Asio = string.Empty;

        public static bool HasValidSettingsFile(string SettingsFile) => File.Exists(SettingsFile) && File.ReadAllText(SettingsFile).Length > 0;
        public static void RefreshLocations()
        {
            RocksmithLocation = Util.GenUtil.GetRSDirectory();

            // Standard Rocksmith Information
            SETTINGS_Rocksmith = Path.Combine(RocksmithLocation, "Rocksmith.ini");
            DLL_CDLC = Path.Combine(RocksmithLocation, "D3DX9_42.dll");

            // RS_Asio
            DLL_Asio_AVRT = Path.Combine(RocksmithLocation, "avrt.dll");
            DLL_Asio_RSASIO = Path.Combine(RocksmithLocation, "RS_ASIO.dll");
            SETTINGS_Asio = Path.Combine(RocksmithLocation, "RS_ASIO.ini");
        }
    }
}
