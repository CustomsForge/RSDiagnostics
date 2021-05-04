using System;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace RSDiagnostics
{
    public partial class MainForm : Form
    {
        readonly byte[] CF_DLL_HASH = { 0x9A, 0xD0, 0xEF, 0x34, 0x9A, 0x8D, 0x2A, 0x89, 0x24, 0x35, 0xF6, 0x4D, 0xD2, 0x8D, 0x9C, 0x8D, 0xA3, 0x6B, 0xA1, 0xD3, 0x3A, 0xE3, 0xBF, 0xD0, 0x2A, 0x62, 0xF4, 0x2B, 0xC4, 0x3F, 0x08, 0x62 };
        DLLType DLLType = DLLType.None;

        public MainForm()
        {
            InitializeComponent();

            MessageBox.Show(ValidCdlcDLL().ToString() + "\n" + DLLType.ToString());
            new Settings.Rocksmith.LoadSettings();
        }

        bool ValidCdlcDLL()
        {
            string location = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Rocksmith2014\\";
            string DLL = "D3DX9_42.dll";

            if (!File.Exists(Path.Combine(location, DLL)))
            {
                DLLType = DLLType.None;
                return false;
            }
                
            try
            {
                X509Certificate2 cert = new X509Certificate2(X509Certificate.CreateFromSignedFile(Path.Combine(location, DLL)));

                if (cert.GetNameInfo(X509NameType.SimpleName, false) == "Microsoft Corporation" || cert.Verify())
                {
                    DLLType = DLLType.Microsoft;
                    return false; // User is using an ACTUAL D3DX9_42.dll, not a CDLC hack.
                }
            }
            catch { } // We want this catch case!


            using (SHA256 sha256 = SHA256.Create())
            {
                FileStream dllStream = File.Open(Path.Combine(location, DLL), FileMode.Open);
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

            if (File.GetCreationTime(Path.Combine(location, DLL)) >= new DateTime(2020, 7, 1) && new FileInfo(Path.Combine(location, DLL)).Length >= 300000)
            {
                DLLType = DLLType.RSMods;
                return true;
            }


            DLLType = DLLType.Unknown;
            return false;
        }
    }

    enum DLLType
    {
        CustomsForge,
        RSMods,
        Microsoft,
        Unknown,
        None
    };
}
