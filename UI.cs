using System;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using RSDiagnostics.Util;

namespace RSDiagnostics
{
    public partial class MainForm : Form
    {
        readonly static byte[] CF_DLL_HASH = { 0x9A, 0xD0, 0xEF, 0x34, 0x9A, 0x8D, 0x2A, 0x89, 0x24, 0x35, 0xF6, 0x4D, 0xD2, 0x8D, 0x9C, 0x8D, 0xA3, 0x6B, 0xA1, 0xD3, 0x3A, 0xE3, 0xBF, 0xD0, 0x2A, 0x62, 0xF4, 0x2B, 0xC4, 0x3F, 0x08, 0x62 };
        public static DLLType DLLType = DLLType.None;

        public MainForm()
        {
            InitializeComponent();

            Settings.Settings.RefreshLocations();
            new Settings.Rocksmith.LoadSettings();

            new Log();
        }

        public static bool ValidCdlcDLL()
        {

            if (!File.Exists(Settings.Settings.CDLC_DLL))
            {
                DLLType = DLLType.None;
                return false;
            }
                
            try
            {
                X509Certificate2 cert = new X509Certificate2(X509Certificate.CreateFromSignedFile(Settings.Settings.CDLC_DLL));

                if (cert.GetNameInfo(X509NameType.SimpleName, false) == "Microsoft Corporation" || cert.Verify())
                {
                    DLLType = DLLType.Microsoft;
                    return false; // User is using an ACTUAL D3DX9_42.dll, not a CDLC hack.
                }
            }
            catch { } // This catch case is needed. We want an error to occur above.


            using (SHA256 sha256 = SHA256.Create())
            {
                FileStream dllStream = File.Open(Settings.Settings.CDLC_DLL, FileMode.Open);
                dllStream.Position = 0;

                byte[] hash = sha256.ComputeHash(dllStream);

                for(int i = 0; i < hash.Length; i++)
                {
                    if (hash[i] != CF_DLL_HASH[i])
                        break;

                    if (i == hash.Length - 1)
                    {
                        DLLType = DLLType.CustomsForge;
                        return true; // User is using the CustomsForge DLL.
                    }  
                }
            }

            if (File.GetCreationTime(Settings.Settings.CDLC_DLL) >= new DateTime(2020, 7, 1) && new FileInfo(Settings.Settings.CDLC_DLL).Length >= 300000)
            {
                DLLType = DLLType.RSMods;
                return true;
            }

            DLLType = DLLType.Unknown;
            return false;
        }
        
        public static bool ValidGame() => !(File.Exists(Path.Combine(Settings.Settings.RocksmithLocation, "IGG-GAMES.COM.url")) || File.Exists(Path.Combine(Settings.Settings.RocksmithLocation, "SmartSteamEmu.ini")) || File.Exists(Path.Combine(Settings.Settings.RocksmithLocation, "GAMESTORRENT.CO.url")) || File.Exists(Path.Combine(Settings.Settings.RocksmithLocation, "Codex.ini")) || File.Exists(Path.Combine(Settings.Settings.RocksmithLocation, "Skidrow.ini")));
    
    }

    public enum DLLType
    {
        CustomsForge,
        RSMods,
        Microsoft,
        Unknown,
        None
    };
}
