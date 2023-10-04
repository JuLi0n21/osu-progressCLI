using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using OsuMemoryDataProvider.OsuMemoryModels;
using Newtonsoft.Json.Linq;
using osu_progressCLI;


//Add proper debug messages and levels...
//make it possible to retrive data 1. prepared stuff like, betweewn dates and or costuem request as a bonus maybe...
//refactor sometime to make it proper data types instead of text, maybe....
//FILTER FOREVERY THING THAT IS SAVED

//importer for funoranges tracker
namespace osu1progressbar.Game.Database
{
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
                    Console.WriteLine(e.ToString());
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
                    Ar REAL,
                    Cs REAL,
                    Hp REAL,
                    Od REAL,
                    Status TEXT,
                    StarRating REAL,
                    Bpm REAL,
                    Artist TEXT,
                    Creator TEXT,
                    Username TEXT,
                    Accuracy REAL,
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
                    Time INTEGER,
                    PP REAL,
                    AIM REAL,
                    SPEED REAL,
                    ACCURACYATT REAL,
                    Grade TEXT,
                    FCPP REAL
                );
            ";

            using (var command = new SQLiteCommand(createScoreTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            Console.WriteLine("Database Created if it doesnt exist: " + dbname);

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
                CREATE TABLE IF NOT EXISTS BeatmapHelper (id INTEGER, StarRating REAL, Artist TEXT, Creator TEXT, Bpm REAL, Version TEXT, Status TEXT, Tags TEXT, CoverList TEXT, Cover TEXT)";

            using (var command = new SQLiteCommand(ScoreHelperTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            string query = "SELECT name FROM sqlite_master WHERE type='table';";
            using (var command = new SQLiteCommand(query, connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    Console.WriteLine(tableName);
                }
            }

        }

        public void UpdateTimeWasted(int OldStatus, float timeElapsed)
        {
            DateTime time = DateTime.UtcNow;
            string date = time.ToString("yyyy-MM-dd HH:00");

            if(OldStatus == -1)
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
                            Console.WriteLine("New TimeWasted Added for this hour");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Updated TimeWasted in: " + OldStatus + " time: " + timeElapsed);
                    }
                }

                connection.Close();
            }
        }

        public void UpdateBanchoTime(string BanchoStatus, float timeElapsed)
        {
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
                            Console.WriteLine("New BanchoTime Added for this hour");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Updated BanchoTime in: " + BanchoStatus + " time: " + timeElapsed);
                    }
                }

                connection.Close();
            }
        }

        //maybe consider passed or failed/canceld retires// more beatmap attributes
        public async void InsertScore(OsuBaseAddresses baseAddresses, float timeElapsed)
        {

            string creator = "null", artist = "null", status = baseAddresses.Beatmap.Status.ToString(), version = "null", tags = "null", coverlist = "null", cover = "null";
            double bpm = -1;
            double starrating = -1;


            PerfomanceAttributes perfomanceAttributes = new PerfomanceAttributes();


            perfomanceAttributes = DifficultyAttributes.CalculatePP(
                                baseAddresses.Beatmap.FolderName,
                                baseAddresses.Beatmap.OsuFileName,
                                baseAddresses.Player.Mods.Value,
                                baseAddresses.Player.HitMiss,
                                baseAddresses.Player.Hit50,
                                baseAddresses.Player.Hit100,
                                baseAddresses.Player.Hit300,
                                baseAddresses.Player.MaxCombo);

            if(baseAddresses.Player.HP == 0)
            {
                perfomanceAttributes.grade = "F";
            }

            starrating = perfomanceAttributes.starrating;
            Console.WriteLine(perfomanceAttributes.Maxcombo);

            double fcpp = DifficultyAttributes.CalculateFcWithAcc(
                                baseAddresses.Beatmap.FolderName,
                                baseAddresses.Beatmap.OsuFileName,
                                baseAddresses.Player.Accuracy,
                                baseAddresses.Player.Mods.Value);

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
                        if (reader.Read())
                        {
                            // Update variables with the fetched data
                            starrating = double.Parse(reader["starrating"].ToString());
                            bpm = double.Parse(reader["bpm"].ToString());
                            creator = reader["creator"].ToString();
                            artist = reader["artist"].ToString();
                            status = reader["status"].ToString();
                            coverlist = reader["coverlist"].ToString();
                            cover = reader["cover"].ToString();
                            tags = reader["tags"].ToString();
                            version = reader["version"].ToString();

                            Console.WriteLine($"Beatmap with id: {baseAddresses.Beatmap.Id} exists");
                        }
                        else
                        {
                            JObject beatmap = await ApiController.Instance.getExpandedBeatmapinfo(baseAddresses.Beatmap.Id.ToString());
                            //Console.WriteLine(beatmap);
                            if (beatmap != null)
                            {
                                starrating = double.Parse(beatmap["difficulty_rating"].ToString());
                                bpm = double.Parse(beatmap["bpm"].ToString());
                                creator = beatmap["beatmapset"]["creator"].ToString();
                                artist = beatmap["beatmapset"]["artist"].ToString();
                                status = beatmap["beatmapset"]["status"].ToString();
                                Console.WriteLine(status);
                                coverlist = beatmap["beatmapset"]["covers"]["cover@2x"].ToString();
                                cover = beatmap["beatmapset"]["covers"]["list@2x"].ToString();
                                version = beatmap["version"].ToString();
                                tags = beatmap["beatmapset"]["tags"].ToString();
                            }   

                            // Create a new BeatmapHelper score with "null" values for everything except ID
                            using (SQLiteCommand insertCommand = new SQLiteCommand(connection))
                            {

                                insertCommand.CommandText = @"
                                 INSERT INTO BeatmapHelper (id, starrating, bpm, creator, artist, status, coverlist, cover, version, tags)
                                 VALUES (@id, @starrating, @bpm, @creator, @artist, @status, @coverlist, @cover, @version, @tags)";

                                // Providing the beatmap's attributes
                                insertCommand.Parameters.AddWithValue("@id", baseAddresses.Beatmap.Id);
                                insertCommand.Parameters.AddWithValue("@starrating", starrating);
                                insertCommand.Parameters.AddWithValue("@bpm", bpm);
                                insertCommand.Parameters.AddWithValue("@creator", creator);
                                insertCommand.Parameters.AddWithValue("@artist", artist);
                                insertCommand.Parameters.AddWithValue("@status", status);
                                insertCommand.Parameters.AddWithValue("@coverlist", coverlist);
                                insertCommand.Parameters.AddWithValue("@cover", cover);
                                insertCommand.Parameters.AddWithValue("@version", version);
                                insertCommand.Parameters.AddWithValue("@tags", tags);

                                int rowsInserted = insertCommand.ExecuteNonQuery();

                                if (rowsInserted > 0)
                                {
                                    Console.WriteLine($"New BeatmapHelper score with id: {baseAddresses.Beatmap.Id} created.");
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
                    Ar,
                    Cs,
                    Hp,
                    Od,
                    Status,
                    StarRating,
                    Bpm,
                    Creator,
                    Artist,
                    Username,
                    Accuracy,
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
                            @Ar,
                            @Cs,
                            @Hp,
                            @Od,
                            @Status,
                            @StarRating,
                            @Bpm,
                            @Creator,
                            @Artist,
                            @Username,
                            @Accuracy,
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
                            @Time,
                            @pp,
                            @fcpp,
                            @aim,
                            @speed,
                            @accuracyatt,
                            @grade
                        );
                    ";

                    DateTime dateTime = DateTime.Now;
                    float ur = 0; // calculate ur here.
                    int urcount = 0;

                    baseAddresses.Player.HitErrors.ForEach(error =>
                    {
                        ur += error;
                        urcount++;
                    });

                    //Console.WriteLine(((ur / urcount) * 100).ToString());

                    //YYYY-MM-DD HH:MM THIS FORMAT IS SUPPOSED TO BE USED 
                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Date", dateTime.ToString("yyyy-MM-dd HH:mm"));
                        command.Parameters.AddWithValue("@BeatmapSetid", baseAddresses.Beatmap.SetId);
                        command.Parameters.AddWithValue("@Beatmapid", baseAddresses.Beatmap.Id);
                        command.Parameters.AddWithValue("@Osufilename", baseAddresses.Beatmap.OsuFileName);
                        command.Parameters.AddWithValue("@Ar", baseAddresses.Beatmap.Ar);
                        command.Parameters.AddWithValue("@Cs", baseAddresses.Beatmap.Cs);
                        command.Parameters.AddWithValue("@Hp", baseAddresses.Beatmap.Hp);
                        command.Parameters.AddWithValue("@Od", baseAddresses.Beatmap.Od);
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@StarRating", starrating);
                        command.Parameters.AddWithValue("@Bpm", bpm);
                        command.Parameters.AddWithValue("@Creator", creator);
                        command.Parameters.AddWithValue("@Artist", artist);
                        command.Parameters.AddWithValue("@Username", baseAddresses.Player.Username);
                        command.Parameters.AddWithValue("@Accuracy", baseAddresses.Player.Accuracy);
                        command.Parameters.AddWithValue("@MaxCombo", perfomanceAttributes.Maxcombo);
                        command.Parameters.AddWithValue("@Score", baseAddresses.Player.Score);
                        command.Parameters.AddWithValue("@Combo", baseAddresses.Player.MaxCombo);
                        command.Parameters.AddWithValue("@Hit50", baseAddresses.Player.Hit50);
                        command.Parameters.AddWithValue("@Hit100", baseAddresses.Player.Hit100);
                        command.Parameters.AddWithValue("@Hit300", baseAddresses.Player.Hit300);
                        command.Parameters.AddWithValue("@Ur", ur.ToString()); 
                        command.Parameters.AddWithValue("@HitMiss", baseAddresses.Player.HitMiss);
                        command.Parameters.AddWithValue("@Mode", baseAddresses.Player.Mode);
                        command.Parameters.AddWithValue("@Mods", baseAddresses.Player.Mods.Value);
                        command.Parameters.AddWithValue("@Version", version);
                        command.Parameters.AddWithValue("@Cover", cover);
                        command.Parameters.AddWithValue("@Coverlist", coverlist);
                        command.Parameters.AddWithValue("@Tags", tags);
                        command.Parameters.AddWithValue("@Time", timeElapsed);
                        command.Parameters.AddWithValue("@pp", perfomanceAttributes.pp);
                        command.Parameters.AddWithValue("@fcpp", fcpp);
                        command.Parameters.AddWithValue("@aim", perfomanceAttributes.aim);
                        command.Parameters.AddWithValue("@speed", perfomanceAttributes.speed);
                        command.Parameters.AddWithValue("@accuracyatt", perfomanceAttributes.accuracy);
                        command.Parameters.AddWithValue("@grade", perfomanceAttributes.grade);


                        command.ExecuteNonQuery();
                    }

                    Console.WriteLine($"Saved Score: {baseAddresses.Beatmap.OsuFileName}");

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());

                }
                finally
                {
                    connection.Close();
                }
            }

        }

        public List<Dictionary<string, object>> GetScoresInTimeSpan(DateTime from, DateTime to)
        {
            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");

            //List<List<Dictionary<string, object>>> scores = new List<List<Dictionary<string, object>>>();
            List<Dictionary<string, object>> scores = new List<Dictionary<string, object>>();
            
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {

                using (var command = new SQLiteCommand(connection))
                {

                    connection.Open();
                    //command.CommandText = "SELECT Date FROM TimeWasted";
                    //command.CommandText = "SELECT  datetime(Date, '%Y-%m-%d %H:%M') AS Date FROM TimeWasted ";
                    //command.CommandText = " SELECT datetime(Date) AS FormattedDate FROM TimeWasted";
                    command.CommandText = "SELECT * " +
                        "FROM ScoreData " +
                        "WHERE datetime(Date) BETWEEN @from AND @to " +
                        "ORDER BY Date DESC " +
                        "LIMIT 1000;";
                    //command.CommandText = "SELECT Date FROM ScoreData";
                    //command.CommandText = "SELECT strftime('%Y-%m-%d-%H-',Date) AS parsedDate, RawStatus, Time  FROM TimeWasted;";
                    //Console.WriteLine(fromFormatted + " " + toFormatted);

                    command.Parameters.AddWithValue("@from", fromFormatted);
                    command.Parameters.AddWithValue("@to", toFormatted);

                    DateTime dateString = DateTime.Now;

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        //Console.WriteLine(reader.HasRows.ToString

                        while (reader.Read())
                        {
                            //List<Dictionary<string, object>> row = new List<Dictionary<string, object>>();
                            //Turn it back into a beatmap
                            Dictionary<string, object> score = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object columnValue = reader.GetValue(i);
                                // Create a dictionary entry for each 
                                 score.Add(columnName, columnValue);

                            }
                            scores.Add(score);
                        }
                    }
                    connection.Close();
                }
                return scores;
            }
        }

        //ar(0-12) od(0-12) cs(0-12) sr(0-XX) bpm(0-XXX) pp(0-XXXX) hp(0-12  grade(A, S, SS...) time(seconds) mods(hd, dt, nc, ...) status(0-4)
        public List<Dictionary<string, object>> GetScores1(DateTime from = new DateTime(), 
            DateTime to = new DateTime(), 
            string search = "")
        {

            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");

            //List<List<Dictionary<string, object>>> scores = new List<List<Dictionary<string, object>>>();
            List<Dictionary<string, object>> scores = new List<Dictionary<string, object>>();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {

                using (var command = new SQLiteCommand(connection))
                {

                    connection.Open();

                    StringBuilder queryBuilder = new StringBuilder("SELECT * FROM ScoreData WHERE 1=1 ");
                    

                    if (from != null || to != null) {
                        queryBuilder.Append("AND datetime(Date) BETWEEN @from AND @to ");
                        queryBuilder.Append(search);
                        queryBuilder.Append("ORDER BY Date DESC LIMIT 1000;");
                        command.Parameters.AddWithValue("@from", fromFormatted);
                        command.Parameters.AddWithValue("@to", toFormatted);
                    }

                
                    command.CommandText = queryBuilder.ToString();
                    DateTime dateString = DateTime.Now;

                    Console.WriteLine(queryBuilder.ToString());

                    try
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            //Console.WriteLine(reader.HasRows.ToString

                            while (reader.Read())
                            {
                                //List<Dictionary<string, object>> row = new List<Dictionary<string, object>>();
                                //Turn it back into a beatmap
                                Dictionary<string, object> score = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string columnName = reader.GetName(i);
                                    object columnValue = reader.GetValue(i);

                                    score.Add(columnName, columnValue);

                                }
                                scores.Add(score);
                            }
                        }

                    }
                    catch {
                        return null;
                    }
                    finally {
                        connection.Close();
                    }
                    
                }
                return scores;
            }
        }

        public List<Dictionary<string, object>> GetScoreAveragesbyDay(DateTime from, DateTime to) {

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
                AVG(StarRating) AS AverageSR,
                AVG(Accuracy) AS AverageAccuracy,
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

        public int GetScoreAmounts(DateTime from, DateTime to)
        {
            int rows = -1;
            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
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

                    Console.WriteLine(rows.ToString());

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

                    Console.WriteLine(rows.ToString());

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
}


