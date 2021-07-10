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

        /// <summary>
        /// A list of every song in the user's game.
        /// </summary>
        public static Dictionary<string, SongData> Songs = new Dictionary<string, SongData>();

        /// <summary>
        /// Fills Songs with SongData.
        /// </summary>
        /// <param name="progressBar"> - Attach a progress bar to the loading process.</param>
        /// <returns> - Value of Songs</returns>
        public static Dictionary<string, SongData> ExtractSongData(ProgressBar progressBar = null)
        {
            // Init
            Songs.Clear();
            bool progressBarAvailable = progressBar != null;
            List<string> allFiles = Directory.GetFiles(Path.Combine(Settings.Settings.RocksmithLocation, "dlc"), "*_p.psarc", SearchOption.AllDirectories).ToList();


            // Setup Progressbar if passed in as a parameter.
            if (progressBarAvailable)
            {
                progressBar.Visible = true;
                progressBar.Minimum = 1;
                progressBar.Maximum = allFiles.Count;
                progressBar.Value = 1;
                progressBar.Step = 1;
            }


            // Look through every file in allFiles.
            ParallelLoopResult loopResult = Parallel.ForEach(allFiles, (file) =>
            {
                try
                {
                    using (PsarcFile psarc = new PsarcFile(file))
                    {

                        List<SongArrangement> ExtractedArrangementManifests = psarc.ExtractArrangementManifests(); // List every arrangement in the psarc. This is crucial for looking at song packs!

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

                            // Add Song to Songs if it isn't already in it.
                            if (Songs.ContainsKey(song.DLCKey))
                                continue;
                            else if (song != null)
                                Songs.Add(song.DLCKey, song);
                        }
                    }
                }
                catch {} // We are reading the files too quick. Let's forget about it for now as we are just trying to get a rough count.

            });

            // Shutdown Progressbar
            if (progressBarAvailable)
            {
                progressBar.Visible = false;
                progressBar.Value = progressBar.Minimum;
            }

            Songs = Songs.Where(song => song.Key != null).Distinct().ToDictionary(song => song.Key, song => song.Value); // Clear out duplicates, and any null values that got accidently set.

            return Songs;
        }

        /// <summary>
        /// Find how many official songs are non-authentic.
        /// </summary>
        /// <param name="officialSongs"> - List of all official songs.</param>
        /// <returns> - Number of non-authentic official songs.</returns>
        public static int Validate(List<SongData> officialSongs)
        {

            List<int> UniqueAppIds = new List<int>();
            Dictionary<int, List<string>> DLCPacks = new Dictionary<int, List<string>>();
            List<string> nonAuthenticSongs = new List<string>();

            foreach (SongData song in officialSongs)
            {
                int appID = song.AppID;

                // RS1 Compat Disc
                if (appID == 258341 && song.RS1AppID == 0 && song.SKU == "RS1")
                    continue;

                // Exercise Packs
                else if (appID == 899900 || appID == 1089222 || appID == 1089172 || appID == 1122551 || appID == 1089199 || appID == 1122574)
                {
                    if(!song.CommonName.ToLower().Contains("notetrackers")) // Every song with these IDs contains "notetrackers" in it's artist / title. If it doesn't, then it's pirated.
                    {
                        nonAuthenticSongs.Add(song.CommonName);
                    }
                }

                // Extra Pack-Only Packs. There should ONLY be 5 per pack. We check to make sure that only 5 are in each pack further down.
                else if (appID == 436572 || appID == 294990 || appID == 390389 || appID == 753836) 
                {
                    if (DLCPacks.ContainsKey(appID))
                        DLCPacks[appID].Add(song.CommonName);
                    else
                        DLCPacks.Add(appID, new List<string>() { song.CommonName });
                }

                else
                {
                    if (UniqueAppIds.Contains(appID)) // Yeah, this is an ODLC with an AppID we've already scanned. This must be non-Authentic.
                    {
                        nonAuthenticSongs.Add(song.CommonName);
                    }
                    else // We haven't seen this AppID yet.
                        UniqueAppIds.Add(appID);
                }
            }

            // Validate that "Extra Pack-Only Packs" only have 5 songs per AppID, else they have some non-Authentic files hiding in there.
            foreach (KeyValuePair<int, List<string>> pack in DLCPacks) 
            {
                if (pack.Value.Count > 5)
                {
                    foreach(string song in pack.Value)
                    {
                        nonAuthenticSongs.Add(song);
                    }
                }
            }

            return nonAuthenticSongs.Count;
        }
    }
    
    public class SongData
    {
        /// <summary>
        /// <para>Unique string for song.</para>
        /// <para>"DLC Key" in RS Toolkit.</para>
        /// </summary>
        public string DLCKey { get; set; }

        /// <summary>
        /// <para>The AppID of the Steam DLC we should assosciate this CDLC / ODLC with.</para>
        /// <para>"App ID" in RS Toolkit.</para>
        /// </summary>
        public int AppID { get; set; }

        /// <summary>
        /// <para>This one should be pretty self explanatory, what band made this song?</para>
        /// <para>"Artist" in RS Toolkit.</para>
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// <para>This one should be pretty self explanatory, what is the name of this song?</para>
        /// <para>"Song Title" in RS Toolkit.</para>
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// <para>A combined version of Artist & Title, to be used to identify the song.</para>
        /// <para>"Artist - Title" (without quotes).</para>
        /// </summary>
        public string CommonName { get; set; }

        /// <summary>
        /// <para>A true / false value that determines if the song should show up in game.</para>
        /// <para>This should almost ALWAYS be set to true.</para>
        /// </summary>
        public bool Shipping { get; set; }

        /// <summary>
        /// A true / false value that sees if Ubisoft made this chart.
        /// </summary>
        public bool ODLC { get; set; }

        /// <summary>
        /// What game was this initially made for ("RS1", "RS2", etc)
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        /// <para>Rocksmith 1 DLC have a number assosciated with them inside their arrangement manifests.</para>
        /// <para>We grab this value separate from "AppID", as they are not the same value.</para>
        /// </summary>
        public int RS1AppID { get; set; }
    }
}


