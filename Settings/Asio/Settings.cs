using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace RSDiagnostics.Settings.Asio
{
    /// <summary>
    /// RS_ASIO Settings
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// INI Section that the setting is located under.
        /// </summary>
        public string Section { get; }

        /// <summary>
        /// Name of setting in RS_ASIO.ini
        /// </summary>
        public string SettingName { get; }

        /// <summary>
        /// PRIVATE. Used so we can create a save function for "Value".
        /// </summary>
        private object _value;

        /// <summary>
        /// PUBLIC. Public interface for "_value", to create a save function.
        /// </summary>
        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (!alreadyInit) // Don't cause a stack overflow by creating a loop on startup.
                {
                    if (RSDiagnostics.Settings.Settings.HasValidSettingsFile(RSDiagnostics.Settings.Settings.SETTINGS_Asio))
                    {
                        if (LoadSettings.LoadedSettings.Where(settings => settings.SettingName == SettingName && settings.Section == Section).First().Value != Value) // Change cached version of the setting to match the new value.
                            LoadSettings.LoadedSettings.Where(settings => settings.SettingName == SettingName && settings.Section == Section).First().Value = Value;
                    }

                    LoadSettings.WriteSettingsFile(); // Rewrite the Settings File with the new values.
                }
                else
                    alreadyInit = false;

            }
        }

        /// <summary>
        /// Value to use if the user doesn't have the setting in their RS_ASIO.ini
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// Prevents stack overflow loop.
        /// </summary>
        private bool alreadyInit = false;

        /// <summary>
        /// Create A New RS_ASIO Setting
        /// </summary>
        /// <param name="_Section"> - What section of the Settings File is this Setting located in?</param>
        /// <param name="_SettingName"> - What is the name of the Setting in the Settings File</param>
        /// <param name="_DefaultValue"> - If we can't find the setting, what should we default to?</param>
        public Settings(string _Section, string _SettingName, object _DefaultValue)
        {
            alreadyInit = true;
            Section = _Section;
            SettingName = _SettingName;
            DefaultValue = _DefaultValue;

            if (RSDiagnostics.Settings.Settings.HasValidSettingsFile(RSDiagnostics.Settings.Settings.SETTINGS_Asio)) // Make sure the RS_ASIO.ini isn't just a blank file.
            {
                Value = ReadPreviousSetting(_Section, _SettingName, _DefaultValue);

                if (Value == null) // Setting not found in Settings File.
                    Value = DefaultValue;
            }
        }

        /// <summary>
        /// Read A RS_ASIO Setting From The Settings File
        /// </summary>
        /// <param name="SectionName">Name of section where the setting is located inside the Settings File.</param>
        /// <param name="SettingName">Name of setting in the Settings File.</param>
        /// <returns>INT (if int), STRING (if string), NULL (if not found)</returns>
        private static object ReadPreviousSetting(string SectionName, string SettingName, object @default)
        {
            // Not Cached
            if (LoadSettings.SettingsFile_Cache.Count == 0)
            {
                bool settingExistsInSettingsFile = false; // Have we seen the setting in the settings file?
                string currentSection = string.Empty; // Current section being ran through.

                // Read Settings File
                foreach (string line in File.ReadAllLines(RSDiagnostics.Settings.Settings.SETTINGS_Asio))
                {

                    // Don't cache comments and blank lines
                    if (line.Length == 0 || line[0] == ';') 
                        continue;
                    
                    if (line[0] == '[') // Section marker detected
                    {
                        currentSection = line.Remove(0, 1).Remove(line.Length - 2, 1); // Remove the brackets
                        continue; // We know this is a section, so move on to the next line.
                    }
                        
                    int equals = line.IndexOf("=");
                    string currentLine_SettingName = line.Substring(0, equals);
                    string currentLine_SettingValue = line.Substring(equals + "=".Length);

                    if (LoadSettings.SettingsFile_Cache.ContainsKey(currentSection)) // Section already exists
                    {
                        if (LoadSettings.SettingsFile_Cache[currentSection].ContainsKey(currentLine_SettingName))
                            LoadSettings.SettingsFile_Cache[currentSection][currentLine_SettingName] = currentLine_SettingValue; // ... AND the setting already exists. Update value to the new value.
                        else
                            LoadSettings.SettingsFile_Cache[currentSection].Add(currentLine_SettingName, currentLine_SettingValue); // ... BUT the setting doesn't exist.
                    }
                    else
                        LoadSettings.SettingsFile_Cache.Add(currentSection, new Dictionary<string, object>() { { currentLine_SettingName, currentLine_SettingValue } }); // Section doesn't exist, so create the section and add the setting to it.

                    if (currentLine_SettingName == SettingName) // Verify the setting we are looking for is, in fact, in the Settings File.
                        settingExistsInSettingsFile = true;
                }

                if (!settingExistsInSettingsFile) // Mod doesn't exist in Settings File.
                {
                    if (LoadSettings.SettingsFile_Cache.ContainsKey(SectionName))
                        LoadSettings.SettingsFile_Cache[SectionName].Add(SettingName, @default); // Section exists BUT setting does not
                    else
                        LoadSettings.SettingsFile_Cache.Add(SectionName, new Dictionary<string, object>() { { SettingName, @default } }); // Section doesn't exist, so create the section and add the setting to it.
                }

                return ReadPreviousSetting(SectionName, SettingName, @default); // Re-run this function, but read over the cached results.
            }

            // Cached
            else
            {
                // Error checking
                if (!LoadSettings.SettingsFile_Cache.ContainsKey(SectionName)) // Section doesn't exist, so create the section and add the setting to it. (shouldn't ever happen this late, but better safe than sorry)
                    LoadSettings.SettingsFile_Cache.Add(SettingName, new Dictionary<string, object>() { { SettingName, @default } });
                else if (!LoadSettings.SettingsFile_Cache[SectionName].ContainsKey(SettingName)) // Section exists, but setting doesn't exist. Add it's default value.
                    LoadSettings.SettingsFile_Cache[SectionName].Add(SettingName, @default);
                
                object output = LoadSettings.SettingsFile_Cache[SectionName][SettingName];
                try
                {
                    // If the value is a number, output a number, else output what we have.
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

        /// <summary>
        /// Get instance of Setting from the Setting Name and Section.
        /// </summary>
        /// <param name="_Section"> - Section of the Settings File to find this Setting in</param>
        /// <param name="_SettingName"> - Name of the Setting in the settings file.</param>
        /// <returns>Instance of the Setting, if it exist.</returns>
        public static List<Settings> Where(string _Section, string _SettingName) => LoadSettings.LoadedSettings.Where(setting => setting.SettingName == _SettingName && setting.Section == _Section).ToList();
    }
}
