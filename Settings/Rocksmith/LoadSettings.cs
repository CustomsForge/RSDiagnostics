using System.Collections.Generic;
using System.IO;

namespace RSDiagnostics.Settings.Rocksmith
{
    /// <summary>
    /// Load, cache, and write all Rocksmith Settings
    /// </summary>
    public class LoadSettings
    {
        /// <summary>
        /// Load Rocksmith Settings
        /// </summary>
        public LoadSettings()
        {
            LoadedSettings.Clear();

            // Audio
            LoadedSettings.Add(new Settings("Audio", "EnableMicrophone", 1));
            LoadedSettings.Add(new Settings("Audio", "ExclusiveMode", 1));
            LoadedSettings.Add(new Settings("Audio", "LatencyBuffer", 4));
            LoadedSettings.Add(new Settings("Audio", "ForceDefaultPlaybackDevice", ""));
            LoadedSettings.Add(new Settings("Audio", "ForceWDM", 0));
            LoadedSettings.Add(new Settings("Audio", "ForceDirectXSink", 0));
            LoadedSettings.Add(new Settings("Audio", "DumpAudioLog", 0));
            LoadedSettings.Add(new Settings("Audio", "MaxOutputBufferSize", 0));
            LoadedSettings.Add(new Settings("Audio", "RealToneCableOnly", 0));
            LoadedSettings.Add(new Settings("Audio", "Win32UltraLowLatencyMode", 1));

            // Renderer.Win32
            LoadedSettings.Add(new Settings("Renderer.Win32", "ShowGamepadUI", 0));
            LoadedSettings.Add(new Settings("Renderer.Win32", "ScreenWidth", 0));
            LoadedSettings.Add(new Settings("Renderer.Win32", "ScreenHeight", 0));
            LoadedSettings.Add(new Settings("Renderer.Win32", "Fullscreen", 2));
            LoadedSettings.Add(new Settings("Renderer.Win32", "VisualQuality", 1));
            LoadedSettings.Add(new Settings("Renderer.Win32", "RenderingWidth", 0));
            LoadedSettings.Add(new Settings("Renderer.Win32", "RenderingHeight", 0));
            LoadedSettings.Add(new Settings("Renderer.Win32", "EnablePostEffects", 1));
            LoadedSettings.Add(new Settings("Renderer.Win32", "EnableShadows", 1));
            LoadedSettings.Add(new Settings("Renderer.Win32", "EnableHighResScope", 1));
            LoadedSettings.Add(new Settings("Renderer.Win32", "EnableDepthOfField", 1));
            LoadedSettings.Add(new Settings("Renderer.Win32", "EnablePerPixelLighting", 1));
            LoadedSettings.Add(new Settings("Renderer.Win32", "MsaaSamples", 4));
            LoadedSettings.Add(new Settings("Renderer.Win32", "DisableBrowser", 0));

            // Net
            LoadedSettings.Add(new Settings("Net", "UseProxy", 1));

            WriteSettingsFile();
        }

        /// <summary>
        /// List of Settings
        /// </summary>
        public static List<Settings> LoadedSettings = new List<Settings>();

        /// <summary>
        /// Cache of Settings File.
        /// </summary>
        public static Dictionary<string, object> SettingsFile_Cache = new Dictionary<string, object>();

        /// <summary>
        /// Create a new Settings File
        /// </summary>
        /// <param name="changedSettings"> - Setting adjusted, to be replaced in the settings file.</param>
        public static void WriteSettingsFile(Settings changedSettings = null)
        {
            using (StreamWriter sw = File.CreateText(RSDiagnostics.Settings.Settings.SETTINGS_Rocksmith))
            {
                Dictionary<string, List<Settings>> splitSettingsIntoSections = new Dictionary<string, List<Settings>>();

                foreach (Settings setting in LoadedSettings) // Force the settings into their respective sections.
                {
                    if (splitSettingsIntoSections.ContainsKey(setting.Section))
                        splitSettingsIntoSections[setting.Section].Add(setting);
                    else
                        splitSettingsIntoSections.Add(setting.Section, new List<Settings> { setting });
                }

                if (changedSettings != null)  // If we want to change a setting before we write to the file, this will swap out the old setting for the inputted setting.
                    splitSettingsIntoSections[changedSettings.Section][splitSettingsIntoSections[changedSettings.Section].FindIndex(setting => setting.SettingName == changedSettings.SettingName)] = changedSettings;

                foreach (string section in splitSettingsIntoSections.Keys)
                {
                    sw.WriteLine("[" + section + "]"); // Write section name to Settings file to format as an INI Section.

                    foreach (Settings setting in splitSettingsIntoSections[section])
                    {
                        if (setting.Value == null)  // If the setting doesn't exist, let's set it to the default mod value.
                            sw.WriteLine(setting.SettingName + "=" + setting.DefaultValue);
                        else
                            sw.WriteLine(setting.SettingName + "=" + setting.Value);
                    }
                }
            }
        }
    }
}
