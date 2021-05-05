using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rocksmith2014PsarcLib.Psarc;
using Rocksmith2014PsarcLib.Psarc.Models.Json;
using System.Windows.Forms;

namespace RSDiagnostics
{
    public class SongManager
    {
        public static Dictionary<string, SongData> Songs = new Dictionary<string, SongData>();
        public static Dictionary<string, SongData> ExtractSongData(ProgressBar progressBar = null)
        {
            Songs.Clear();
            bool progressBarAvailable = progressBar != null;
            List<string> allFiles = Directory.GetFiles(Path.Combine(Settings.Settings.RocksmithLocation, "dlc"), "*_p.psarc", SearchOption.AllDirectories).ToList();

            if (progressBarAvailable)
            {
                progressBar.Visible = true;
                progressBar.Minimum = 1;
                progressBar.Maximum = allFiles.Count;
                progressBar.Value = 1;
                progressBar.Step = 1;
            }

            ParallelLoopResult loopResult = Parallel.ForEach(allFiles, (file) =>
            {
                try
                {
                    using (PsarcFile psarc = new PsarcFile(file))
                    {

                        List<SongArrangement> ExtractedArrangementManifests = psarc.ExtractArrangementManifests();

                        foreach (SongArrangement arrangement in ExtractedArrangementManifests)
                        {
                            SongData song = new SongData()
                            {
                                DLCKey = arrangement.Attributes.SongKey,
                                Artist = arrangement.Attributes.ArtistName,
                                AppID = psarc.ExtractAppID(),
                                Title = arrangement.Attributes.SongName,
                                Shipping = arrangement.Attributes.Shipping,
                                SKU = arrangement.Attributes.SKU,
                                CommonName = $"{arrangement.Attributes.ArtistName} - {arrangement.Attributes.SongName}"
                            };

                            if (song.DLCKey == null || song.CommonName == string.Empty || song.CommonName == " - " || song.Artist == string.Empty || song.Title == string.Empty || !song.Shipping) // Some songs have a glitched arrangment, so we skip it.
                                continue;

                            // Load all RS1 DLC and their ID so we can determine if the user owns it, and if we should display it accordingly.
                            if (psarc.ExtractToolkitInfo().PackageAuthor == "Ubisoft")
                            {
                                song.ODLC = true;
                                if (arrangement.Attributes.SKU == "RS1" && arrangement.Attributes.DLCRS1Key != null)
                                    song.RS1AppID = arrangement.Attributes.DLCRS1Key[0].WIN32;
                            }

                            if (Songs.ContainsKey(song.DLCKey))
                                continue;
                            else if (song != null)
                                Songs.Add(song.DLCKey, song);
                        }
                    }
                }
                catch {}

            });

            if (progressBarAvailable)
            {
                progressBar.Visible = false;
                progressBar.Value = progressBar.Minimum;
            }

            Songs = Songs.Where(song => song.Key != null).Distinct().ToDictionary(song => song.Key, song => song.Value);

            return Songs;
        }

        /// <summary>
        /// Find how many official songs are non-authentic.
        /// </summary>
        /// <param name="officialSongs"> - List of all official songs.</param>
        /// <returns> - Number of non-authentic official songs.</returns>
        public static int Validate(List<SongData> officialSongs)
        {
            int nonAuthenticCount = 0;

            List<int> UniqueAppIds = new List<int>();
            Dictionary<int, List<string>> DLCPacks = new Dictionary<int, List<string>>();
            List<string> nonAuthenticSongs = new List<string>();

            foreach (SongData song in officialSongs)
            {
                int appID = song.AppID;

                if (appID == 258341 && song.RS1AppID == 0 && song.SKU == "RS1") // RS1 Compat Disc
                    continue;

                else if (appID == 899900 || appID == 1089222 || appID == 1089172 || appID == 1122551 || appID == 1089199 || appID == 1122574) // Exercise Packs
                {
                    if(!song.CommonName.ToLower().Contains("notetrackers"))
                    {
                        nonAuthenticCount++;
                        nonAuthenticSongs.Add(song.CommonName);
                    }
                }

                else if (appID == 436572 || appID == 294990 || appID == 390389 || appID == 753836) // Extra Pack-Only Packs
                {
                    if (DLCPacks.ContainsKey(appID))
                        DLCPacks[appID].Add(song.CommonName);
                    else
                        DLCPacks.Add(appID, new List<string>() { song.CommonName });
                }

                else
                {
                    if (UniqueAppIds.Contains(appID))
                    {
                        nonAuthenticCount++;
                        nonAuthenticSongs.Add(song.CommonName);
                    }
                    else
                        UniqueAppIds.Add(appID);
                }
            }

            foreach (KeyValuePair<int, List<string>> pack in DLCPacks)
            {
                if (pack.Value.Count > 5)
                {
                    foreach(string song in pack.Value)
                    {
                        nonAuthenticCount++;
                        nonAuthenticSongs.Add(song);
                    }
                }
            }
           

            return nonAuthenticCount;
        }


    }

    public class SongData
    {
        public string DLCKey { get; set; }
        public int AppID { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string CommonName { get; set; } // Artist - Title
        public bool Shipping { get; set; } // Should the song show up in game
        public bool ODLC { get; set; } // Is the file made by Ubisoft
        public string SKU { get; set; } // What game was this initially made for ("RS1", "RS2")
        public int RS1AppID { get; set; } // AppID from RS1CompatDLC
    }
}
