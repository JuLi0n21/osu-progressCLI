using System;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using MethodTimer;
using Newtonsoft.Json.Linq;
using OsuMemoryDataProvider.OsuMemoryModels;
using osu_progressCLI;
using osu_progressCLI.Datatypes;
using osu_progressCLI.Webserver.Server;

namespace osu1progressbar.Game.Database
{
    /// <summary>
    /// Handeling Database Stuff
    /// </summary>
    public class DatabaseController
    {
        private readonly string dbname = null;
        private readonly string connectionString = "Data Source=osu!progress.db;Version=3;";

        //private readonly string connectionString = "Data Source=osu!TEST.db;Version=3;";

        public DatabaseController()
        {
            //  configJson = File.ReadAllText(configJson);
            // JObject config = JObject.Parse(configJson);
        }

        public bool Init()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    createTables(connection);
                }
                catch (Exception e)
                {
                    Logger.Log(Logger.Severity.Error, Logger.Framework.Database, $"{e.Message}");

                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }

            return true;
        }

        private void createTables(SQLiteConnection connection)
        {
            string createScoreTableQuery =
                @"
                CREATE TABLE IF NOT EXISTS ScoreData (
                    Date TEXT,
                    Title TEXT,
                    BeatmapSetid INTEGER,
                    Beatmapid INTEGER,
                    Osufilename TEXT,
                    Foldername TEXT,
                    Replay TEXT,
                    Playtype TEXT,
                    Ar REAL,
                    Cs REAL,
                    Hp REAL,
                    Od REAL,
                    Status TEXT,
                    SR REAL,
                    Bpm REAL,
                    Artist TEXT,
                    Creator TEXT,
                    Username TEXT,
                    ACC REAL,
                    MaxCombo INTEGER,
                    Score INTEGER,
                    Combo INTEGER,
                    Hit50 INTEGER,
                    Hit100 INTEGER,
                    Hit300 INTEGER,
                    Ur REAL,
                    HitMiss INTEGER,
                    Mode INTEGER,
                    Mods INTEGER,
                    Version TEXT,
                    Tags TEXT,
                    CoverList TEXT,
                    Cover TEXT,
                    Preview TEXT,
                    Time INTEGER,
                    PP REAL,
                    AIM REAL,
                    SPEED REAL,
                    ACCURACYATT REAL,
                    Grade TEXT,
                    FCPP REAL,
                    CONSTRAINT duplicate_score UNIQUE (Beatmapid, Date, Score)
                );
            ";

            Logger.Log(
                Logger.Severity.Debug,
                Logger.Framework.Database,
                $"Creating Database if it dont exists: {dbname}"
            );

            using (var command = new SQLiteCommand(createScoreTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            string timeSpendTableQuery =
                @"
                CREATE TABLE IF NOT EXISTS TimeWasted (Date TEXT ,RawStatus INTEGER, Time REAL)";

            using (var command = new SQLiteCommand(timeSpendTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            string BanchoTimesTableQuery =
                @"
                CREATE TABLE IF NOT EXISTS BanchoTime (Date TEXT ,BanchoStatus TEXT, Time REAL)";

            using (var command = new SQLiteCommand(BanchoTimesTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            string ScoreHelperTableQuery =
                @"
                CREATE TABLE IF NOT EXISTS BeatmapHelper (id INTEGER, SR REAL, Artist TEXT, Creator TEXT, Bpm REAL, Version TEXT, Status TEXT, Tags TEXT, CoverList TEXT, Cover TEXT, Preview TEXT)";

            using (var command = new SQLiteCommand(ScoreHelperTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public void UpdateTimeWasted(int OldStatus, float timeElapsed)
        {
            Logger.Log(
                Logger.Severity.Info,
                Logger.Framework.Database,
                $"Updating Timewasted: {OldStatus} : {timeElapsed}s"
            );

            DateTime time = DateTime.UtcNow;
            string date = time.ToString("yyyy-MM-dd HH:00");

            if (OldStatus == -1)
            {
                return;
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                        @"
                        Update TimeWasted Set Time = Time + @timeElapsed WHERE Date = @Date AND RawStatus = @RawStatus";
                    command.Parameters.AddWithValue("@timeElapsed", timeElapsed);
                    command.Parameters.AddWithValue("@Date", date);
                    command.Parameters.AddWithValue("@RawStatus", OldStatus);

                    int rowsUpdated = command.ExecuteNonQuery();

                    //Update score if already exist

                    if (rowsUpdated == 0)
                    {
                        using (SQLiteCommand insertCommand = new SQLiteCommand(connection))
                        {
                            insertCommand.CommandText =
                                @"
                                INSERT INTO TimeWasted (Date ,RawStatus, Time
                                    ) VALUES (
                                        @Date, @RawStatus, @Time
                                    );
                                ";

                            insertCommand.Parameters.AddWithValue("@Date", date);
                            insertCommand.Parameters.AddWithValue("@RawStatus", OldStatus);
                            insertCommand.Parameters.AddWithValue("@Time", timeElapsed);

                            insertCommand.ExecuteNonQuery();

                            Logger.Log(
                                Logger.Severity.Debug,
                                Logger.Framework.Database,
                                $"New Timewasted for this Hour: {OldStatus} : {timeElapsed}s"
                            );
                        }
                    }
                    else
                    {
                        Logger.Log(
                            Logger.Severity.Debug,
                            Logger.Framework.Database,
                            $"Updated TimeWasted in: {OldStatus} time: {timeElapsed}s"
                        );
                    }
                }

                connection.Close();
            }
        }

        public void UpdateBanchoTime(string BanchoStatus, float timeElapsed)
        {
            Logger.Log(
                Logger.Severity.Info,
                Logger.Framework.Database,
                $"Updating BanchoTime: {BanchoStatus} : {timeElapsed}s"
            );

            DateTime time = DateTime.UtcNow;
            string date = time.ToString("yyyy-MM-dd HH:00");

            if (BanchoStatus == null || timeElapsed == 0)
            {
                return;
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                        @"
                        Update BanchoTime Set Time = Time + @timeElapsed WHERE Date = @Date AND BanchoStatus = @BanchoStatus";
                    command.Parameters.AddWithValue("@timeElapsed", timeElapsed);
                    command.Parameters.AddWithValue("@Date", date);
                    command.Parameters.AddWithValue("@BanchoStatus", BanchoStatus);

                    int rowsUpdated = command.ExecuteNonQuery();

                    //Update score if already exist

                    if (rowsUpdated == 0)
                    {
                        using (SQLiteCommand insertCommand = new SQLiteCommand(connection))
                        {
                            insertCommand.CommandText =
                                @"
                                INSERT INTO BanchoTime (Date ,BanchoStatus, Time
                                    ) VALUES (
                                        @Date, @BanchoStatus, @Time
                                    );
                                ";

                            insertCommand.Parameters.AddWithValue("@Date", date);
                            insertCommand.Parameters.AddWithValue("@BanchoStatus", BanchoStatus);
                            insertCommand.Parameters.AddWithValue("@Time", timeElapsed);

                            insertCommand.ExecuteNonQuery();
                            Logger.Log(
                                Logger.Severity.Debug,
                                Logger.Framework.Database,
                                $"Updating BanchoStatus {BanchoStatus} : {timeElapsed}s"
                            );
                        }
                    }
                    else
                    {
                        Logger.Log(
                            Logger.Severity.Debug,
                            Logger.Framework.Database,
                            $"Updating BanchoStatus: {BanchoStatus} : {timeElapsed}s"
                        );
                    }
                }

                connection.Close();
            }
        }

        public static async Task<bool> ImportScore(OsuParsers.Database.Objects.Score score)
        {
            if (score == null)
                return false;

            var beatmap = OsuDbsExposer.GetBeatmapbyHash(score.BeatmapMD5Hash);

            if (beatmap == null)
                return false;

            Logger.Log(
                Logger.Severity.Debug,
                Logger.Framework.Database,
                $"Trying to Insert Score {beatmap.MD5Hash} {score.ScoreId}"
            );

            using (var connection = new SQLiteConnection("Data Source=osu!progress.db;Version=3;"))
            {
                connection.Open();

                double bpm = -1;
                if (beatmap.TimingPoints.Count > 0)
                {
                    bpm = beatmap
                        .TimingPoints.GroupBy(tp => tp.BPM)
                        .OrderByDescending(group => group.Count())
                        .Select(group => group.Key)
                        .First();
                }

                if (bpm < 0)
                    bpm *= -1;

                bpm = Math.Round(bpm, 2);

                string background = Util.getBackground(beatmap.FolderName, beatmap.FileName);

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                        @"
                       INSERT OR IGNORE INTO ScoreData (
                                Date,
                                BeatmapSetid,
                                Beatmapid,
                                Osufilename,
                                Foldername,
                                Replay,
                                Playtype,
                                Ar,
                                Cs,
                                Hp,
                                Od,
                                Status,
                                SR,
                                Bpm,
                                Creator,
                                Artist,
                                Username,
                                Acc,
                                MaxCombo,
                                Score,
                                Combo,
                                Hit50,
                                Hit100,
                                Hit300,
                                Ur,
                                HitMiss,
                                Mode,
                                Mods,
                                Version,
                                Tags,
                                Cover,
                                Coverlist,
                                Preview, 
                                Time,
                                pp,
                                fcpp,
                                aim,
                                speed,
                                accuracyatt,
                                grade
                                    ) VALUES (
                                        @Date,
                                        @BeatmapSetid,
                                        @Beatmapid,
                                        @Osufilename,
                                        @Foldername,
                                        @Replay,
                                        @Playtype,
                                        @Ar,
                                        @Cs,
                                        @Hp,
                                        @Od,
                                        @Status,
                                        @SR,
                                        @Bpm,
                                        @Creator,
                                        @Artist,
                                        @Username,
                                        @Acc,
                                        @MaxCombo,
                                        @Score,
                                        @Combo,
                                        @Hit50,
                                        @Hit100,
                                        @Hit300,
                                        @Ur,
                                        @HitMiss,
                                        @Mode,
                                        @Mods,
                                        @Version,
                                        @Tags,
                                        @Cover,
                                        @Coverlist,
                                        @Preview,
                                        @Time,
                                        @pp,
                                        @fcpp,
                                        @aim,
                                        @speed,
                                        @accuracyatt,
                                        @grade
                                            );
                    ";

                    command.Parameters.AddWithValue(
                        "@Date",
                        score.ScoreTimestamp.ToString("yyyy-MM-dd HH:mm")
                    );
                    command.Parameters.AddWithValue("@BeatmapSetid", beatmap.BeatmapSetId);
                    command.Parameters.AddWithValue("@Beatmapid", beatmap.BeatmapId);
                    command.Parameters.AddWithValue("@Osufilename", beatmap.FileName);
                    command.Parameters.AddWithValue("@Foldername", beatmap.FolderName);
                    command.Parameters.AddWithValue("@Replay", null);
                    command.Parameters.AddWithValue("@Playtype", "Pass");
                    command.Parameters.AddWithValue("@Ar", beatmap.ApproachRate); //needs mod recalculation
                    command.Parameters.AddWithValue("@Cs", beatmap.CircleSize); //needs mod recalculation
                    command.Parameters.AddWithValue("@Hp", beatmap.HPDrain); //needs mod recalculation
                    command.Parameters.AddWithValue("@Od", beatmap.OverallDifficulty); //needs mod recalculation
                    command.Parameters.AddWithValue("@Status", beatmap.RankedStatus.ToString());

                    PerfomanceAttributes pp = DifficultyAttributes.CalculatePP(
                        beatmap.FolderName,
                        beatmap.FileName,
                        score.ReplayScore,
                        (int)score.Mods,
                        score.CountMiss,
                        score.Count50,
                        score.Count100,
                        score.Count300,
                        0,
                        score.Combo,
                        (int)score.Ruleset
                    );

                    command.Parameters.AddWithValue("@SR", pp.starrating);

                    command.Parameters.AddWithValue("@Bpm", bpm);
                    command.Parameters.AddWithValue("@Creator", beatmap.Creator);
                    command.Parameters.AddWithValue("@Artist", beatmap.Artist);
                    command.Parameters.AddWithValue("@Username", score.PlayerName);
                    double acc =
                        (double)(
                            score.Count300 * 300
                            + score.Count100 * 100
                            + score.Count50 * 50
                            + score.CountMiss * 0
                        )
                        / (
                            (score.Count300 + score.Count100 + score.Count50 + score.CountMiss)
                            * 300
                        )
                        * 100;

                    command.Parameters.AddWithValue("@Acc", Math.Round(acc, 2));

                    command.Parameters.AddWithValue("@MaxCombo", pp.Maxcombo);
                    command.Parameters.AddWithValue("@Score", score.ReplayScore);
                    command.Parameters.AddWithValue("@Combo", score.Combo);
                    command.Parameters.AddWithValue("@Hit50", score.Count50);
                    command.Parameters.AddWithValue("@Hit100", score.Count100);
                    command.Parameters.AddWithValue("@Hit300", score.Count300);
                    command.Parameters.AddWithValue("@Ur", 0);
                    command.Parameters.AddWithValue("@HitMiss", score.CountMiss);
                    command.Parameters.AddWithValue("@Mode", score.Ruleset);
                    command.Parameters.AddWithValue("@Mods", score.Mods);
                    command.Parameters.AddWithValue("@Version", beatmap.Difficulty);
                    command.Parameters.AddWithValue("@Cover", background);
                    command.Parameters.AddWithValue("@Coverlist", background);
                    command.Parameters.AddWithValue("@Preview", beatmap.AudioFileName);
                    command.Parameters.AddWithValue("@Tags", beatmap.Tags);
                    command.Parameters.AddWithValue("@Time", beatmap.DrainTime);

                    command.Parameters.AddWithValue("@pp", pp.pp);

                    PerfomanceAttributes fcpp = DifficultyAttributes.CalculatePP(
                        beatmap.FolderName,
                        beatmap.FileName,
                        score.ReplayScore,
                        (int)score.Mods,
                        0,
                        score.Count50,
                        score.Count100,
                        score.Count300,
                        0,
                        0,
                        (int)score.Ruleset
                    );

                    command.Parameters.AddWithValue("@fcpp", fcpp.pp);

                    command.Parameters.AddWithValue("@aim", pp.aim);
                    command.Parameters.AddWithValue("@speed", pp.speed);
                    command.Parameters.AddWithValue("@accuracyatt", pp.accuracy);
                    command.Parameters.AddWithValue(
                        "@grade",
                        DifficultyAttributes.CalculateGrade(
                            score.Count300,
                            score.Count100,
                            score.Count50,
                            score.CountMiss
                        )
                    );

                    int rows = command.ExecuteNonQuery();
                    Console.WriteLine("rows inserted: " + rows);
                    connection.Close();
                    Logger.Log(
                        Logger.Severity.Debug,
                        Logger.Framework.Database,
                        $"Saved Score: {beatmap.Title}"
                    );
                    return true;
                }
            }
        }

        public static async Task<bool> ImportScore(object importscore)
        {
            ImportScore score = (ImportScore)importscore;
            Logger.Log(
                Logger.Severity.Debug,
                Logger.Framework.Database,
                $"Trying to Insert Score {score.file_md5} {score.pp}"
            );
            using (var connection = new SQLiteConnection("Data Source=osu!progress.db;Version=3;"))
            {
                try
                {
                    connection.Open();

                    string foldername = "";

                    string osufilename = "";

                    string background = "";
                    //GET FOLDER AND FILENAME
                    HttpClient client = new HttpClient();

                    foldername = await ApiController.Instance.DownloadBeatmapset(
                        client,
                        score.set_id,
                        true
                    );

                    client.Dispose();

                    Logger.Log(
                        Logger.Severity.Debug,
                        Logger.Framework.Database,
                        $"Trying to Get Filename {score.beatmap_id}"
                    );
                    osufilename = Util.osufile(foldername, score.diffname);

                    Logger.Log(
                        Logger.Severity.Debug,
                        Logger.Framework.Database,
                        $"Trying to Find Background for {score.beatmap_id}"
                    );
                    background = Util.getBackground(foldername, osufilename);

                    string insertQuery =
                        @"
                        INSERT OR IGNORE INTO ScoreData (
                                Date,
                                Title,
                                BeatmapSetid,
                                Beatmapid,
                                Osufilename,
                                Foldername,
                                Replay,
                                Playtype,
                                Ar,
                                Cs,
                                Hp,
                                Od,
                                Status,
                                SR,
                                Bpm,
                                Creator,
                                Artist,
                                Username,
                                Acc,
                                MaxCombo,
                                Score,
                                Combo,
                                Hit50,
                                Hit100,
                                Hit300,
                                Ur,
                                HitMiss,
                                Mode,
                                Mods,
                                Version,
                                Tags,
                                Cover,
                                Coverlist,
                                Preview, 
                                Time,
                                pp,
                                fcpp,
                                aim,
                                speed,
                                accuracyatt,
                                grade
                                    ) VALUES (
                                        @Date,
                                        @Title,
                                        @BeatmapSetid,
                                        @Beatmapid,
                                        @Osufilename,
                                        @Foldername,
                                        @Replay,
                                        @Playtype,
                                        @Ar,
                                        @Cs,
                                        @Hp,
                                        @Od,
                                        @Status,
                                        @SR,
                                        @Bpm,
                                        @Creator,
                                        @Artist,
                                        @Username,
                                        @Acc,
                                        @MaxCombo,
                                        @Score,
                                        @Combo,
                                        @Hit50,
                                        @Hit100,
                                        @Hit300,
                                        @Ur,
                                        @HitMiss,
                                        @Mode,
                                        @Mods,
                                        @Version,
                                        @Tags,
                                        @Cover,
                                        @Coverlist,
                                        @Preview,
                                        @Time,
                                        @pp,
                                        @fcpp,
                                        @aim,
                                        @speed,
                                        @accuracyatt,
                                        @grade
                                            );
                    ";

                    PerfomanceAttributes fcpp = DifficultyAttributes.CalculatePP(
                        foldername,
                        osufilename,
                        score.score,
                        score.enabled_mods,
                        0,
                        score.count50,
                        score.count100,
                        score.count300,
                        0,
                        0
                    );

                    //YYYY-MM-DD HH:MM THIS FORMAT IS SUPPOSED TO BE USED
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@Date",
                            score.date_played.ToString("yyyy-MM-dd HH:mm")
                        );
                        command.Parameters.AddWithValue("@Title", score.title);
                        command.Parameters.AddWithValue("@BeatmapSetid", score.set_id);
                        command.Parameters.AddWithValue("@Beatmapid", score.beatmap_id);
                        command.Parameters.AddWithValue("@Osufilename", osufilename);
                        command.Parameters.AddWithValue("@Foldername", foldername);
                        command.Parameters.AddWithValue("@Replay", null);
                        command.Parameters.AddWithValue("@Playtype", "Pass");
                        command.Parameters.AddWithValue("@Ar", score.ar);
                        command.Parameters.AddWithValue("@Cs", score.cs); //needs mod recalculation
                        command.Parameters.AddWithValue("@Hp", score.hp); //needs mod recalculation
                        command.Parameters.AddWithValue("@Od", score.od);
                        command.Parameters.AddWithValue("@Status", "Ranked");
                        command.Parameters.AddWithValue("@SR", score.stars);
                        command.Parameters.AddWithValue("@Bpm", score.bpm);
                        command.Parameters.AddWithValue("@Creator", score.creator);
                        command.Parameters.AddWithValue("@Artist", score.artist);
                        command.Parameters.AddWithValue(
                            "@Username",
                            Credentials.Instance.GetConfig().username
                        );
                        command.Parameters.AddWithValue("@Acc", score.accuracy);
                        command.Parameters.AddWithValue("@MaxCombo", score.maxcombo);
                        command.Parameters.AddWithValue("@Score", score.score);
                        command.Parameters.AddWithValue("@Combo", score.combo);
                        command.Parameters.AddWithValue("@Hit50", score.count50);
                        command.Parameters.AddWithValue("@Hit100", score.count100);
                        command.Parameters.AddWithValue("@Hit300", score.count300);
                        command.Parameters.AddWithValue("@Ur", 0);
                        command.Parameters.AddWithValue("@HitMiss", score.countmiss);
                        command.Parameters.AddWithValue("@Mode", score.mode);
                        command.Parameters.AddWithValue("@Mods", score.enabled_mods);
                        command.Parameters.AddWithValue("@Version", score.diffname);
                        command.Parameters.AddWithValue("@Cover", background);
                        command.Parameters.AddWithValue("@Coverlist", background);
                        command.Parameters.AddWithValue("@Preview", null);
                        command.Parameters.AddWithValue("@Tags", score.tags);
                        command.Parameters.AddWithValue("@Time", score.drain);
                        command.Parameters.AddWithValue("@pp", score.pp);

                        command.Parameters.AddWithValue("@fcpp", fcpp.pp);
                        command.Parameters.AddWithValue("@aim", 0);
                        command.Parameters.AddWithValue("@speed", 0);
                        command.Parameters.AddWithValue("@accuracyatt", 0);
                        command.Parameters.AddWithValue("@grade", score.rank);

                        command.ExecuteNonQuery();
                    }

                    Logger.Log(
                        Logger.Severity.Debug,
                        Logger.Framework.Database,
                        $"Saved Score: {score.title}"
                    );
                    return true;
                }
                catch (Exception e)
                {
                    Logger.Log(
                        Logger.Severity.Debug,
                        Logger.Framework.Database,
                        $"Failed to Import Score({score.file_md5}):{e.Message}"
                    );
                    return false;
                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public List<Score> GetPotentcialtopplays(int ppcutoffpoint)
        {
            List<Score> scores = new List<Score>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    string insertquery =
                        @"SELECT *, rowid AS id 
                            FROM ScoreData
                            WHERE COMBO >= MAXCOMBO * 0.45
                            AND COMBO <= MAXCOMBO * 0.95
                            AND HITMISS <= 5
                            AND FCPP > @pp; 
                                            ";

                    command.CommandText = insertquery;

                    command.Parameters.AddWithValue("@pp", ppcutoffpoint);

                    command.ExecuteNonQuery( );

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Score score = new Score(reader);
                            scores.Add(score);
                        }
                    }
                }
            }

            return scores;
        }

        public async void InsertScore(
            OsuBaseAddresses baseAddresses,
            float timeElapsed,
            string playtype,
            string replay = null
        )
        {
            var localbeatmap = OsuDbsExposer.GetBeatmapbyHash(baseAddresses.Beatmap.Md5);

            Score score = new();

            score.Title = baseAddresses.Beatmap.OsuFileName;

            if (localbeatmap != null)
            {
                score.Title = localbeatmap.Title;
                score.Bpm = 0;

                double oldbpm = 1 / localbeatmap.TimingPoints[0].BPM * 1000 * 60;
                ;

                int oldoffset = 0;
                List<KeyValuePair<double, int>> bpmpairs = new List<KeyValuePair<double, int>>
                {
                    new KeyValuePair<double, int>(oldbpm, (int)localbeatmap.TimingPoints[0].Offset)
                };
                for (int i = 1; i < localbeatmap.TimingPoints.Count - 1; i++)
                {
                    if (localbeatmap.TimingPoints[i].Inherited == false)
                    {
                        double newbpm = 1 / localbeatmap.TimingPoints[i].BPM * 1000 * 60;

                        if (newbpm != oldbpm)
                        {
                            KeyValuePair<double, int> existingPair = bpmpairs.FirstOrDefault(t =>
                                t.Key == oldbpm
                            );

                            int newoffset = (int)localbeatmap.TimingPoints[i].Offset;

                            if (existingPair.Equals(default(KeyValuePair<double, int>)))
                            {
                                bpmpairs.Add(
                                    new KeyValuePair<double, int>(oldbpm, (newoffset - oldoffset))
                                );
                            }
                            else
                            {
                                int updatedValue = existingPair.Value + (newoffset - oldoffset);
                                bpmpairs.Remove(existingPair);
                                bpmpairs.Add(new KeyValuePair<double, int>(oldbpm, updatedValue));
                            }

                            oldbpm = newbpm;
                            oldoffset = newoffset;
                        }
                    }
                }

                score.Bpm = Math.Round(
                    bpmpairs.OrderByDescending(kv => kv.Value).FirstOrDefault().Key
                );

                score.BeatmapSetId = localbeatmap.BeatmapSetId;
                score.BeatmapId = localbeatmap.BeatmapId;
                score.OsuFilename = localbeatmap.FileName;
                score.FolderName = localbeatmap.FolderName;
                score.Ar = localbeatmap.ApproachRate;
                score.Cs = localbeatmap.CircleSize;
                score.Hp = localbeatmap.HPDrain;
                score.Od = localbeatmap.OnlineOffset;
                score.Status = localbeatmap.RankedStatus.ToString();
                score.Creator = localbeatmap.Creator;
                score.Artist = localbeatmap.Artist;
                score.Version = localbeatmap.Difficulty;
                score.Cover = Util.getBackground(localbeatmap.FolderName, localbeatmap.FileName);
                score.CoverList = score.Cover;
                score.Preview = localbeatmap.AudioFileName;
                score.Tags = localbeatmap.Tags;
                //score.Replay = replay;
                //score.PlayType = playtype;
                //score.SR = localbeatmap.StandardStarRating.ToString();
                //score.Bpm = localbeatmap.timing...
                //score.Username =
                //score.Acc =
                //score.MaxCombo =
                //score.score
                //score.combo
                //score.hit50
                //score.hit100
                //score.hit300
                //score.hitMiss
                //score.Ur
                //score.mode
                //score.mods
                //score.time
                //score.pp
                //score.fcpp
                //score.aim
                //score.speed
                //score.accuarcyatt
                //score.grade
            }

            JObject apibeatmap = await ApiController.Instance.getExpandedBeatmapinfo(
                baseAddresses.Beatmap.Id.ToString()
            );

            if (apibeatmap != null)
            {
                score.Bpm = double.Parse(apibeatmap["bpm"].ToString());
                score.Creator = apibeatmap["beatmapset"]["creator"].ToString();
                score.Artist = apibeatmap["beatmapset"]["artist"].ToString();
                score.Status = apibeatmap["beatmapset"]["status"].ToString();
                score.CoverList = apibeatmap["beatmapset"]["covers"]["cover@2x"].ToString();
                score.Cover = apibeatmap["beatmapset"]["covers"]["list@2x"].ToString();
                score.Version = apibeatmap["version"].ToString();
                score.Tags = apibeatmap["beatmapset"]["tags"].ToString();
                score.Preview = apibeatmap["beatmapset"]["preview_url"].ToString();
            }

            var perfAtt = DifficultyAttributes.CalculatePP(
                baseAddresses.Beatmap.FolderName,
                baseAddresses.Beatmap.OsuFileName,
                baseAddresses.Player.Score,
                baseAddresses.Player.Mods.Value,
                baseAddresses.Player.HitMiss,
                baseAddresses.Player.Hit50,
                baseAddresses.Player.Hit100,
                baseAddresses.Player.Hit300,
                0, //allow acc calc
                baseAddresses.Player.MaxCombo,
                baseAddresses.Player.Mode
            );

            if (playtype == "Cancel" || playtype == "Fail" || playtype == "Retry")
            {
                perfAtt.grade = "F";
            }

            var fcPerfAtt = DifficultyAttributes.CalculatePP(
                baseAddresses.Beatmap.FolderName,
                baseAddresses.Beatmap.OsuFileName,
                baseAddresses.Player.Score,
                baseAddresses.Player.Mods.Value,
                0, //force no Misses
                baseAddresses.Player.Hit50,
                baseAddresses.Player.Hit100,
                baseAddresses.Player.Hit300,
                0, //allow acc calc
                0, //force fc
                baseAddresses.Player.Mode
            );

            score.Date = DateTime.Now;
            score.Replay = replay;
            score.PlayType = playtype;
            score.SR = perfAtt.starrating;
            score.Username = baseAddresses.Player.Username;
            score.Acc = Math.Round(baseAddresses.Player.Accuracy, 2);
            score.MaxCombo = perfAtt.Maxcombo;
            score.score = baseAddresses.Player.Score;
            score.Combo = baseAddresses.Player.MaxCombo;
            score.ScoreValue = baseAddresses.Player.Score;
            score.Hit50 = baseAddresses.Player.Hit50;
            score.Hit100 = baseAddresses.Player.Hit100;
            score.Hit300 = baseAddresses.Player.Hit300;
            score.HitMiss = baseAddresses.Player.HitMiss;
            score.Ur = 0; // calc or something...
            score.Mode = baseAddresses.Player.Mode.ToString();
            score.Mods = baseAddresses.Player.Mods.Value;
            score.Time = int.Parse(timeElapsed.ToString());
            score.PP = perfAtt.pp;
            score.FCPP = fcPerfAtt.pp;
            score.AIM = perfAtt.aim;
            score.SPEED = perfAtt.speed;
            score.ACCURACYATT = perfAtt.accuracy;
            score.Grade = perfAtt.grade;

            string insertQuery =
                "INSERT OR IGNORE INTO ScoreData (Date,Title,BeatmapSetid,Beatmapid,Osufilename,Foldername,Replay,Playtype,Ar,Cs,Hp,Od,Status,SR,Bpm,Creator,Artist,Username,Acc,MaxCombo,Score,Combo,Hit50,Hit100,Hit300,Ur,HitMiss,Mode,Mods,Version,Tags,Cover,Coverlist,Preview,Time,pp,fcpp,aim,speed,accuracyatt,grade"
                + ") VALUES ("
                + "@Date,@Title,@BeatmapSetid,@Beatmapid,@Osufilename,@Foldername,@Replay,@Playtype,@Ar,@Cs,@Hp,@Od,@Status,@SR,@Bpm,@Creator,@Artist,@Username,@Acc,@MaxCombo,@Score,@Combo,@Hit50,@Hit100,@Hit300,@Ur,@HitMiss,@Mode,@Mods,@Version,@Tags,@Cover,@Coverlist,@Preview,@Time,@pp,@fcpp,@aim,@speed,@accuracyatt,@grade"
                + ");";

            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@Date",
                            score.Date.ToString("yyyy-MM-dd HH:mm:ss")
                        );
                        command.Parameters.AddWithValue("@Title", score.Title);
                        command.Parameters.AddWithValue("@BeatmapSetid", score.BeatmapSetId);
                        command.Parameters.AddWithValue("@Beatmapid", score.BeatmapId);
                        command.Parameters.AddWithValue("@Osufilename", score.OsuFilename);
                        command.Parameters.AddWithValue("@Foldername", score.FolderName);
                        command.Parameters.AddWithValue("@Replay", score.Replay);
                        command.Parameters.AddWithValue("@Playtype", score.PlayType);
                        command.Parameters.AddWithValue("@Ar", score.Ar);
                        command.Parameters.AddWithValue("@Cs", score.Cs); //needs mod recalculation
                        command.Parameters.AddWithValue("@Hp", score.Hp); //needs mod recalculation
                        command.Parameters.AddWithValue("@Od", score.Od);
                        command.Parameters.AddWithValue("@Status", score.Status);
                        command.Parameters.AddWithValue("@SR", score.SR);
                        command.Parameters.AddWithValue("@Bpm", score.Bpm);
                        command.Parameters.AddWithValue("@Creator", score.Creator);
                        command.Parameters.AddWithValue("@Artist", score.Artist);
                        command.Parameters.AddWithValue("@Username", score.Username);
                        command.Parameters.AddWithValue("@Acc", score.Acc);
                        command.Parameters.AddWithValue("@MaxCombo", score.MaxCombo);
                        command.Parameters.AddWithValue("@Score", score.ScoreValue);
                        command.Parameters.AddWithValue("@Combo", score.Combo);
                        command.Parameters.AddWithValue("@Hit50", score.Hit50);
                        command.Parameters.AddWithValue("@Hit100", score.Hit100);
                        command.Parameters.AddWithValue("@Hit300", score.Hit300);
                        command.Parameters.AddWithValue("@Ur", score.Ur);
                        command.Parameters.AddWithValue("@HitMiss", score.HitMiss);
                        command.Parameters.AddWithValue("@Mode", score.Mode);
                        command.Parameters.AddWithValue("@Mods", score.Mods);
                        command.Parameters.AddWithValue("@Version", score.Version);
                        command.Parameters.AddWithValue("@Cover", score.Cover);
                        command.Parameters.AddWithValue("@Coverlist", score.CoverList);
                        command.Parameters.AddWithValue("@Preview", score.Preview);
                        command.Parameters.AddWithValue("@Tags", score.Tags);
                        command.Parameters.AddWithValue("@Time", score.Time);
                        command.Parameters.AddWithValue("@pp", score.PP);
                        command.Parameters.AddWithValue("@fcpp", score.FCPP);
                        command.Parameters.AddWithValue("@aim", score.AIM);
                        command.Parameters.AddWithValue("@speed", score.SPEED);
                        command.Parameters.AddWithValue("@accuracyatt", score.ACCURACYATT);
                        command.Parameters.AddWithValue("@grade", score.Grade);

                        int rows = command.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            command.CommandText = "select last_insert_rowid()";

                            score.Id = int.Parse(command.ExecuteScalar().ToString());
                            score.ModsString = ModParser.ParseMods(baseAddresses.Player.Mods.Value);

                            SSEstream.SendScore(score);
                        }
                    }

                    Logger.Log(
                        Logger.Severity.Debug,
                        Logger.Framework.Database,
                        $"Saved Score: {baseAddresses.Beatmap.OsuFileName}"
                    );
                }
                catch (Exception e)
                {
                    Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"{e.Message}");

                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [Time]
        public List<Score> GetScoresInTimeSpan(DateTime from, DateTime to)
        {
            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");

            List<Score> scores = new List<Score>();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (var command = new SQLiteCommand(connection))
                {
                    connection.Open();

                    command.CommandText =
                        "SELECT rowid as id, * "
                        + "FROM ScoreData "
                        + "WHERE datetime(Date) BETWEEN @from AND @to "
                        + "ORDER BY Date DESC "
                        + "LIMIT 1000;";
                    //";";
                    command.Parameters.AddWithValue("@from", fromFormatted);
                    command.Parameters.AddWithValue("@to", toFormatted);

                    DateTime dateString = DateTime.Now;

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Score score = new Score(reader);
                            scores.Add(score);
                        }
                    }
                }
                connection.Close();
            }
            return scores;
        }

        [Time]
        public List<Score> GetScoreSearch(
            DateTime from = new DateTime(),
            DateTime to = new DateTime(),
            string search = ""
        )
        {
            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");

            List<Score> scores = new();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                using (var command = new SQLiteCommand(connection))
                {
                    connection.Open();

                    StringBuilder queryBuilder = new StringBuilder(
                        "SELECT rowid as id, * FROM ScoreData WHERE 1=1 "
                    );

                    if (from != null || to != null)
                    {
                        queryBuilder.Append("AND datetime(Date) BETWEEN @from AND @to ");
                        queryBuilder.Append(search);
                        queryBuilder.Append("ORDER BY Date DESC LIMIT 1000;");
                        command.Parameters.AddWithValue("@from", fromFormatted);
                        command.Parameters.AddWithValue("@to", toFormatted);
                    }

                    command.CommandText = queryBuilder.ToString();
                    DateTime dateString = DateTime.Now;

                    try
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Score score = new Score(reader);
                                scores.Add(score);
                            }
                        }
                    }
                    catch
                    {
                        return null;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
                return scores;
            }
        }

        public List<Dictionary<string, object>> GetScoreAveragesbyDay(DateTime from, DateTime to)
        {
            List<Dictionary<string, object>> scoreAverages = new List<Dictionary<string, object>>();

            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                string commandstring =
                    @"
            SELECT 
                strftime('%Y-%m-%d', Date) AS FDate,
                AVG(Bpm) AS AverageBpm,
                AVG(SR) AS AverageSR,
                AVG(Acc) AS AverageAccuracy,
                AVG(Ar) AS AverageAr,
                AVG(Cs) AS AverageCs,
                AVG(Hp) AS AverageHp,
                AVG(Od) AS AverageOd
            FROM ScoreData
            WHERE Date BETWEEN @from AND @to
            GROUP BY Date(Date)
        ";

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = commandstring;
                    command.Parameters.AddWithValue("@from", fromFormatted);
                    command.Parameters.AddWithValue("@to", toFormatted);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<string, object> result = new Dictionary<string, object>();

                            result["Date"] = reader["FDate"].ToString();
                            result["AverageBpm"] = Convert.ToDouble(reader["AverageBpm"]);
                            result["AverageSR"] = Convert.ToDouble(reader["AverageSR"]);
                            result["AverageAccuracy"] = Convert.ToDouble(reader["AverageAccuracy"]);
                            result["AverageAr"] = Convert.ToDouble(reader["AverageAr"]);
                            result["AverageCs"] = Convert.ToDouble(reader["AverageCs"]);
                            result["AverageHp"] = Convert.ToDouble(reader["AverageHp"]);
                            result["AverageOd"] = Convert.ToDouble(reader["AverageOd"]);

                            scoreAverages.Add(result);
                        }
                    }
                }
            }
            return scoreAverages;
        }

        public WeekCompare GetWeekCompare()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                WeekCompare week = new WeekCompare();

                connection.Open();

                string sqlQuery =
                    @"
                         WITH StatusTime AS (
                            SELECT
                                BanchoStatus AS Status,
                                SUM(CASE
                                    WHEN julianday('now') - julianday(Date) BETWEEN 0 AND 7 THEN Time
                                    ELSE 0
                                END) AS Last7DaysTime,
                                SUM(CASE
                                    WHEN julianday('now') - julianday(Date) BETWEEN 8 AND 15 THEN Time
                                    ELSE 0
                                END) AS EightTo15DaysTime
                            FROM BanchoTime
                            GROUP BY Status
                        )

                        SELECT
                            Status,
                            TotalLast7DaysTime,
                            TotalEightTo15DaysTime
                        FROM StatusTime,
                            (SELECT
                                SUM(Last7DaysTime) AS TotalLast7DaysTime,
                                SUM(EightTo15DaysTime) AS TotalEightTo15DaysTime
                            FROM StatusTime) AS Totals
                        WHERE Last7DaysTime = (SELECT MAX(Last7DaysTime) FROM StatusTime);
                        ";

                using (SQLiteCommand cmd = new SQLiteCommand(sqlQuery, connection))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            week.Status = reader["Status"].ToString();
                            week.LastWeek = Convert.ToDouble(reader["TotalEightTo15DaysTime"]);
                            week.ThisWeek = Convert.ToDouble(reader["TotalLast7DaysTime"]);
                        }
                    }
                }

                sqlQuery =
                    @"SELECT SUM(Time) AS TotalTimeWasted, RawStatus
                            FROM TimeWasted
                            WHERE Date >= date('now', '-7 days')
                            GROUP BY RawStatus
                            ORDER BY TotalTimeWasted DESC;
                            ";

                using (SQLiteCommand cmd = new SQLiteCommand(sqlQuery, connection))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            week.Screen = DifficultyAttributes.ScreenConverter(
                                int.Parse(reader["RawStatus"].ToString())
                            );
                        }
                    }
                }
                return week;
            }
            return new WeekCompare();
        }

        public int GetScoreAmounts(DateTime from, DateTime to)
        {
            int rows = -1;
            string fromFormatted = from.ToString("yyyy -MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                        "SELECT * FROM ScoreData WHERE datetime(Date) BETWEEN @from AND @to ;";

                    command.Parameters.AddWithValue("@from", fromFormatted);
                    command.Parameters.AddWithValue("@to", toFormatted);

                    rows = command.ExecuteNonQuery();

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows++;
                        }
                    }
                }
            }
            return rows;
        }

        public List<KeyValuePair<string, double>> GetBanchoTime(DateTime from, DateTime to)
        {
            List<KeyValuePair<string, double>> BanchoTime =
                new List<KeyValuePair<string, double>>();
            int rows = 0;

            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                        "SELECT BanchoStatus, SUM(Time) as Time FROM BanchoTime WHERE datetime(Date) BETWEEN @from AND @to Group by BanchoStatus;";

                    command.Parameters.AddWithValue("@from", fromFormatted);
                    command.Parameters.AddWithValue("@to", toFormatted);

                    //Console.WriteLine(rows.ToString());

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BanchoTime.Add(
                                new KeyValuePair<string, double>(
                                    reader["BanchoStatus"].ToString(),
                                    Convert.ToDouble(reader["Time"])
                                )
                            );
                        }
                    }
                }
            }
            return BanchoTime;
        }

        public List<Dictionary<string, object>> GetBanchoTimebyDay(DateTime from, DateTime to)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                        @"
                    SELECT strftime('%Y-%m-%d', Date) AS FormattedDate, BanchoStatus, SUM(Time) AS TotalTime
                    FROM BanchoTime
                    GROUP BY FormattedDate, BanchoStatus;";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var date = DateTime.Parse(reader["FormattedDate"].ToString());
                            var banchoStatus = reader["BanchoStatus"].ToString();
                            var totalTime = Convert.ToDouble(reader["TotalTime"]);

                            // Check if there is an entry for the current date in the result list.
                            var entry = result.FirstOrDefault(dict =>
                                dict.ContainsKey("Date")
                                && ((DateTime)dict["Date"]).Date == date.Date
                            );

                            if (entry == null)
                            {
                                // If the entry doesn't exist, create a new dictionary for the date.
                                entry = new Dictionary<string, object>
                                {
                                    { "Date", date.Date },
                                    { banchoStatus, totalTime }
                                };
                                result.Add(entry);
                            }
                            else
                            {
                                // If the entry exists, update the existing dictionary.
                                if (entry.ContainsKey(banchoStatus))
                                {
                                    entry[banchoStatus] =
                                        Convert.ToDouble(entry[banchoStatus]) + totalTime;
                                }
                                else
                                {
                                    entry[banchoStatus] = totalTime;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public Score GetScore(int id)
        {
            Score score;

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT rowid as id, * FROM SCOREDATA WHERE id = @id";

                    command.Parameters.AddWithValue("@id", id);

                    command.ExecuteNonQuery();

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return new Score(reader);
                        }
                    }
                }
            }
            return null;
        }

        public List<KeyValuePair<string, double>> GetTimeWasted(DateTime from, DateTime to)
        {
            List<KeyValuePair<string, double>> BanchoTime =
                new List<KeyValuePair<string, double>>();
            int rows = 0;

            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                        "SELECT strftime('%Y-%m-%d', Date) as Day, RawStatus, SUM(Time) as Time FROM TimeWasted Group by RawStatus";

                    command.Parameters.AddWithValue("@from", fromFormatted);
                    command.Parameters.AddWithValue("@to", toFormatted);

                    //Console.WriteLine(rows.ToString());

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BanchoTime.Add(
                                new KeyValuePair<string, double>(
                                    reader["RawStatus"].ToString(),
                                    Convert.ToDouble(reader["Time"])
                                )
                            );
                        }
                    }
                }
            }
            return BanchoTime;
        }

        public List<Dictionary<string, object>> GetTimeWastedByDay(DateTime from, DateTime to)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText =
                        @"
                    SELECT strftime('%Y-%m-%d', Date) AS FormattedDate, RawStatus, SUM(Time) AS TotalTime
                    FROM TimeWasted
                    GROUP BY FormattedDate, RawStatus;
                    ";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var date = DateTime.Parse(reader["FormattedDate"].ToString());
                            var banchoStatus = reader["RawStatus"].ToString();
                            var totalTime = Convert.ToDouble(reader["TotalTime"]);

                            // Check if there is an entry for the current date in the result list.
                            var entry = result.FirstOrDefault(dict =>
                                dict.ContainsKey("Date")
                                && ((DateTime)dict["Date"]).Date == date.Date
                            );

                            if (entry == null)
                            {
                                // If the entry doesn't exist, create a new dictionary for the date.
                                entry = new Dictionary<string, object>
                                {
                                    { "Date", date.Date },
                                    { banchoStatus, totalTime }
                                };
                                result.Add(entry);
                            }
                            else
                            {
                                // If the entry exists, update the existing dictionary.
                                if (entry.ContainsKey(banchoStatus))
                                {
                                    entry[banchoStatus] =
                                        Convert.ToDouble(entry[banchoStatus]) + totalTime;
                                }
                                else
                                {
                                    entry[banchoStatus] = totalTime;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public int scorecount()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"SELECT COUNT(*) FROM ScoreData";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            return int.Parse(reader.GetValue(0).ToString());
                        }
                    }
                }
            }
            return 0;
        }
    }

    public class WeekCompare
    {
        public string Status { get; set; }
        public string Screen { get; set; }
        public double LastWeek { get; set; }
        public double ThisWeek { get; set; }
    }
}
