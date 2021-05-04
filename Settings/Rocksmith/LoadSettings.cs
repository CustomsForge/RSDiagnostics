using System.Collections.Generic;
using System.IO;

namespace RSDiagnostics.Settings.Rocksmith
{
    class LoadSettings
    {
        public LoadSettings()
        {
            LoadedSettings.Clear();

            LoadedSettings.Add(new Settings("Audio", "EnableMicrophone", 0));


            WriteSettingsFile();
        }

        public static List<Settings> LoadedSettings = new List<Settings>();

        public static Dictionary<string, object> SettingsFile_Cache = new Dictionary<string, object>();

        public static void WriteSettingsFile(Settings changedSettings = null)
        {
            using (StreamWriter sw = File.CreateText(RSDiagnostics.Settings.Settings.SETTINGS_Rocksmith))
            {
                Dictionary<string, List<Settings>> splitSettingsIntoSections = new Dictionary<string, List<Settings>>();

                foreach (Settings setting in LoadedSettings)
                {
                    if (splitSettingsIntoSections.ContainsKey(setting.Section))
                        splitSettingsIntoSections[setting.Section].Add(setting);
                    else
                        splitSettingsIntoSections.Add(setting.Section, new List<Settings> { setting });
                }

                if (changedSettings != null)
                    splitSettingsIntoSections[changedSettings.Section][splitSettingsIntoSections[changedSettings.Section].FindIndex(setting => setting.SettingName == changedSettings.SettingName)] = changedSettings;

                foreach (string section in splitSettingsIntoSections.Keys)
                {
                    sw.WriteLine("[" + section + "]");

                    foreach (Settings setting in splitSettingsIntoSections[section])
                    {
                        if (setting.Value == null)
                            sw.WriteLine(setting.SettingName + " = " + setting.DefaultValue);
                        else
                            sw.WriteLine(setting.SettingName + " = " + setting.Value);
                    }
                }
            }
        }
    }
}
