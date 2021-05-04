using System.Linq;
using System.IO;

namespace RSDiagnostics.Settings.Rocksmith
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
                    if (RSDiagnostics.Settings.Settings.HasValidSettingsFile(RSDiagnostics.Settings.Settings.SETTINGS_Rocksmith))
                    {
                        if (LoadSettings.LoadedSettings.Where(settings => settings.SettingName == SettingName).First().Value != Value)
                            LoadSettings.LoadedSettings.Where(settings => settings.SettingName == SettingName).First().Value = Value;
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
            if (LoadSettings.SettingsFile_Cache.Count == 0)
            {
                bool settingExistsInSettingsFile = false;
                foreach (string line in File.ReadAllLines(RSDiagnostics.Settings.Settings.SETTINGS_Rocksmith))
                {
                    if (line[0] == '[') // Don't cache sections
                        continue;

                    int equals = line.IndexOf(" = ");
                    LoadSettings.SettingsFile_Cache.Add(line.Substring(0, equals), line.Substring(equals + " = ".Length));

                    if (line.Substring(0, equals) == SettingName)
                        settingExistsInSettingsFile = true;
                }

                if (!settingExistsInSettingsFile) // Mod doesn't exist in Settings File.
                    LoadSettings.SettingsFile_Cache.Add(SettingName, @default);

                return ReadPreviousSetting(SettingName, @default);
            }
            else
            {
                if (!LoadSettings.SettingsFile_Cache.ContainsKey(SettingName)) // Mod doesn't exist in Settings File.
                    LoadSettings.SettingsFile_Cache.Add(SettingName, @default);

                object output = LoadSettings.SettingsFile_Cache[SettingName];
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

        public Settings WhereSettingName(string _SettingName) => LoadSettings.LoadedSettings.Where(setting => setting.SettingName == _SettingName).First();
    }
}
