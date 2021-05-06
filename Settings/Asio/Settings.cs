using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace RSDiagnostics.Settings.Asio
{
    public class Settings
    {
        /// <summary>
        /// Section that the setting is in.
        /// </summary>
        public string Section { get; }

        /// <summary>
        /// Name of setting in Rocksmith.ini
        /// </summary>
        public string SettingName { get; }

        /// <summary>
        /// PRIAVTE. Used so we can create a save function for "Value".
        /// </summary>
        public object _value;

        /// <summary>
        /// PUBLIC. Public interface for "_value", to create a save function.
        /// </summary>
        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (!alreadyInit)
                {
                    if (RSDiagnostics.Settings.Settings.HasValidSettingsFile(RSDiagnostics.Settings.Settings.SETTINGS_Asio))
                    {
                        if (LoadSettings.LoadedSettings.Where(settings => settings.SettingName == SettingName && settings.Section == Section).First().Value != Value)
                            LoadSettings.LoadedSettings.Where(settings => settings.SettingName == SettingName && settings.Section == Section).First().Value = Value;
                    }

                    LoadSettings.WriteSettingsFile();
                }
                else
                    alreadyInit = false;

            }
        }

        /// <summary>
        /// Value to use if the user doesn't have the setting in their Rocksmith.ini
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// Prevents stack overflow loop.
        /// </summary>
        private bool alreadyInit = false;

        public Settings(string _Section, string _SettingName, object _DefaultValue)
        {
            alreadyInit = true;
            Section = _Section;
            SettingName = _SettingName;
            DefaultValue = _DefaultValue;

            if (RSDiagnostics.Settings.Settings.HasValidSettingsFile(RSDiagnostics.Settings.Settings.SETTINGS_Asio))
            {
                if (ReadPreviousSetting(_Section, _SettingName, _DefaultValue) == null)
                    Value = DefaultValue; // Value not found
                else
                    Value = ReadPreviousSetting(_Section, _SettingName, _DefaultValue); // Value found
            }
        }

        /// <summary>
        /// Read Setting From Settings File
        /// </summary>
        /// <param name="SettingName"> - Name of setting in the Settings File.</param>
        /// <returns>INT (if int), STRING (if string), NULL (if not found)</returns>
        private static object ReadPreviousSetting(string SectionName, string SettingName, object @default)
        {
            if (LoadSettings.SettingsFile_Cache.Count == 0)
            {
                bool settingExistsInSettingsFile = false;
                string currentSection = string.Empty;
                foreach (string line in File.ReadAllLines(RSDiagnostics.Settings.Settings.SETTINGS_Asio))
                {
                    if (line.Length == 0 || line[0] == ';') // Don't cache comments
                        continue;

                    if (line[0] == '[')
                    {
                        currentSection = line.Remove(0, 1).Remove(line.Length - 2, 1); // Remove the brackets
                        continue; // We know this is a section, so move on to the next line.
                    }
                        

                    int equals = line.IndexOf("=");

                    if (LoadSettings.SettingsFile_Cache.ContainsKey(currentSection))
                        LoadSettings.SettingsFile_Cache[currentSection].Add(line.Substring(0, equals), line.Substring(equals + "=".Length));
                    else
                        LoadSettings.SettingsFile_Cache.Add(currentSection, new Dictionary<string, object>() { { line.Substring(0, equals), line.Substring(equals + "=".Length) } });

                    if (line.Substring(0, equals) == SettingName)
                        settingExistsInSettingsFile = true;
                }

                if (!settingExistsInSettingsFile) // Mod doesn't exist in Settings File.
                {
                    if (LoadSettings.SettingsFile_Cache.ContainsKey(currentSection))
                        LoadSettings.SettingsFile_Cache[currentSection].Add(SettingName, @default);
                    else
                        LoadSettings.SettingsFile_Cache.Add(currentSection, new Dictionary<string, object>() { { SettingName, @default } });
                }

                return ReadPreviousSetting(SectionName, SettingName, @default);
            }
            else
            {
                if (!LoadSettings.SettingsFile_Cache.ContainsKey(SectionName)) // Mod doesn't exist in Settings File.
                    LoadSettings.SettingsFile_Cache.Add(SettingName, new Dictionary<string, object>() { { SettingName, @default } });
                else if (!LoadSettings.SettingsFile_Cache[SectionName].ContainsKey(SettingName))
                    LoadSettings.SettingsFile_Cache[SectionName].Add(SettingName, @default);
                

                object output = LoadSettings.SettingsFile_Cache[SectionName][SettingName];
                try
                {
                    if (output != null && int.TryParse(output.ToString(), out int intOutput))
                        return intOutput;
                    else
                        return output;
                }
                catch // INI Error. Best to just return that we don't know what the mod is.
                {
                    return null;
                }
            }
        }

        public static Settings WhereSettingName(string _Section, string _SettingName) => LoadSettings.LoadedSettings.Where(setting => setting.SettingName == _SettingName && setting.Section == _Section).First();
    }
}
