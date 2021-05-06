using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RSDiagnostics
{
    public class Log
    {
        readonly static string outputFile = "output.log";

        public Log()
        {
            Init();
            DumpRocksmithINI();
            DumpASIO();
            Songs();
        }

        void Init()
        {
            using (StreamWriter sw = File.CreateText(outputFile))
            {
                sw.WriteLine("Rocksmith Location: " + Settings.Settings.RocksmithLocation);
                sw.WriteLine("Valid CDLC DLL: " + MainForm.ValidCdlcDLL().ToString());
                sw.WriteLine("DLL Type: " + MainForm.DLLType);
                sw.WriteLine("Valid Game: " + MainForm.ValidGame.ToString());
                sw.WriteLine("\n");
            }
        }

        void DumpRocksmithINI()
        {
            using (StreamWriter sw = File.AppendText(outputFile))
            {
                sw.WriteLine("Rocksmith.ini is as follows");
                string section = string.Empty;
                foreach(Settings.Rocksmith.Settings setting in Settings.Rocksmith.LoadSettings.LoadedSettings)
                {
                    if (setting.Section != section)
                    {
                        section = setting.Section;
                        sw.WriteLine("  " + "[" + section + "]");
                    }

                    sw.WriteLine("  " + setting.SettingName + "=" + setting.Value);
                }
                sw.WriteLine("\n");
            }
        }

        void DumpASIO()
        {
            using (StreamWriter sw = File.AppendText(outputFile))
            {
                sw.WriteLine("RS_ASIO.ini is as follows");
                string section = string.Empty;
                foreach (Settings.Asio.Settings setting in Settings.Asio.LoadSettings.LoadedSettings)
                {
                    if (setting.Section != section)
                    {
                        section = setting.Section;
                        sw.WriteLine("  " + "[" + section + "]");
                    }

                    sw.WriteLine("  " + setting.SettingName + "=" + setting.Value);
                }
                sw.WriteLine("\n");
            }
        }

        void Songs()
        {
            List<SongData> ODLC = SongManager.Songs.Where(song => song.Value.ODLC == true && song.Value.RS1AppID == 0).Select(pair => pair.Value).ToList();
            using (StreamWriter sw = File.AppendText(outputFile))
            {
                sw.WriteLine("Total Songs: " + SongManager.Songs.Count);
                sw.WriteLine("Non-Authentic ODLC: " + SongManager.Validate(ODLC));
            }
        }
    }
}
