using System;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace RSDiagnostics
{
    public partial class MainForm : Form
    {
        readonly static byte[] HASH_D3DX9_CF = { 0x9A, 0xD0, 0xEF, 0x34, 0x9A, 0x8D, 0x2A, 0x89, 0x24, 0x35, 0xF6, 0x4D, 0xD2, 0x8D, 0x9C, 0x8D, 0xA3, 0x6B, 0xA1, 0xD3, 0x3A, 0xE3, 0xBF, 0xD0, 0x2A, 0x62, 0xF4, 0x2B, 0xC4, 0x3F, 0x08, 0x62 };
        readonly static byte[] HASH_D3DX9_P7 = { 0x0B, 0x94, 0x00, 0x35, 0x8A, 0x20, 0xDA, 0x1A, 0xDB, 0x18, 0x27, 0x53, 0x69, 0x46, 0x38, 0xCB, 0x0A, 0xAB, 0x5C, 0x43, 0x1A, 0x23, 0x6E, 0x12, 0xD0, 0xB7, 0x15, 0x22, 0xC6, 0x89, 0x02, 0x2A };
        public static DLLType DLLType = DLLType.None;

        public static bool ValidGame = !(File.Exists(Path.Combine(Settings.Settings.RocksmithLocation, "IGG-GAMES.COM.url")) || File.Exists(Path.Combine(Settings.Settings.RocksmithLocation, "SmartSteamEmu.ini")) || File.Exists(Path.Combine(Settings.Settings.RocksmithLocation, "GAMESTORRENT.CO.url")) || File.Exists(Path.Combine(Settings.Settings.RocksmithLocation, "Codex.ini")) || File.Exists(Path.Combine(Settings.Settings.RocksmithLocation, "Skidrow.ini")));

        /// <summary>
        /// Startup GUI
        /// </summary>
        public MainForm()
        {

            InitializeComponent();

            Settings.Settings.RefreshLocations();

            // Load Rocksmith Settings
            new Settings.Rocksmith.LoadSettings();

            // Load RS_ASIO if we see it exists.
            if (File.Exists(Settings.Settings.DLL_Asio_RSASIO) || File.Exists(Settings.Settings.DLL_Asio_AVRT))
            {
                new Settings.Asio.LoadSettings();
                new Settings.Asio.VerifySettings();
            }
                
            // Get data from songs
            SongManager.ExtractSongData();

            // Dump to log
            new Log();

            // Close GUI since it's unused (AS OF NOW)
            Environment.Exit(1);
        }

        /// <summary>
        /// <para>Looks over the "D3DX9_42.dll" in the user's Rocksmith folder to see if the user has a DLL that will enable CDLC.</para> 
        /// <para>The type of CDLC enabling DLL is posted into the DLLType variable.</para>
        /// </summary>
        /// <returns>Can CDLC be played?</returns>
        public static bool ValidCdlcDLL()
        {

            // Does the "D3DX9_42.dll" even exist?
            if (!File.Exists(Settings.Settings.DLL_CDLC))
            {
                DLLType = DLLType.None;
                return false;
            }
                
            // Is this the Microsoft "D3DX9_42.dll"?
            try
            {
                X509Certificate2 cert = new X509Certificate2(X509Certificate.CreateFromSignedFile(Settings.Settings.DLL_CDLC));

                if (cert.GetNameInfo(X509NameType.SimpleName, false) == "Microsoft Corporation" || cert.Verify())
                {
                    DLLType = DLLType.Microsoft;
                    return false; // User is using an ACTUAL D3DX9_42.dll, not a CDLC hack.
                }
            }
            catch { } // We want this function to error out. If it does, that means we have a non-Microsoft DLL.


            // Is the user using the normal, CustomsForge, "D3DX9_42.dll"?
            using (SHA256 sha256 = SHA256.Create())
            {
                FileStream dllStream = File.Open(Settings.Settings.DLL_CDLC, FileMode.Open);
                dllStream.Position = 0;

                byte[] hash = sha256.ComputeHash(dllStream);

                for(int i = 0; i < hash.Length; i++)
                {
                    if (hash[i] != HASH_D3DX9_CF[i])
                        break;

                    if (i == hash.Length - 1)
                    {
                        DLLType = DLLType.CustomsForge;
                        return true; // User is using the CustomsForge DLL.
                    }  
                }

                for (int i = 0; i < hash.Length; i++)
                {
                    if (hash[i] != HASH_D3DX9_P7[i])
                        break;

                    if (i == hash.Length - 1)
                    {
                        DLLType = DLLType.Patch7;
                        ValidGame = false;
                        return false; // User is using the Patch7 DLL
                    }
                }
            }

            // Is the user using a RSMods version of the "D3DX9_42.dll"?
            if (File.GetCreationTime(Settings.Settings.DLL_CDLC) >= new DateTime(2020, 7, 1) && new FileInfo(Settings.Settings.DLL_CDLC).Length >= 300000)
            {
                DLLType = DLLType.RSMods;
                return true;
            }

            // The user is using a different type of "D3DX9_42.dll". This could be off google from some random site, or potentially a pirate DLL.
            DLLType = DLLType.Unknown;
            return false;
        }
    
    }

    /// <summary>
    /// D3DX9_42.dll types
    /// </summary>
    public enum DLLType
    {
        CustomsForge,
        RSMods,
        Microsoft,
        Patch7,
        Unknown,
        None
    };
}
