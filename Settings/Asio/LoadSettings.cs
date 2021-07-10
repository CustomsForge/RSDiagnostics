using System.Collections.Generic;
using System.IO;

namespace RSDiagnostics.Settings.Asio
{
    /// <summary>
    /// Load, cache, and write all RS_ASIO Settings
    /// </summary>
    public class LoadSettings
    {

        /// <summary>
        /// Load RS_ASIO Settings from the Settings File.
        /// </summary>
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
        
        /// <summary>
        /// List of RS_ASIO Settings
        /// </summary>
        public static List<Settings> LoadedSettings = new List<Settings>();

        /// <summary>
        /// Cache of RS_ASIO Settings File.
        /// </summary>
        public static Dictionary<string, Dictionary<string, object>> SettingsFile_Cache = new Dictionary<string, Dictionary<string, object>>();

        /// <summary>
        /// Creates / Edits the RS_ASIO Settings File To The Current Mod Settings.
        /// </summary>
        /// <param name="changedSettings">Setting adjusted, to be replaced in the settings file.</param>
        public static void WriteSettingsFile(Settings changedSettings = null)
        {
            using (StreamWriter sw = File.CreateText(RSDiagnostics.Settings.Settings.SETTINGS_Asio))
            {
                Dictionary<string, List<Settings>> splitSettingsIntoSections = new Dictionary<string, List<Settings>>();

                foreach (Settings setting in LoadedSettings) // Force the settings into their respective sections.
                {
                    if (splitSettingsIntoSections.ContainsKey(setting.Section))
                        splitSettingsIntoSections[setting.Section].Add(setting);
                    else
                        splitSettingsIntoSections.Add(setting.Section, new List<Settings> { setting });
                }

                if (changedSettings != null) // If we want to change a setting before we write to the file, this will swap out the old setting for the inputted setting.
                    splitSettingsIntoSections[changedSettings.Section][splitSettingsIntoSections[changedSettings.Section].FindIndex(setting => setting.SettingName == changedSettings.SettingName)] = changedSettings;

                foreach (string section in splitSettingsIntoSections.Keys)
                {
                    sw.WriteLine("[" + section + "]"); // Write section name to Settings file to format as an INI Section.

                    bool pastedAsioComments = false;

                    foreach (Settings setting in splitSettingsIntoSections[section])
                    {
                        if (section == "Asio" && pastedAsioComments == false) // Re-adds the comments regarding buffer size modes. Trying to make the RS_ASIO settings file look as "stock" as possible.
                        {
                            sw.WriteLine("; available buffer size modes:");
                            sw.WriteLine(";    driver - respect buffer size setting set in the driver");
                            sw.WriteLine(";    host   - use a buffer size as close as possible as that requested by the host application");
                            sw.WriteLine(";    custom - use the buffer size specified in CustomBufferSize field");
                            pastedAsioComments = true;
                        }

                        if (setting.Value == null) // If the setting doesn't exist, let's set it to the default mod value.
                            sw.WriteLine(setting.SettingName + "=" + setting.DefaultValue);
                        else
                            sw.WriteLine(setting.SettingName + "=" + setting.Value);
                    }
                }
            }
        }
    }
}
