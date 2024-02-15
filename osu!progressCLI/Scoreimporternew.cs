using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
        private static readonly string IMPORT_LOCATION = "Importer/imports/";
        private static readonly string FINISHED_LOCATION = "Importer/exports/";
        private static readonly string CACHE_LOCATION = "Importer/cache/";
        public List<ScoreFileTracker> tracker = new();
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

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

            //await startup();

            if (tracker == null)
            {
                tracker = new List<ScoreFileTracker>();
            }

            if (files.Length > 0)
            {
                if (files[0].EndsWith(".csv"))
                {

                    List<ImportScore> scores = new();
                    List<ImportScore> filterd = new();
                    using (var reader = new StreamReader(files[0]))
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

                    foreach (var score in filterd)
                    {
                        await DatabaseController.ImportScore(score);
                    }
                }
            }
        }
          
        
                
            

        public List<ScoreFileTracker> getScoreFileTracker()
        {
            return tracker;
        }

        public async Task save()
        {
            await semaphore.WaitAsync();

            try
            {
                string jsonText = JsonSerializer.Serialize(
                    tracker,
                    new JsonSerializerOptions { WriteIndented = true }
                );

                await File.WriteAllTextAsync(
                    $"{CACHE_LOCATION}progress.json",
                    jsonText,
                    Encoding.UTF8
                );
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task startup()
        {
            if (
                File.Exists($"{CACHE_LOCATION}progress.json")
                && File.ReadAllText($"{CACHE_LOCATION}progress.json").Length > 0
            )
                tracker = System.Text.Json.JsonSerializer.Deserialize<List<ScoreFileTracker>>(
                    await File.ReadAllTextAsync($"{CACHE_LOCATION}progress.json")
                );
        }
    }

    public class ScoreFileTracker
    {
        public bool running { get; set; } = false;
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
