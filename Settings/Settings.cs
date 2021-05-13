using System.IO;

namespace RSDiagnostics.Settings
{
    public static class Settings
    {
        public static string RocksmithLocation = string.Empty;

        // Standard Rocksmith Information
        public static string SETTINGS_Rocksmith = string.Empty; // Rocksmith.ini
        public static string DLL_CDLC = string.Empty; // D3DX9_42.dll
        public static string EXE = string.Empty; // Rocksmith2014.exe

        // RS_ASIO
        public static string DLL_Asio_AVRT = string.Empty; // avrt.dll
        public static string DLL_Asio_RSASIO = string.Empty; // RS_ASIO.dll
        public static string SETTINGS_Asio = string.Empty; // RS_ASIO.ini

        /// <summary>
        /// Makes sure the user isn't providing fake files.
        /// </summary>
        /// <param name="SettingsFile"> - File to check</param>
        /// <returns>Does the file exist, and is it not blank.</returns>
        public static bool HasValidSettingsFile(string SettingsFile) => File.Exists(SettingsFile) && File.ReadAllText(SettingsFile).Length > 0;

        /// <summary>
        /// Adjust Rocksmith's install location, and changes all the file locations.
        /// </summary>
        public static void RefreshLocations()
        {
            RocksmithLocation = Util.GenUtil.GetRSDirectory();

            // Standard Rocksmith Information
            SETTINGS_Rocksmith = Path.Combine(RocksmithLocation, "Rocksmith.ini");
            DLL_CDLC = Path.Combine(RocksmithLocation, "D3DX9_42.dll");
            EXE = Path.Combine(RocksmithLocation, "Rocksmith2014.exe");

            // RS_Asio
            DLL_Asio_AVRT = Path.Combine(RocksmithLocation, "avrt.dll");
            DLL_Asio_RSASIO = Path.Combine(RocksmithLocation, "RS_ASIO.dll");
            SETTINGS_Asio = Path.Combine(RocksmithLocation, "RS_ASIO.ini");
        }
    }
}
