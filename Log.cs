using System.IO;

namespace RSDiagnostics
{
    public class Log
    {
        public Log()
        {
            using (StreamWriter sw = File.CreateText("output.log"))
            {
                sw.WriteLine("Rocksmith Location: " + Settings.Settings.RocksmithLocation);
                sw.WriteLine("Valid CDLC DLL: " + MainForm.ValidCdlcDLL().ToString());
                sw.WriteLine("DLL Type: " + MainForm.DLLType);
                sw.WriteLine("Valid Game: " + MainForm.ValidGame().ToString());
                sw.WriteLine("Exclusive Mode: " + Settings.Rocksmith.Settings.WhereSettingName("ExclusiveMode").Value.ToString());

            }
        }
    }
}
