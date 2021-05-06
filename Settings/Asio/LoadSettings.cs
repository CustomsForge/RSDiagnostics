using System.Collections.Generic;
using System.IO;

namespace RSDiagnostics.Settings.Asio
{
    public class LoadSettings
    {
        public LoadSettings()
        {
            LoadedSettings.Clear();

            // Config
            LoadedSettings.Add(new Settings("Config", "EnableWasapiOutputs", 0));
            LoadedSettings.Add(new Settings("Config", "EnableWasapiInputs", 0));
            LoadedSettings.Add(new Settings("Config", "EnableAsio", 1));

            // Asio
            LoadedSettings.Add(new Settings("Asio", "BufferSizeMode", "driver"));
            LoadedSettings.Add(new Settings("Asio", "CustomBufferSize", ""));

            // Asio.Output
            LoadedSettings.Add(new Settings("Asio.Output", "Driver", ""));
            LoadedSettings.Add(new Settings("Asio.Output", "BaseChannel", 0));
            LoadedSettings.Add(new Settings("Asio.Output", "AltBaseChannel", ""));
            LoadedSettings.Add(new Settings("Asio.Output", "EnableSoftwareEndpointVolumeControl", 1));
            LoadedSettings.Add(new Settings("Asio.Output", "EnableSoftwareMasterVolumeControl", 1));
            LoadedSettings.Add(new Settings("Asio.Output", "SoftwareMasterVolumePercent", 100));

            // Asio.Input.0
            LoadedSettings.Add(new Settings("Asio.Input.0", "Driver", ""));
            LoadedSettings.Add(new Settings("Asio.Input.0", "Channel", 0));
            LoadedSettings.Add(new Settings("Asio.Input.0", "EnableSoftwareEndpointVolumeControl", 1));
            LoadedSettings.Add(new Settings("Asio.Input.0", "EnableSoftwareMasterVolumeControl", 1));
            LoadedSettings.Add(new Settings("Asio.Input.0", "SoftwareMasterVolumePercent", 100));

            // Asio.Input.1
            LoadedSettings.Add(new Settings("Asio.Input.1", "Driver", ""));
            LoadedSettings.Add(new Settings("Asio.Input.1", "Channel", 1));
            LoadedSettings.Add(new Settings("Asio.Input.1", "EnableSoftwareEndpointVolumeControl", 1));
            LoadedSettings.Add(new Settings("Asio.Input.1", "EnableSoftwareMasterVolumeControl", 1));
            LoadedSettings.Add(new Settings("Asio.Input.1", "SoftwareMasterVolumePercent", 100));

            // Asio.Input.Mic
            LoadedSettings.Add(new Settings("Asio.Input.Mic", "Driver", ""));
            LoadedSettings.Add(new Settings("Asio.Input.Mic", "Channel", 1));
            LoadedSettings.Add(new Settings("Asio.Input.Mic", "EnableSoftwareEndpointVolumeControl", 1));
            LoadedSettings.Add(new Settings("Asio.Input.Mic", "EnableSoftwareMasterVolumeControl", 1));
            LoadedSettings.Add(new Settings("Asio.Input.Mic", "SoftwareMasterVolumePercent", 100));


            WriteSettingsFile();
        }

        public static List<Settings> LoadedSettings = new List<Settings>();
        public static Dictionary<string, Dictionary<string, object>> SettingsFile_Cache = new Dictionary<string, Dictionary<string, object>>();

        public static void WriteSettingsFile(Settings changedSettings = null)
        {
            using (StreamWriter sw = File.CreateText(RSDiagnostics.Settings.Settings.SETTINGS_Asio))
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
                            sw.WriteLine(setting.SettingName + "=" + setting.DefaultValue);
                        else
                            sw.WriteLine(setting.SettingName + "=" + setting.Value);
                    }
                }
            }
        }

    }
}
