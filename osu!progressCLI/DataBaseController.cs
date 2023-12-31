﻿using CsvHelper;
using Newtonsoft.Json.Linq;
using osu_progressCLI;
using osu_progressCLI.Datatypes;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuParsers.Beatmaps;
using OsuParsers.Beatmaps.Objects;
using Parlot.Fluent;
using System;
using System.Data.SQLite;
using System.Text;
using MethodTimer;
using osu_progressCLI.Webserver.Server;
using System.Runtime.InteropServices;
using OsuParsers.Storyboards.Commands;

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

            string createScoreTableQuery = @"
                CREATE TABLE IF NOT EXISTS ScoreData (
                    Date TEXT,
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

            Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Creating Database if it dont exists: {dbname}");

            using (var command = new SQLiteCommand(createScoreTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            string timeSpendTableQuery = @"
                CREATE TABLE IF NOT EXISTS TimeWasted (Date TEXT ,RawStatus INTEGER, Time REAL)";

            using (var command = new SQLiteCommand(timeSpendTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            string BanchoTimesTableQuery = @"
                CREATE TABLE IF NOT EXISTS BanchoTime (Date TEXT ,BanchoStatus TEXT, Time REAL)";

            using (var command = new SQLiteCommand(BanchoTimesTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            string ScoreHelperTableQuery = @"
                CREATE TABLE IF NOT EXISTS BeatmapHelper (id INTEGER, SR REAL, Artist TEXT, Creator TEXT, Bpm REAL, Version TEXT, Status TEXT, Tags TEXT, CoverList TEXT, Cover TEXT, Preview TEXT)";

            using (var command = new SQLiteCommand(ScoreHelperTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

        }

        public void UpdateTimeWasted(int OldStatus, float timeElapsed)
        {
            Logger.Log(Logger.Severity.Info, Logger.Framework.Database, $"Updating Timewasted: {OldStatus} : {timeElapsed}s");

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

                    command.CommandText = @"
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

                            insertCommand.CommandText = @"
                                INSERT INTO TimeWasted (Date ,RawStatus, Time
                                    ) VALUES (
                                        @Date, @RawStatus, @Time
                                    );
                                ";

                            insertCommand.Parameters.AddWithValue("@Date", date);
                            insertCommand.Parameters.AddWithValue("@RawStatus", OldStatus);
                            insertCommand.Parameters.AddWithValue("@Time", timeElapsed);

                            insertCommand.ExecuteNonQuery();

                            Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"New Timewasted for this Hour: {OldStatus} : {timeElapsed}s");

                        }
                    }
                    else
                    {
                        Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Updated TimeWasted in: {OldStatus} time: {timeElapsed}s");
                    }
                }

                connection.Close();
            }
        }

        public void UpdateBanchoTime(string BanchoStatus, float timeElapsed)
        {
            Logger.Log(Logger.Severity.Info, Logger.Framework.Database, $"Updating BanchoTime: {BanchoStatus} : {timeElapsed}s");

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

                    command.CommandText = @"
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

                            insertCommand.CommandText = @"
                                INSERT INTO BanchoTime (Date ,BanchoStatus, Time
                                    ) VALUES (
                                        @Date, @BanchoStatus, @Time
                                    );
                                ";

                            insertCommand.Parameters.AddWithValue("@Date", date);
                            insertCommand.Parameters.AddWithValue("@BanchoStatus", BanchoStatus);
                            insertCommand.Parameters.AddWithValue("@Time", timeElapsed);

                            insertCommand.ExecuteNonQuery();
                            Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Updating BanchoStatus {BanchoStatus} : {timeElapsed}s");

                        }
                    }
                    else
                    {
                        Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Updating BanchoStatus: {BanchoStatus} : {timeElapsed}s");
                    }
                }

                connection.Close();
            }
        }

        [Time]
        public async static Task<bool> ImportScore(OsuParsers.Database.Objects.DbBeatmap beatmap, OsuParsers.Database.Objects.Score score)
        {
            if (beatmap == null || score == null) {
                return false;
            }

            Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Trying to Insert Score {beatmap.MD5Hash} {score.ScoreId}");

            using (var connection = new SQLiteConnection("Data Source=osu!progress.db;Version=3;"))
            {
                connection.Open();

                double bpm = -1;
                if (beatmap.TimingPoints.Count > 0)
                {
                    bpm = beatmap.TimingPoints.GroupBy(tp => tp.BPM)
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
                    command.CommandText = @"
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

                        command.Parameters.AddWithValue("@Date", score.ScoreTimestamp.ToString("yyyy-MM-dd HH:mm"));
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

                    PerfomanceAttributes pp = DifficultyAttributes.CalculatePP(beatmap.FolderName, beatmap.FileName, score.ReplayScore,
                      (int)score.Mods, score.CountMiss, score.Count50, score.Count100, score.Count300, 0, score.Combo, (int)score.Ruleset);

                   

                    command.Parameters.AddWithValue("@SR", pp.starrating);

                    command.Parameters.AddWithValue("@Bpm", bpm);
                        command.Parameters.AddWithValue("@Creator", beatmap.Creator);
                        command.Parameters.AddWithValue("@Artist", beatmap.Artist);
                        command.Parameters.AddWithValue("@Username", score.PlayerName);
                    double acc = (double)(score.Count300 * 300 + score.Count100 * 100 + score.Count50 * 50 + score.CountMiss * 0) /
                        ((score.Count300 + score.Count100 + score.Count50 + score.CountMiss) * 300) * 100;

                    command.Parameters.AddWithValue("@Acc", Math.Round(acc,2));

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

                    PerfomanceAttributes fcpp = DifficultyAttributes.CalculatePP(beatmap.FolderName, beatmap.FileName, score.ReplayScore,
                       (int)score.Mods, 0, score.Count50, score.Count100, score.Count300, 0, score.Combo, (int)score.Ruleset);
                        
                        command.Parameters.AddWithValue("@fcpp", pp.pp);

                        command.Parameters.AddWithValue("@aim", pp.aim);
                        command.Parameters.AddWithValue("@speed", pp.speed);
                        command.Parameters.AddWithValue("@accuracyatt", pp.accuracy);
                        command.Parameters.AddWithValue("@grade", DifficultyAttributes.CalculateGrade(score.Count300, score.Count100, score.Count50, score.CountMiss));

                    int rows = command.ExecuteNonQuery();
                    Console.WriteLine("rows inserted: " + rows);
                    connection.Close();
                    Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Saved Score: {beatmap.Title}");
                    return true;

                }
            }
        }

        [Time]
        public async static Task<bool> ImportScore(object importscore)
        {
            ImportScore score = (ImportScore)importscore;
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Trying to Insert Score {score.file_md5} {score.pp}");
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

                    foldername = await ApiController.Instance.DownloadBeatmapset(client, score.set_id, true);

                    client.Dispose();

                    Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Trying to Get Filename {score.beatmap_id}");
                    osufilename = Util.osufile(foldername, score.diffname, "TESTFOLDERDELETEAFTER");

                    Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Trying to Find Background for {score.beatmap_id}");
                    background = Util.getBackground(foldername, osufilename);


                    string insertQuery = @"
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



                    //YYYY-MM-DD HH:MM THIS FORMAT IS SUPPOSED TO BE USED 
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Date", score.date_played.ToString("yyyy-MM-dd HH:mm"));
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
                        command.Parameters.AddWithValue("@Username", Credentials.Instance.GetConfig().username);
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
                       
                        //add pp calc here

                        command.Parameters.AddWithValue("@fcpp", 0);
                        command.Parameters.AddWithValue("@aim", 0);
                        command.Parameters.AddWithValue("@speed", 0);
                        command.Parameters.AddWithValue("@accuracyatt", 0);
                        command.Parameters.AddWithValue("@grade", score.rank);

                        command.ExecuteNonQuery();
                    }

                    Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Saved Score: {score.title}");
                    return true;

                }
                catch (Exception e)
                {
                    Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Failed to Import Score({score.file_md5}):{e.Message}");
                    return false;
                    Console.WriteLine(e.ToString());

                }
                finally
                {
                    connection.Close();
                }
            }
        }

        public List<Score> GetPotentcialtopplays(string from, string to, int combopercent, int misscount ,bool pass) { 
            List<Score> scores = new List<Score>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
            
                using (var command = connection.CreateCommand()) {

                    string insertquery = @" SELECT *, rowid AS id 
                                            FROM ScoreData
                                            WHERE Date BETWEEN @from AND @to
                                            AND 
                                             ";
                }   
            }


                return scores;
        }
        
        [Time]
        public async void InsertScore(OsuBaseAddresses baseAddresses, float timeElapsed, string playtype, string replay = null)
        {
            Logger.Log(Logger.Severity.Info, Logger.Framework.Database, $"Inserting Score on: {baseAddresses.Beatmap.OsuFileName}");

            string creator = "null", artist = "null", status = baseAddresses.Beatmap.Status.ToString(), version = "null", tags = "null", coverlist = "null", cover = "null", preview = "null";
            double bpm = -1;
            double starrating = -1;


            PerfomanceAttributes perfomanceAttributes = new PerfomanceAttributes();


            perfomanceAttributes = DifficultyAttributes.CalculatePP(
                                baseAddresses.Beatmap.FolderName,
                                baseAddresses.Beatmap.OsuFileName,
                                baseAddresses.Player.Score,
                                baseAddresses.Player.Mods.Value,
                                baseAddresses.Player.HitMiss,
                                baseAddresses.Player.Hit50,
                                baseAddresses.Player.Hit100,
                                baseAddresses.Player.Hit300,
                                0,
                                baseAddresses.Player.MaxCombo,
                                baseAddresses.Player.Mode);

            //Override Grade incase Play was not passed
            if (playtype == "Cancel" || playtype == "Fail" || playtype == "Retry")
            {
                perfomanceAttributes.grade = "F";
            }

            PerfomanceAttributes fcPerformanceAttributes = DifficultyAttributes.CalculatePP(
                                baseAddresses.Beatmap.FolderName,
                                baseAddresses.Beatmap.OsuFileName,
                                baseAddresses.Player.Score,
                                baseAddresses.Player.Mods.Value,
                                0,  //force no Misses
                                baseAddresses.Player.Hit50,
                                baseAddresses.Player.Hit100,
                                baseAddresses.Player.Hit300,
                                0,
                                0,
                                baseAddresses.Player.Mode);


            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                    Select * from BeatmapHelper WHERE id = @id";
                    command.Parameters.AddWithValue("@id", baseAddresses.Beatmap.Id);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read() && baseAddresses.Beatmap.Id != 0) //prevent Default Id to saved only once!
                        {
                            // Update variables with the fetched data
                            starrating = double.Parse(reader["sr"].ToString());
                            bpm = double.Parse(reader["bpm"].ToString());
                            creator = reader["creator"].ToString();
                            artist = reader["artist"].ToString();
                            status = reader["status"].ToString();
                            coverlist = reader["coverlist"].ToString();
                            cover = reader["cover"].ToString();
                            tags = reader["tags"].ToString();
                            version = reader["version"].ToString();
                            preview = reader["preview"].ToString();

                            Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Beatmap with id: {baseAddresses.Beatmap.Id} exists");

                        }
                        else
                        {
                            JObject beatmap = await ApiController.Instance.getExpandedBeatmapinfo(baseAddresses.Beatmap.Id.ToString());

                            if (beatmap != null)
                            {
                                bpm = double.Parse(beatmap["bpm"].ToString());
                                creator = beatmap["beatmapset"]["creator"].ToString();
                                artist = beatmap["beatmapset"]["artist"].ToString();
                                status = beatmap["beatmapset"]["status"].ToString();
                                coverlist = beatmap["beatmapset"]["covers"]["cover@2x"].ToString();
                                cover = beatmap["beatmapset"]["covers"]["list@2x"].ToString();
                                version = beatmap["version"].ToString();
                                tags = beatmap["beatmapset"]["tags"].ToString();
                                preview = beatmap["beatmapset"]["preview_url"].ToString();
                            }
                            else
                            {
                                // add parsing for beatmap filename! 
                            }

                            if (cover == "null" || cover.EndsWith("?0") || cover.Equals(""))
                            {
                                cover = Util.getBackground(baseAddresses.Beatmap.FolderName, baseAddresses.Beatmap.OsuFileName);
                                coverlist = cover;
                            }

                            using (SQLiteCommand insertCommand = new SQLiteCommand(connection))
                            {

                                insertCommand.CommandText = @"
                                 INSERT INTO BeatmapHelper (id, sr, bpm, creator, artist, status, coverlist, cover, preview, version, tags)
                                 VALUES (@id, @sr, @bpm, @creator, @artist, @status, @coverlist, @cover, @preview , @version, @tags)";

                                insertCommand.Parameters.AddWithValue("@id", baseAddresses.Beatmap.Id);
                                insertCommand.Parameters.AddWithValue("@sr", starrating);
                                insertCommand.Parameters.AddWithValue("@bpm", bpm);
                                insertCommand.Parameters.AddWithValue("@creator", creator);
                                insertCommand.Parameters.AddWithValue("@artist", artist);
                                insertCommand.Parameters.AddWithValue("@status", status);
                                insertCommand.Parameters.AddWithValue("@coverlist", coverlist);
                                insertCommand.Parameters.AddWithValue("@cover", cover);
                                insertCommand.Parameters.AddWithValue("@version", version);
                                insertCommand.Parameters.AddWithValue("@tags", tags);
                                insertCommand.Parameters.AddWithValue("@preview", preview);

                                int rowsInserted = insertCommand.ExecuteNonQuery();

                                if (rowsInserted > 0)
                                {
                                    Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"New BeatmapHelper score with id: {baseAddresses.Beatmap.Id} created.");

                                }

                            }

                        }
                    }
                }
                connection.Close();
            }


            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string insertQuery = @"
                    INSERT INTO ScoreData (
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



                    //YYYY-MM-DD HH:MM THIS FORMAT IS SUPPOSED TO BE USED 
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                        command.Parameters.AddWithValue("@BeatmapSetid", baseAddresses.Beatmap.SetId);
                        command.Parameters.AddWithValue("@Beatmapid", baseAddresses.Beatmap.Id);
                        command.Parameters.AddWithValue("@Osufilename", baseAddresses.Beatmap.OsuFileName);
                        command.Parameters.AddWithValue("@Foldername", baseAddresses.Beatmap.FolderName);
                        command.Parameters.AddWithValue("@Replay", replay);
                        command.Parameters.AddWithValue("@Playtype", playtype);
                        command.Parameters.AddWithValue("@Ar", perfomanceAttributes.ar);
                        command.Parameters.AddWithValue("@Cs", baseAddresses.Beatmap.Cs); //needs mod recalculation
                        command.Parameters.AddWithValue("@Hp", baseAddresses.Beatmap.Hp); //needs mod recalculation
                        command.Parameters.AddWithValue("@Od", perfomanceAttributes.od);
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@SR", perfomanceAttributes.starrating);
                        command.Parameters.AddWithValue("@Bpm", bpm);
                        command.Parameters.AddWithValue("@Creator", creator);
                        command.Parameters.AddWithValue("@Artist", artist);
                        command.Parameters.AddWithValue("@Username", baseAddresses.Player.Username);
                        command.Parameters.AddWithValue("@Acc", Math.Round(baseAddresses.Player.Accuracy,2));
                        command.Parameters.AddWithValue("@MaxCombo", perfomanceAttributes.Maxcombo);
                        command.Parameters.AddWithValue("@Score", baseAddresses.Player.Score);
                        command.Parameters.AddWithValue("@Combo", baseAddresses.Player.MaxCombo);
                        command.Parameters.AddWithValue("@Hit50", baseAddresses.Player.Hit50);
                        command.Parameters.AddWithValue("@Hit100", baseAddresses.Player.Hit100);
                        command.Parameters.AddWithValue("@Hit300", baseAddresses.Player.Hit300);
                        command.Parameters.AddWithValue("@Ur", null);
                        command.Parameters.AddWithValue("@HitMiss", baseAddresses.Player.HitMiss);
                        command.Parameters.AddWithValue("@Mode", baseAddresses.Player.Mode);
                        command.Parameters.AddWithValue("@Mods", baseAddresses.Player.Mods.Value);
                        command.Parameters.AddWithValue("@Version", version);
                        command.Parameters.AddWithValue("@Cover", cover);
                        command.Parameters.AddWithValue("@Coverlist", coverlist);
                        command.Parameters.AddWithValue("@Preview", preview);
                        command.Parameters.AddWithValue("@Tags", tags);
                        command.Parameters.AddWithValue("@Time", timeElapsed);
                        command.Parameters.AddWithValue("@pp", perfomanceAttributes.pp);
                        command.Parameters.AddWithValue("@fcpp", fcPerformanceAttributes.pp);
                        command.Parameters.AddWithValue("@aim", perfomanceAttributes.aim);
                        command.Parameters.AddWithValue("@speed", perfomanceAttributes.speed);
                        command.Parameters.AddWithValue("@accuracyatt", perfomanceAttributes.accuracy);
                        command.Parameters.AddWithValue("@grade", perfomanceAttributes.grade);

                      int rows = command.ExecuteNonQuery();

                        if(rows > 0)
                        {
                            command.CommandText = "select last_insert_rowid()";
                           
                            Score score = new Score();
                            score.Id = int.Parse(command.ExecuteScalar().ToString());
                            score.Status = status;
                            score.Grade = perfomanceAttributes.grade;
                            score.Cover = cover;
                            score.OsuFilename = baseAddresses.Beatmap.OsuFileName;
                            score.ScoreValue = baseAddresses.Player.Score;
                            score.Combo = baseAddresses.Player.MaxCombo;
                            score.MaxCombo = perfomanceAttributes.Maxcombo;
                            score.Version = version;
                            score.Date = DateTime.Now;
                            score.PlayType = playtype;
                            score.ModsString = ModParser.ParseMods(baseAddresses.Player.Mods.Value);
                            score.Acc = Math.Round(baseAddresses.Player.Accuracy, 2);
                            score.Hit300 = baseAddresses.Player.Hit300;
                            score.Hit100 = baseAddresses.Player.Hit100;
                            score.Hit50 = baseAddresses.Player.Hit50;
                            score.HitMiss = baseAddresses.Player.HitMiss;
                            score.PP = perfomanceAttributes.pp;
                            score.FCPP = fcPerformanceAttributes.pp;
                            SSEstream.SendScore(score);
                        }
                    }

                    Logger.Log(Logger.Severity.Debug, Logger.Framework.Database, $"Saved Score: {baseAddresses.Beatmap.OsuFileName}");


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

                    command.CommandText = "SELECT rowid as id, * " +
                        "FROM ScoreData " +
                        "WHERE datetime(Date) BETWEEN @from AND @to " +
                        "ORDER BY Date DESC " +
                        "LIMIT 100;";
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
        public List<Score> GetScoreSearch(DateTime from = new DateTime(),
            DateTime to = new DateTime(),
            string search = "")
        {

            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");

            List<Score> scores = new();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {

                using (var command = new SQLiteCommand(connection))
                {

                    connection.Open();

                    StringBuilder queryBuilder = new StringBuilder("SELECT rowid as id, * FROM ScoreData WHERE 1=1 ");


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

                string commandstring = @"
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

                string sqlQuery = @"
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

                sqlQuery = @"SELECT SUM(Time) AS TotalTimeWasted, RawStatus
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
                            week.Screen = DifficultyAttributes.ScreenConverter(int.Parse(reader["RawStatus"].ToString()));
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
                    command.CommandText = "SELECT * FROM ScoreData WHERE datetime(Date) BETWEEN @from AND @to ;";

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
            List<KeyValuePair<string, double>> BanchoTime = new List<KeyValuePair<string, double>>();
            int rows = 0;

            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {

                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT BanchoStatus, SUM(Time) as Time FROM BanchoTime WHERE datetime(Date) BETWEEN @from AND @to Group by BanchoStatus;";

                    command.Parameters.AddWithValue("@from", fromFormatted);
                    command.Parameters.AddWithValue("@to", toFormatted);

                    //Console.WriteLine(rows.ToString());

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            BanchoTime.Add(new KeyValuePair<string, double>(reader["BanchoStatus"].ToString(), Convert.ToDouble(reader["Time"])));
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
                    command.CommandText = @"
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
                            var entry = result.FirstOrDefault(dict => dict.ContainsKey("Date") && ((DateTime)dict["Date"]).Date == date.Date);

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
                                    entry[banchoStatus] = Convert.ToDouble(entry[banchoStatus]) + totalTime;
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
            List<KeyValuePair<string, double>> BanchoTime = new List<KeyValuePair<string, double>>();
            int rows = 0;

            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {

                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT strftime('%Y-%m-%d', Date) as Day, RawStatus, SUM(Time) as Time FROM TimeWasted Group by RawStatus";

                    command.Parameters.AddWithValue("@from", fromFormatted);
                    command.Parameters.AddWithValue("@to", toFormatted);

                    //Console.WriteLine(rows.ToString());

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            BanchoTime.Add(new KeyValuePair<string, double>(reader["RawStatus"].ToString(), Convert.ToDouble(reader["Time"])));
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
                    command.CommandText = @"
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
                            var entry = result.FirstOrDefault(dict => dict.ContainsKey("Date") && ((DateTime)dict["Date"]).Date == date.Date);

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
                                    entry[banchoStatus] = Convert.ToDouble(entry[banchoStatus]) + totalTime;
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

    }

    public class WeekCompare
    {
        public string Status { get; set; }
        public string Screen { get; set; }
        public double LastWeek { get; set; }
        public double ThisWeek { get; set; }
    }
}


