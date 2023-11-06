using CsvHelper;
using CsvHelper.Configuration;
using osu1progressbar.Game.Database;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace osu_progressCLI
{
    internal class ScoreImporter
    {
        public async static Task<bool> ImportScores(string filepath)
        {

            //filepath = @"upload/score.csv";
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Logger.Log(Logger.Severity.Info, Logger.Framework.Scoreimporter, $"Trying to Parse {filepath}");

            using (var reader = new StreamReader(filepath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                try
                {
                    var records = csv.GetRecords<Score>();
                    List<Score> scores = records.ToList();
                    Console.WriteLine("Potential Beatmaps to Download: " + scores.Count());

                    string prev = "random string";
                    int count = 0;

                    var tasks = new List<Task>();

                    foreach (var item in scores)
                    {
                        string check = $"{item.beatmap_id}{item.rank}{item.pp}";

                        if (prev != check)
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await DatabaseController.ImportScore(item);
                                    Interlocked.Increment(ref count);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error importing score: {ex.Message}");
                                }
                            }));
                        }
                        else
                        {
                            Console.WriteLine("Duplicate Score found. Skipping!");
                        }

                        if (tasks.Count >= 50) // Limit to 10 concurrent tasks
                        {
                            await Task.WhenAll(tasks);
                            tasks.Clear();
                        }

                        prev = check;
                    }

                    await Task.WhenAll(tasks);

                    Console.WriteLine($"Scores Successfully imported! ({count} Skipped:{scores.Count() - count}) in {stopwatch.Elapsed.Hours}:{stopwatch.Elapsed.Minutes}:{stopwatch.Elapsed.Seconds}");

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

}
