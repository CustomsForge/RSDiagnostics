using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RSDiagnostics.Util
{
    public static class GenUtil
    {

        /// <summary>
        /// Convert a string to an decimal, if possible. If it's not possible, result to @default.
        /// </summary>
        /// <param name="s"> - String to parse to an decimal.</param>
        /// <param name="default"> - Default value to use as a backup.</param>
        /// <returns>"s" as an decimal, if possible, else default value.</returns>
        public static decimal StrToDecDef(string s, decimal @default)
        {
            if (decimal.TryParse(s, out decimal number))
                return number;
            return @default;
        }

        /// <summary>
        /// Convert a string to an int, if possible. If it's not possible, result to @default.
        /// </summary>
        /// <param name="s"> - String to parse to an int.</param>
        /// <param name="default"> - Default value to use as a backup.</param>
        /// <returns>"s" as an int, if possible, else default value.</returns>
        public static int StrToIntDef(string s, int @default)
        {
            if (int.TryParse(s, out int number))
                return number;
            return @default;
        }

        /// <summary>
        /// See if a folder has no files in it.
        /// </summary>
        /// <param name="path"> - Folder to check</param>
        /// <returns>Is this folder empty?</returns>
        public static bool IsDirectoryEmpty(string path) => !Directory.EnumerateFileSystemEntries(path).Any();

        /// <summary>
        /// Extract files embedded in the application. This can help remove the need for an installer.
        /// </summary>
        /// <param name="outputDir"></param>
        /// <param name="resourceAssembly"></param>
        /// <param name="resourceLocation"></param>
        /// <param name="files"></param>
        public static void ExtractEmbeddedResource(string outputDir, Assembly resourceAssembly, string resourceLocation, string[] files)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            foreach (string file in files)
            {
                string resourcePath = Path.Combine(outputDir, file);

                Stream stream = resourceAssembly.GetManifestResourceStream(String.Format("{0}.{1}", resourceLocation, file));
                using (FileStream fileStream = new FileStream(resourcePath, FileMode.Create))
                    stream.CopyTo(fileStream);
            }
        }

        /// <summary>
        /// Gets the default browser for the user.
        /// </summary>
        /// <param name="url"> - URL to pass to edge if needed.</param>
        /// <returns>Name of browser executable</returns>
        public static string GetDefaultBrowser(string url)
        {
            string browserName = "iexplore.exe";
            try
            {
                using (RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice"))
                {
                    if (userChoiceKey != null)
                    {
                        object progIdValue = userChoiceKey.GetValue("Progid");
                        if (progIdValue != null)
                        {
                            if (progIdValue.ToString().ToLower().Contains("chrome"))
                                browserName = "chrome.exe";
                            else if (progIdValue.ToString().ToLower().Contains("firefox"))
                                browserName = "firefox.exe";
                            else if (progIdValue.ToString().ToLower().Contains("safari"))
                                browserName = "safari.exe";
                            else if (progIdValue.ToString().ToLower().Contains("opera"))
                                browserName = "opera.exe";
                            else if (progIdValue.ToString().ToLower().Contains("brave"))
                                browserName = "brave.exe";
                            else if (progIdValue.ToString().ToLower().Contains("edge"))
                                return $"microsoft-edge:{url}";
                        }
                    }
                }
            }
            catch (NullReferenceException) { }

            return browserName;
        }

        /// <summary>
        /// Copy properties of base class to a derived class
        /// </summary>
        /// <typeparam name="T"> - Derived Class</typeparam>
        /// <typeparam name="TU"> - Base Class</typeparam>
        /// <param name="target"> - Target</param>
        /// <param name="source"> - Base / Source</param>
        /// <returns>Target with properties from base class.</returns>
        public static T Map<T, TU>(this T target, TU source)
        {
            var tprops = target.GetType().GetProperties();

            tprops.Where(x => x.CanWrite == true).ToList().ForEach(prop =>
            {
                var sp = source.GetType().GetProperty(prop.Name);
                if (sp != null)
                {
                    var value = sp.GetValue(source, null);
                    target.GetType().GetProperty(prop.Name).SetValue(target, value, null);
                }
            });

            return target;
        }

        /// <summary>
        /// Is this folder actually a Rocksmith folder, or did the user lie to us.
        /// </summary>
        /// <param name="folderPath"> - Path to Rocksmith</param>
        /// <returns>Is this a real Rocksmith folder?</returns>
        private static bool IsRSFolder(this string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return false;

            string cachePsarcPath = Path.Combine(folderPath, "cache.psarc");

            //if (IsDirectoryEmpty(dlcFolderPath))
            //    return false;

            if (!File.Exists(cachePsarcPath))
                return false;

            return true;
        }

        /// <summary>
        /// Return a string from the registry.
        /// </summary>
        /// <param name="keyName"> - Full registry path</param>
        /// <param name="valueName"> - Name of value to look for</param>
        /// <returns>Value as stored in registry, if it exist, and if it doesn't return string.Empty</returns>
        private static string GetStringValueFromRegistry(string keyName, string valueName)
        {
            try
            {
                var retValue = (string)Registry.GetValue(keyName, valueName, "");
                return retValue ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Read Steam's libraryfolders.vdf file to find other "steamapps/" folders
        /// </summary>
        /// <param name="mainSteamPath"> - Steam install location</param>
        /// <returns>List of "steamapps/" locations.</returns>
        private static List<string> GetCustomSteamappsFolders(string mainSteamPath)
        {
            string libRegex = "(^\\t\"[1-9]\").*(\".*\")";
            var libDirs = new List<string>();

            string steamappsFolder = Path.Combine(mainSteamPath, "steamapps");
            string libVdf = Path.Combine(steamappsFolder, "libraryfolders.vdf");

            if (!File.Exists(libVdf))
                return new List<string>();

            var content = File.ReadAllLines(libVdf);
            foreach (string l in content)
            {
                var reg = Regex.Match(l, libRegex);
                string dir = reg.Groups[2].Value;

                if (dir != string.Empty)
                {
                    string ndir = dir.Trim('\"');
                    libDirs.Add(ndir);
                }
            }

            if (libDirs.Count == 0)
                return new List<string>();

            return libDirs;
        }

        /// <summary>
        /// Try to find Rocksmith in a custom "steamapps/" folder.
        /// </summary>
        /// <param name="mainSteamPath"> - Steam install location</param>
        /// <returns>Rocksmith folder, if found, and string.Empty if not.</returns>
        private static string GetCustomRSFolder(string mainSteamPath)
        {
            var customSteamppsFolders = GetCustomSteamappsFolders(mainSteamPath);
            string rsFolderPath = string.Empty;
            bool found = false;

            foreach (var customSteamappsFolder in customSteamppsFolders)
            {
                string dirPath = Path.Combine(customSteamappsFolder, "steamapps", "appmanifest_221680.acf");

                if (File.Exists(dirPath))
                {
                    string finalPath = Path.GetDirectoryName(dirPath);

                    rsFolderPath = Path.Combine(finalPath, "common", "Rocksmith2014");

                    if (rsFolderPath.IsRSFolder())
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (found)
            {
                //  MessageBox.Show("Found Custom RS2014 Installation Directory ...");
                return rsFolderPath;
            }

            // MessageBox.Show("<WARNING> Custom RS2014 Installation Directory not found ...");
            return string.Empty;
        }

        /// <summary>
        /// Scan the registry for Steam's install location.
        /// </summary>
        /// <returns>Steam's install location</returns>
        public static string GetSteamDirectory()
        {
            const string steamRegPath = @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam"; //IIRC it isn't the same on X86 machines, but do we really need to support those?

            return GetStringValueFromRegistry(steamRegPath, "SteamPath").Replace('/', '\\');
        }

        /// <summary>
        /// List of keys that can be used to look for Rocksmith's install location.
        /// </summary>
        public static List<Tuple<string, string>> InstallRegKeys = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Ubisoft\Rocksmith2014", "installdir"),
            new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 221680", "InstallLocation"),
            new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Ubisoft\Rocksmith2014", "InstallLocation"),
            new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 221680", "InstallLocation")
        };

        /// <summary>
        /// Split Setting=Value into a Dictionary {Setting, Value}
        /// </summary>
        /// <param name="settingsLines"> - List of "Setting=Value"</param>
        /// <returns>Dictionary of {Setting, Value}</returns>
        public static Dictionary<string, string> GetSettingsPairs(List<string> settingsLines)
        {
            var dictRet = new Dictionary<string, string>();

            settingsLines.ForEach(line => 
                {
                    string entry = line.Split('=')[0].Trim();
                    string value = line.Split('=')[1].Trim();

                    dictRet.Add(entry, value);
                }
            );

            return dictRet;
        }

        /// <summary>
        /// Try to find Rocksmith's install location
        /// </summary>
        /// <returns>Rocksmith's install location, if found, else string.Empty</returns>
        public static string GetRSDirectory()
        {
            try
            {
                var rs2RootDir = string.Empty;
                var steamRootPath = GetSteamDirectory();

                if (!string.IsNullOrEmpty(steamRootPath))
                {
                    rs2RootDir = Path.Combine(steamRootPath, "SteamApps\\common\\Rocksmith2014");

                    if (!Directory.Exists(rs2RootDir)) // RS-Folder doesn't exist
                    {
                        // Go through each possible registry location
                        foreach (var installRegKey in InstallRegKeys)
                        {
                            var path = GetStringValueFromRegistry(installRegKey.Item1, installRegKey.Item2);

                            if (!string.IsNullOrEmpty(path))
                            {
                                rs2RootDir = path;
                                break;
                            }
                        }

                        rs2RootDir = GetCustomRSFolder(steamRootPath); // Grab custom Steam library paths from .vdf file

                        if (string.IsNullOrEmpty(rs2RootDir) || !rs2RootDir.IsRSFolder()) // If neither that's OK, ask the user to point the GUI to the correct location
                        {
                            MessageBox.Show("We were unable to detect your Rocksmith 2014 folder, please select it manually!", "Your help is required!");
                            return AskUserForRSFolder();
                        }
                    }
                    else // RS-Folder does exist
                    {
                        if (!File.Exists(Path.Combine(rs2RootDir, "cache.psarc")))
                        { // If cache.psarc doesn't exist (old install / steam left-overs)
                            if (AskUserForRSFolder() == string.Empty)
                            {
                                MessageBox.Show("We were unable to detect your Rocksmith 2014 folder, and you didn't give us a valid RS Folder.", "Closing Application");
                                Application.Exit();
                            }

                        }
                    }
                }

                return rs2RootDir;
            }
            catch (Exception ex)
            {
                MessageBox.Show("<Warning> GetRSDirectory, " + ex.Message);
            }

            return string.Empty;
        }

        /// <summary>
        /// Prompt user for their Rocksmith install location
        /// </summary>
        /// <returns>Rocksmith install location, if the folder is a Rocksmith folder, else string.Empty</returns>
        public static string AskUserForRSFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string rsFolder = dialog.SelectedPath;

                    if (rsFolder.IsRSFolder())
                        return rsFolder;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Try to find the user's Rocksmith profile folder.
        /// </summary>
        /// <returns>Rocksmith profile folder, if in it's default location, else string.Empty</returns>
        public static string GetSteamProfilesFolderManual()
        {
            string steamUserdataPath = Path.Combine(GetSteamDirectory(), "userdata");
            try
            {
                var subdirs = new DirectoryInfo(steamUserdataPath).GetDirectories(@"221680", SearchOption.AllDirectories).ToArray();
                var userprofileFolder = subdirs.FirstOrDefault(dir => !dir.FullName.Contains("760")); //760 is the ID for Steam's screenshot thingy

                if (Directory.Exists(userprofileFolder.FullName))
                    return userprofileFolder.FullName;
                else
                    MessageBox.Show("Could not find profile folder!", "Error");
            }
            catch (IOException ioex)
            {
                MessageBox.Show($"Could not find Steam profiles folder: {ioex.Message}", "Error");
            }

            return string.Empty;
        }
    }
}
