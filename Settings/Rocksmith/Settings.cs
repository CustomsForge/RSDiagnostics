using System.Linq;
using System.IO;

namespace RSDiagnostics.Settings.Rocksmith
{
    /// <summary>
    /// Setting for Rocksmith
    /// </summary>
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
                if (!alreadyInit) // Don't cause a stack overflow by creating a loop on startup.
                {
                    if (RSDiagnostics.Settings.Settings.HasValidSettingsFile(RSDiagnostics.Settings.Settings.SETTINGS_Rocksmith))
                    {
                        if (LoadSettings.LoadedSettings.Where(settings => settings.SettingName == SettingName).First().Value != Value) // Change Setting if the value is different.
                            LoadSettings.LoadedSettings.Where(settings => settings.SettingName == SettingName).First().Value = Value;
                    }

                    LoadSettings.WriteSettingsFile(); // Rewrite the Settings File with the new values.
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

        /// <summary>
        /// Create New Setting
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

            if (RSDiagnostics.Settings.Settings.HasValidSettingsFile(RSDiagnostics.Settings.Settings.SETTINGS_Rocksmith))
            {
                if (ReadPreviousSetting(_SettingName, _DefaultValue) == null)
                    Value = DefaultValue; // Value not found
                else
                    Value = ReadPreviousSetting(_SettingName, _DefaultValue); // Value found
            }
        }

        /// <summary>
        /// Read Setting From Settings File
        /// </summary>
        /// <param name="SettingName"> - Name of setting in the Settings File.</param>
        /// <returns>INT (if int), STRING (if string), NULL (if not found)</returns>
        private static object ReadPreviousSetting(string SettingName, object @default)
        {
            // No Cache Exists
            if (LoadSettings.SettingsFile_Cache.Count == 0)
            {
                bool settingExistsInSettingsFile = false;

                // Read Settings File
                foreach (string line in File.ReadAllLines(RSDiagnostics.Settings.Settings.SETTINGS_Rocksmith))
                {
                    if (line.Length == 0 || line[0] == '[') // Don't cache sections, or blank lines
                        continue;

                    int equals = line.IndexOf("=");
                    LoadSettings.SettingsFile_Cache.Add(line.Substring(0, equals), line.Substring(equals + "=".Length));

                    if (line.Substring(0, equals) == SettingName) // Verify the setting we are looking for is, in fact, in the Settings File.
                        settingExistsInSettingsFile = true;
                }

                if (!settingExistsInSettingsFile) // Mod doesn't exist in Settings File.
                    LoadSettings.SettingsFile_Cache.Add(SettingName, @default);

                return ReadPreviousSetting(SettingName, @default); // Re-run this function, but read over the cached results.
            }

            // Cache Exists
            else
            {
                if (!LoadSettings.SettingsFile_Cache.ContainsKey(SettingName)) // Mod doesn't exist in Settings File.
                    LoadSettings.SettingsFile_Cache.Add(SettingName, @default);

                object output = LoadSettings.SettingsFile_Cache[SettingName];
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
        /// Get instance of Setting from the Setting Name.
        /// </summary>
        /// <param name="_SettingName"> - Name of the Setting in the settings file.</param>
        /// <returns>Instance of the Setting, if it exist.</returns>
        public static Settings Where(string _SettingName) => LoadSettings.LoadedSettings.Where(setting => setting.SettingName == _SettingName).First();
    }
}
