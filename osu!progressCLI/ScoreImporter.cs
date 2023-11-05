using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Newtonsoft.Json.Linq;
using osu_progressCLI.server;
using osu1progressbar.Game.Database;
using OsuParsers.Database.Objects;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;

namespace osu_progressCLI
{
    internal class ScoreImporter
    {

        private static ScoreImporter instance;
        List<Score> scores;
        List<Score> alreadyimportedscores;
        private string DEFAULTFILEPATH = "imports/Alreadyimportedscores.csv";
        private readonly object objectlock = new object();

        private ScoreImporter()
        {
            scores = new List<Score>();
            alreadyimportedscores = FetchAlreadyImported();
        }

        public static ScoreImporter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ScoreImporter();
                }
                return instance;
            }
        }

        public async Task<bool> TrackImportedScore(Score score)
        {
            lock (objectlock) // Lock the critical section
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                };

                if (!File.Exists(DEFAULTFILEPATH) || new FileInfo(DEFAULTFILEPATH).Length == 0)
                {
                    config = new CsvConfiguration(CultureInfo.InvariantCulture);
                }

                using (var stream = new FileStream(DEFAULTFILEPATH, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var writer = new StreamWriter(stream)) // Append to the file
                using (var csv = new CsvWriter(writer, config))
                {
                    csv.WriteRecordsAsync(new List<Score> { score });
                }
            }

            return true;
        }

        public List<Score> FetchAlreadyImported() {

            if (!File.Exists(DEFAULTFILEPATH))
            {
                return new List<Score>();
            }

            using (var reader = new StreamReader(DEFAULTFILEPATH))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Score>();
                alreadyimportedscores = records.ToList();
                Console.WriteLine(alreadyimportedscores.Count);
            }

            return alreadyimportedscores;
        }

        public bool WriteScore(string filePath, List<Score> score)
        {
            try {

                using (var writer = new StreamWriter(filePath, false)) 
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    csv.WriteRecords(score);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Scoreimporter, ex.Message);
            }

            return true;
        }

        private void removedoubleentrys(string filepath) {

            List<Score> filteredscores = new List<Score>();
            try
            {
                using (var reader = new StreamReader(filepath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
               
                    var records = csv.GetRecords<Score>();
                    scores = records.ToList();

                    string prev = "random string";
                    foreach (var score in scores)
                    {
                        string check = $"{score.beatmap_id}{score.rank}{score.pp}";

                        if (prev != check)
                        {
                            prev = check;
                            filteredscores.Add(score);
                        }
                    }
                }
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Scoreimporter, $"Scores to Import {filteredscores.Count()}");

                WriteScore(filepath, filteredscores);
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Scoreimporter, ex.Message);
            }
        }

        public async Task<bool> ImportScores(string filepath)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Scoreimporter, "Removing Doubles");
            removedoubleentrys(filepath);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Logger.Log(Logger.Severity.Info, Logger.Framework.Scoreimporter, $"Trying to Parse {filepath}");

            using (var reader = new StreamReader(filepath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                try
                {
                    scores = csv.GetRecords<Score>().ToList();

                    List<Score> Filterdscores = new List<Score>();

                    ScoreComparer comparer = new ScoreComparer();
                    List<Score> filteredScores = scores.Where(score =>
                    {
                        bool matchFound = alreadyimportedscores.Any(existingscore => comparer.Compare(score, existingscore) == 0);
                        return !matchFound;
                    }).ToList();

                    Console.WriteLine("Potential Beatmaps to Download: " + filteredScores.Count + "/" + scores.Count);

                    int count = 0;

                    var tasks = new List<Task>();

                    foreach (var item in filteredScores)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await DatabaseController.ImportScore(item);
                                await TrackImportedScore(item);
                                alreadyimportedscores.Add(item);
                                Task.Run(async () => await Webserver.Instance().SendData("ScoreCount", alreadyimportedscores.Count));
                                Interlocked.Increment(ref count);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error importing score: {ex.Message}");
                            }
                        }));
                        if (tasks.Count >= 20)
                        {
                            await Task.WhenAll(tasks);
                            tasks.Clear();
                        }
                    }

                    await Task.WhenAll(tasks);

                    Console.WriteLine($"Scores Successfully imported! ({count} Skipped:{alreadyimportedscores.Count()}) in {stopwatch.Elapsed.Hours}:{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds}");

                    return true;

                }
                catch (Exception ex)
                {
                    Logger.Log(Logger.Severity.Error, Logger.Framework.Scoreimporter, $"{ex.Message}");
                    return false;
                }
            }
            return true;
        }
    }

    public class Score
    {
        public int user_id { get; set; }
        public int beatmap_id { get; set; }
        public int score { get; set; }
        public int count300 { get; set; }
        public int count100 { get; set; }
        public int count50 { get; set; }
        public int countmiss { get; set; }
        public int combo { get; set; }
        public bool perfect { get; set; }
        public int enabled_mods { get; set; }
        public DateTime date_played { get; set; }
        public string rank { get; set; }
        public double pp { get; set; }
        public bool replay_available { get; set; }
        public double accuracy { get; set; }
        public int approved { get; set; }
        public DateTime submit_date { get; set; }
        public DateTime approved_date { get; set; }
        public DateTime last_update { get; set; }
        public string artist { get; set; }
        public int set_id { get; set; }
        public double bpm { get; set; }
        public string creator { get; set; }
        public int creator_id { get; set; }
        public double stars { get; set; }
        public double diff_aim { get; set; }
        public double diff_speed { get; set; }
        public double cs { get; set; }
        public double od { get; set; }
        public double ar { get; set; }
        public double hp { get; set; }
        public int drain { get; set; }
        public string source { get; set; }
        public string genre { get; set; }
        public string language { get; set; }
        public string title { get; set; }
        public int length { get; set; }
        public string diffname { get; set; }
        public string file_md5 { get; set; }
        public int mode { get; set; }
        public string tags { get; set; }
        public int favorites { get; set; }
        public double rating { get; set; }
        public int playcount { get; set; }
        public int passcount { get; set; }
        public int circles { get; set; }
        public int sliders { get; set; }
        public int spinners { get; set; }
        public int maxcombo { get; set; }
        public int storyboard { get; set; }
        public int video { get; set; }
        public int download_unavailable { get; set; }
        public int audio_unavailable { get; set; }
        public double star_rating { get; set; }
        public double aim_diff { get; set; }
        public double speed_diff { get; set; }
        public double fl_diff { get; set; }
        public double slider_factor { get; set; }
        public double speed_note_count { get; set; }
        public double modded_od { get; set; }
        public double modded_ar { get; set; }
        public double modded_cs { get; set; }
        public double modded_hp { get; set; }
        public string packs { get; set; }

       
    }

    public class ScoreComparer : IComparer<Score>
    {
        public int Compare(Score x, Score y)
        {
            // Define your custom comparison logic here
            // For example, compare by Property1 first, and then by Property2
            int result = x.pp.CompareTo(y.pp);
            if (result == 0)
            {
                result = String.Compare(x.file_md5, y.file_md5, StringComparison.Ordinal);
            }
            return result;
        }
    }

}
