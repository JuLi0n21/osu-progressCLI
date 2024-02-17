using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Fluid.Ast;
using osu1progressbar.Game.Database;
using OsuParsers.Beatmaps;
using OsuParsers.Database;
using OsuParsers.Database.Objects;

namespace osu_progressCLI
{
    public class ScoreImporter
    {
        private static ScoreImporter instance;
        public static readonly string IMPORT_LOCATION = "Importer/imports/";
        private static readonly string FINISHED_LOCATION = "Importer/exports/";
        private static readonly string CACHE_LOCATION = "Importer/cache/";
        private static List<ScoreFileTracker> trackerlist = new();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static bool reset = false; // used to reset the score importing incase manualy adding more files

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

        /// <summary>
        /// Starts a new ImportSession
        /// </summary>
        public async Task StartImporting()
        {
            if (!Directory.Exists("Importer"))
            {
                Directory.CreateDirectory("Importer");

                if (!Directory.Exists(IMPORT_LOCATION))
                {
                    Directory.CreateDirectory(IMPORT_LOCATION);
                }

                if (!Directory.Exists(CACHE_LOCATION))
                {
                    Directory.CreateDirectory(CACHE_LOCATION);
                }

                if (!Directory.Exists(FINISHED_LOCATION))
                {
                    Directory.CreateDirectory(FINISHED_LOCATION);
                }
            }

            string[] files = Directory.GetFiles(IMPORT_LOCATION);

            if (files.Length == 0)
            {
                return;
            }

            reset = true;
            trackerlist.Clear();
            Thread.Sleep(2000);
            reset = false;

            if (files.Length > 0)
            {
                Parallel.ForEach(files, file =>
                {
                    try
                    {
                        if (file.EndsWith(".csv"))
                        {
                            Importcsv(file);
                        }

                        if (file.Contains(".db"))
                        {
                            ImportScoreDb(file);
                        }
                    }
                    catch (Exception e){ 
                        Logger.Log(Logger.Severity.Error, Logger.Framework.Scoreimporter, e.Message);
                    }

                });
            }
        }

        public async void Importcsv(string Filename) {

            ScoreFileTracker tracker = await loadprogress(Filename);

            List<ImportScore> scores;
            List<ImportScore> filterd = new();
            using (var reader = new StreamReader(tracker.filename))
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
                        filterd.Add(score);
                    }
                }
            }
            tracker.amountoffscores = filterd.Count;

            for (int i = tracker.index; i <= filterd.Count; i++)
            {
                if (reset)
                    return;

                tracker.index = i;
                await DatabaseController.ImportScore(filterd.ElementAt(i));

                if (i % 10 == 0)
                    await save(tracker);
            }

            cleanup(tracker);
        }

    
        
        public async void ImportScoreDb(string Filename)
        {
            ScoreFileTracker tracker = await loadprogress(Filename);

            ScoresDatabase Scoredb = OsuParsers.Decoders.DatabaseDecoder.DecodeScores($"{tracker.filename}");
            tracker.amountoffscores = Scoredb.Scores.Count;

            for (int i = tracker.index; i <= Scoredb.Scores.Count; i++)
            {
                if (reset)
                    return;

                tracker.index = i;
                await DatabaseController.ImportScore(Scoredb.Scores.ElementAt(i).Item2.FirstOrDefault());

                if (i % 10 == 0)
                    await save(tracker);
            }

            cleanup(tracker);
        }

        private async Task<ScoreFileTracker> loadprogress(string Filename) {

            ScoreFileTracker tracker = null;
            string trackerfilepath = Path.Combine(CACHE_LOCATION, $"{Path.GetFileName(Filename)}.progress.json");

            if (File.Exists(trackerfilepath) && File.ReadAllText(trackerfilepath).Length > 0)
                tracker = JsonSerializer.Deserialize<ScoreFileTracker>(await File.ReadAllTextAsync(trackerfilepath));

            if(tracker == null)
            {
                tracker = new ScoreFileTracker();
                tracker.filename = Filename;
            }

            trackerlist.Add(tracker);

            return tracker;
        }

        private void cleanup(ScoreFileTracker tracker)
        {
            File.Move(tracker.filename, Path.Combine(FINISHED_LOCATION, Path.GetFileName(tracker.filename)));
            File.Delete(Path.Combine(CACHE_LOCATION, $"{Path.GetFileName(tracker.filename)}.progress.json"));
        }

        public List<ScoreFileTracker> getScoreFileTracker()
        {
            return trackerlist;
        }

        public async Task save(ScoreFileTracker tracker)
        {
            await semaphore.WaitAsync();

            try
            {
                string jsonText = JsonSerializer.Serialize(
                    tracker,
                    new JsonSerializerOptions { WriteIndented = true }
                );

                await File.WriteAllTextAsync(
                    Path.Combine(CACHE_LOCATION, $"{Path.GetFileName(tracker.filename)}.progress.json"),
                    jsonText,
                    Encoding.UTF8
                );
            }
            finally
            {
                semaphore.Release();
            }
        }

    }

    public class ScoreFileTracker
    {
        public int amountoffscores { get; set; } = 1;
        public int index { get; set; } = 0;
        public string filename { get; set; } = "";
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
        public string pack_id { get; set; }
    }
}
