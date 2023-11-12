using CsvHelper;
using CsvHelper.Configuration;
using osu1progressbar.Game.Database;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace osu_progressCLI
{
    internal class ScoreImporter
    {

        private static ScoreImporter instance;
        List<ImportScore> scores;
        List<ImportScore> alreadyimportedscores;
        private string DEFAULTFILEPATH = "importcache/Alreadyimportedscores.csv";
        private readonly object objectlock = new object();
        private dumbobject status = new();
        private static List<seconddumbobject> otherstatus = new();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        OsuParsers.Database.OsuDatabase osudb = null;

        private ScoreImporter()
        {
            scores = new List<ImportScore>();
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

        public async Task<bool> TrackImportedScore(ImportScore score)
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
                    csv.WriteRecordsAsync(new List<ImportScore> { score });
                }
            }

            return true;
        }

        public List<ImportScore> FetchAlreadyImported()
        {

            if (!File.Exists(DEFAULTFILEPATH))
            {
                return new List<ImportScore>();
            }

            using (var reader = new StreamReader(DEFAULTFILEPATH))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<ImportScore>();
                alreadyimportedscores = records.ToList();
                status.Finishedimports = alreadyimportedscores.Count;
            }

            return alreadyimportedscores;
        }

        public bool WriteScore(string filePath, List<ImportScore> score)
        {
            try
            {

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

        private void removedoubleentrys(string filepath)
        {

            List<ImportScore> filteredscores = new List<ImportScore>();
            try
            {
                using (var reader = new StreamReader(filepath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {

                    var records = csv.GetRecords<ImportScore>();
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
                status.ToImportScores = filteredscores.Count;
                WriteScore(filepath, filteredscores);
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Scoreimporter, ex.Message);
            }
        }

        public async Task<bool> ImportScores(string filepath)
        {
            string[] files = Directory.GetFiles("imports");
            foreach (string file in files)
            {
                if (Path.GetExtension(file) == "csv")
                {
                    filepath = Path.GetFileName(file);
                }
            }


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
                    scores = csv.GetRecords<ImportScore>().ToList();

                    List<ImportScore> Filterdscores = new List<ImportScore>();
                    ScoreComparer comparer = new ScoreComparer();
                    List<ImportScore> filteredScores = scores.Where(score =>
                    {
                        bool matchFound = alreadyimportedscores.Any(existingscore => comparer.Compare(score, existingscore) == 0);
                        return !matchFound;
                    }).ToList();

                    Console.WriteLine("Potential Beatmaps to Download: " + filteredScores.Count + "/" + status.ToImportScores);
                    status.running = true;
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
                                status.Finishedimports = alreadyimportedscores.Count;
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
                    status.running = false;
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

        public async void ImportScores()
        {

            startup();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Logger.Log(Logger.Severity.Error, Logger.Framework.Scoreimporter, "Starting Score beatmap pair finding" + DateTime.Now);
            if (File.Exists("importcache/osu!.db"))
            {
                if(osudb == null)
                {
                    osudb = OsuParsers.Decoders.DatabaseDecoder.DecodeOsu("importcache/osu!.db");
                }


                string[] scores = Directory.GetFiles("imports");

                List<Task> tasks = new List<Task>();

                List<OsuParsers.Database.ScoresDatabase> scoresDatabases = new();

                string[] names = { };

                foreach (var item in scores)
                {
                    if (Path.GetFileName(item).StartsWith("scores.db"))
                    {
                        scoresDatabases.Add(OsuParsers.Decoders.DatabaseDecoder.DecodeScores(item));
                        names = names.Append(Path.GetFileName(item)).ToArray();
                    }
                }

                if (otherstatus.Count == 0 || scores.Length != otherstatus.Count)
                {
                    otherstatus.Clear();
                    for (int k = 0; k < scoresDatabases.Count; k++)
                    {
                        otherstatus.Add(new seconddumbobject()
                        {
                            index = 0,
                            currentscoredb = k,
                            scorecount = scoresDatabases.ElementAt(k).Scores.Count,
                            running = false,
                            name = names[k]

                        });
                    }
                }

                for (int i = 0; i < scoresDatabases.Count; i++)
                {
                    var scoreDatabase = scoresDatabases[i];

                    int currentIndex = i; // Capture the current value of i

                    tasks.Add(Task.Run(async () =>
                    {
                        otherstatus.ElementAt(currentIndex).running = true;
                        for (int j = otherstatus.ElementAt(currentIndex).index; j < scoreDatabase.Scores.Count; j++)
                        {
                            int currentScoreIndex = j; // Capture the current value of j

                            otherstatus.ElementAt(currentIndex).index = currentScoreIndex;

                            var beatmapMD5Hash = scoreDatabase.Scores[currentScoreIndex].Item2.FirstOrDefault()?.BeatmapMD5Hash;

                            if (beatmapMD5Hash != null)
                            {
                                var matchedBeatmap = osudb.Beatmaps.FirstOrDefault(beatmap => beatmap.MD5Hash == beatmapMD5Hash);

                                if (matchedBeatmap != null)
                                {
                                    var score = scoreDatabase.Scores[currentScoreIndex].Item2.FirstOrDefault();
                                    if (score != null)
                                    {
                                        await DatabaseController.ImportScore(beatmap: matchedBeatmap, score: score);
                                        await save();
                                    }
                                    else
                                    {
                                        // Handle the case when score is null
                                    }
                                }
                            }
                        }
                        otherstatus.ElementAt(currentIndex).running = false;
                        File.Move("imports/" + otherstatus.ElementAt(currentIndex).name, "doneimports/" + otherstatus.ElementAt(currentIndex).name,true);
                    }));
                }

                await Task.WhenAll(tasks);
            }
            else {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Scoreimporter, $"osu!.db was not found in the Import folder please make sure its there");
                return;
            }
        }

            public dumbobject GetStatus()
        {
            return status;
        }

        public List<seconddumbobject> GetotherStatus()
        {
            return otherstatus;
        }

        public static void progress(object sender, EventArgs e)
        {
            File.WriteAllText("progress.json", System.Text.Json.JsonSerializer.Serialize(otherstatus));
        }

        public static async Task save()
        {
            await semaphore.WaitAsync();

            try
            {
                string jsonText = JsonSerializer.Serialize(otherstatus, new JsonSerializerOptions { WriteIndented = true });

                await File.WriteAllTextAsync("progress.json", jsonText, Encoding.UTF8);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static void startup() {
            if(File.Exists("progress.json") && File.ReadAllText("progress.json").Length > 0)
                otherstatus = System.Text.Json.JsonSerializer.Deserialize<List<seconddumbobject>>(File.ReadAllText("progress.json"));
        }
    }
  
    public class seconddumbobject
    {
        public bool running { get; set; } 
        public int currentscoredb { get; set; } = 1;
        public int index { get; set; } = 1;
        public int scorecount { get; set; } = 0;
        public string name { get; set; } = "";
    }

   

    public class dumbobject {
        public bool running { get; set; }
        public int Finishedimports { get; set; }
        public int ToImportScores { get; set; }
    }

    public class ImportScore
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

    public class ScoreComparer : IComparer<ImportScore>
    {
        public int Compare(ImportScore x, ImportScore y)
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
