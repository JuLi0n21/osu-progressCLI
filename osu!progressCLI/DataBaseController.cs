using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using OsuMemoryDataProvider.OsuMemoryModels;
using System.ComponentModel.Design;
using static System.Formats.Asn1.AsnWriter;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Net.Http.Headers;
using osu_progressCLI;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Text.RegularExpressions;


//Add proper debug messages and levels...
//make it possible to retrive data 1. prepared stuff like, betweewn dates and or costuem request as a bonus maybe...
//refactor sometime to make it proper data types instead of text, maybe....
namespace osu1progressbar.Game.Database
{
    public class DatabaseController
    {
        private readonly string dbname = null;
        private readonly string connectionString = "Data Source=osu!progress.db;Version=3;";
        ApiController apiController;
        public DatabaseController()
        {
            //  configJson = File.ReadAllText(configJson);
            // JObject config = JObject.Parse(configJson);
            apiController = new ApiController();
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

            //Hiterros should be recalculated into UR
            string createScoreTableQuery = @"
                CREATE TABLE IF NOT EXISTS ScoreData (
                    Date TEXT,
                    BeatmapSetid TEXT,
                    Beatmapid TEXT,
                    Osufilename TEXT,
                    Ar TEXT,
                    Cs TEXT,
                    Hp TEXT,
                    Od TEXT,
                    Status TEXT,
                    StarRating TEXT,
                    Bpm TEXT,
                    Artist TEXT,
                    Creator TEXT,
                    Username TEXT,
                    Accuracy TEXT,
                    MaxCombo TEXT,
                    Score TEXT,
                    Combo TEXT,
                    Hit50 TEXT,
                    Hit100 TEXT,
                    Hit300 TEXT,
                    Ur TEXT,
                    HitMiss TEXT,
                    Mode TEXT,
                    Mods TEXT
                );
            ";

            using (var command = new SQLiteCommand(createScoreTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            Console.WriteLine("Database Created: " + dbname);

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
                CREATE TABLE IF NOT EXISTS BeatmapHelper (id TEXT, StarRating TEXT, Artist TEXT, Creator TEXT, Bpm TEXT)";

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

            Console.WriteLine(date);

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

            Console.WriteLine(date);

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
        public async void InsertScore(OsuBaseAddresses baseAddresses)
        {

            string starrating = "null", bpm = "null", creator = "null", artist = "null";

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
                            starrating = reader["starrating"].ToString();
                            bpm = reader["bpm"].ToString();
                            creator = reader["creator"].ToString();
                            artist = reader["artist"].ToString();

                            Console.WriteLine($"Beatmap with id: {baseAddresses.Beatmap.Id} exists");
                        }
                        else
                        {
                            JObject beatmap = await apiController.getExpandedBeatmapinfo(baseAddresses.Beatmap.Id.ToString());
                            if (beatmap != null)
                            {
                                starrating = beatmap["difficulty_rating"].ToString();
                                bpm = beatmap["bpm"].ToString();
                                creator = beatmap["beatmapset"]["creator"].ToString();
                                artist = beatmap["beatmapset"]["artist"].ToString();
                            }

                            // Create a new BeatmapHelper score with "null" values for everything except ID
                            using (SQLiteCommand insertCommand = new SQLiteCommand(connection))
                            {
                                insertCommand.CommandText = @"
                        INSERT INTO BeatmapHelper (id, starrating, bpm, creator, artist)
                        VALUES (@id, @starrating, @bpm, @creator, @artist)";

                                insertCommand.Parameters.AddWithValue("@id", baseAddresses.Beatmap.Id);
                                insertCommand.Parameters.AddWithValue("@starrating", starrating);
                                insertCommand.Parameters.AddWithValue("@bpm", bpm);
                                insertCommand.Parameters.AddWithValue("@creator", creator);
                                insertCommand.Parameters.AddWithValue("@artist", artist);

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
                    Mods
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
                            @Mods
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
                        command.Parameters.AddWithValue("@Status", baseAddresses.Beatmap.Status);
                        command.Parameters.AddWithValue("@StarRating", starrating);
                        command.Parameters.AddWithValue("@Bpm", bpm);
                        command.Parameters.AddWithValue("@Creator", creator);
                        command.Parameters.AddWithValue("@Artist", artist);
                        command.Parameters.AddWithValue("@Username", baseAddresses.Player.Username);
                        command.Parameters.AddWithValue("@Accuracy", baseAddresses.Player.Accuracy);
                        command.Parameters.AddWithValue("@MaxCombo", baseAddresses.Player.MaxCombo);
                        command.Parameters.AddWithValue("@Score", baseAddresses.Player.Score);
                        command.Parameters.AddWithValue("@Combo", baseAddresses.Player.Combo);
                        command.Parameters.AddWithValue("@Hit50", baseAddresses.Player.Hit50);
                        command.Parameters.AddWithValue("@Hit100", baseAddresses.Player.Hit100);
                        command.Parameters.AddWithValue("@Hit300", baseAddresses.Player.Hit300);
                        command.Parameters.AddWithValue("@Ur", ur.ToString()); //this is a lie 
                        command.Parameters.AddWithValue("@HitMiss", baseAddresses.Player.HitMiss);
                        command.Parameters.AddWithValue("@Mode", baseAddresses.Player.Mode);
                        command.Parameters.AddWithValue("@Mods", baseAddresses.Player.Mods.Value);


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

        public List<List<Dictionary<string, object>>> GetScoresInTimeSpan(DateTime from, DateTime to)
        {
            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");
            List<List<Dictionary<string, object>>> scores = new List<List<Dictionary<string, object>>>();

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {

                using (var command = new SQLiteCommand(connection))
                {

                    connection.Open();
                    //command.CommandText = "SELECT Date FROM TimeWasted";
                    //command.CommandText = "SELECT  datetime(Date, '%Y-%m-%d %H:%M') AS Date FROM TimeWasted ";
                    //command.CommandText = " SELECT datetime(Date) AS FormattedDate FROM TimeWasted";

                    command.CommandText = "SELECT * FROM ScoreData WHERE datetime(Date) BETWEEN @from AND @to;";
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
                            List<Dictionary<string, object>> row = new List<Dictionary<string, object>>();
                            //Turn it back into a beatmap
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object columnValue = reader.GetValue(i);

                                // Create a dictionary entry for each column
                                Dictionary<string, object> columnEntry = new Dictionary<string, object>
                                {
                                    { columnName, columnValue }
                                };

                                row.Add(columnEntry);

                            }
                            scores.Add(row);
                        }

                    }


                    connection.Close();

                }
                return scores;
            }
        }

        public int GetScoreAmounts(DateTime from, DateTime to)
        {
            //change or not use it, waste ...
            int rows = -1;
            string fromFormatted = from.ToString("yyyy-MM-dd HH:mm:ss");
            string toFormatted = to.ToString("yyyy-MM-dd HH:mm:ss");

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {

                connection.Open();

                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = "SELECT * FROM ScoreData WHERE datetime(Date) BETWEEN @from AND @to;";

                    command.Parameters.AddWithValue("@from", fromFormatted);
                    command.Parameters.AddWithValue("@to", toFormatted);

                    rows = command.ExecuteNonQuery();

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {

                        //change to datatype maybe?

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
                    //SELECT BanchoStatus, SUM(Time) FROM Group by BanchoStatus
                    command.CommandText = "SELECT BanchoStatus, SUM(Time) as Time FROM BanchoTime WHERE datetime(Date) BETWEEN @from AND @to Group by BanchoStatus";

                    command.Parameters.AddWithValue("@from", fromFormatted);
                    command.Parameters.AddWithValue("@to", toFormatted);

                    //rows = command.ExecuteNonQuery();

                    Console.WriteLine(rows.ToString());

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {

                        //change to datatype maybe?




                        while (reader.Read())
                        { 
                            //Console.WriteLine(reader["BanchoStatus"].ToString(), Convert.ToString(reader["Time"]));
                            BanchoTime.Add(new KeyValuePair<string, double>(reader["BanchoStatus"].ToString(), Convert.ToDouble(reader["Time"])));
                        }
                    }
                }
            }
            return BanchoTime;
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
                    //SELECT BanchoStatus, SUM(Time) FROM Group by BanchoStatus
                    command.CommandText = "SELECT RawStatus, SUM(Time) as Time FROM TimeWasted WHERE datetime(Date) BETWEEN @from AND @to Group by RawStatus";

                    command.Parameters.AddWithValue("@from", fromFormatted);
                    command.Parameters.AddWithValue("@to", toFormatted);

                    //rows = command.ExecuteNonQuery();

                    Console.WriteLine(rows.ToString());

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {

                        //change to datatype maybe?




                        while (reader.Read())
                        {
                            //Console.WriteLine(reader["BanchoStatus"].ToString(), Convert.ToString(reader["Time"]));
                            BanchoTime.Add(new KeyValuePair<string, double>(reader["RawStatus"].ToString(), Convert.ToDouble(reader["Time"])));
                        }
                    }
                }
            }
            return BanchoTime;
        }
    }
}


