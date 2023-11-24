using System.Data.SQLite;

namespace osu_progressCLI.Datatypes
{
    public class Score
    {
        public int Id { get; set; }  
        public DateTime Date { get; set; }
        public int BeatmapSetId { get; set; }
        public int BeatmapId { get; set; }
        public string OsuFilename { get; set; }
        public string FolderName { get; set; }
        public string Replay { get; set; }
        public string PlayType { get; set; }
        public float Ar { get; set; }
        public float Cs { get; set; }
        public float Hp { get; set; }
        public float Od { get; set; }
        public string Status { get; set; }
        public double SR { get; set; }
        public double Bpm { get; set; }
        public string Artist { get; set; }
        public string Creator { get; set; }
        public string Username { get; set; }
        public double Acc { get; set; }
        public int score { get; set; } 
        public int MaxCombo { get; set; }
        public int ScoreValue { get; set; }
        public int Combo { get; set; }
        public int Hit50 { get; set; }
        public int Hit100 { get; set; }
        public int Hit300 { get; set; }
        public double Ur { get; set; }
        public int HitMiss { get; set; }
        public string Mode { get; set; }
        public int Mods { get; set; }
        public string ModsString { get; set; }
        public string Version { get; set; }
        public string Tags { get; set; }
        public string CoverList { get; set; }
        public string Cover { get; set; }
        public string Preview { get; set; }
        public int Time { get; set; }
        public double PP { get; set; }
        public double AIM { get; set; }
        public double SPEED { get; set; }
        public double ACCURACYATT { get; set; }
        public string Grade { get; set; }
        public double FCPP { get; set; }

        public Score() {
        }

        public Score(SQLiteDataReader reader)
        {
            Id = Convert.ToInt32(reader["id"]);
            Date = Convert.ToDateTime(reader["Date"]);
            BeatmapSetId = Convert.ToInt32(reader["BeatmapSetid"]);
            BeatmapId = Convert.ToInt32(reader["Beatmapid"]);
            OsuFilename = reader["Osufilename"].ToString();
            FolderName = reader["Foldername"].ToString();
            Replay = reader["Replay"].ToString();
            PlayType = reader["Playtype"].ToString();
            Ar = Convert.ToSingle(reader["Ar"]);
            Cs = Convert.ToSingle(reader["Cs"]);
            Hp = Convert.ToSingle(reader["Hp"]);
            Od = Convert.ToSingle(reader["Od"]);
            Status = reader["Status"].ToString();
            SR = Convert.ToDouble(reader["SR"]);
            Bpm = Convert.ToDouble(reader["Bpm"]);
            Artist = reader["Artist"].ToString();
            Creator = reader["Creator"].ToString();
            Username = reader["Username"].ToString();
            Acc = Convert.ToDouble(reader["Acc"]);
            MaxCombo = Convert.ToInt32(reader["MaxCombo"]);
            ScoreValue = Convert.ToInt32(reader["Score"]);
            Combo = Convert.ToInt32(reader["Combo"]);
            Hit50 = Convert.ToInt32(reader["Hit50"]);
            Hit100 = Convert.ToInt32(reader["Hit100"]);
            Hit300 = Convert.ToInt32(reader["Hit300"]);

            //Ur = Convert.ToDouble(reader["Ur"]);
            Ur = -1;
            HitMiss = Convert.ToInt32(reader["HitMiss"]);
            Mode = reader["Mode"].ToString();
            Mods = Convert.ToInt32(reader["Mods"]);
            ModsString = ModParser.ParseMods(Convert.ToInt32(reader["Mods"].ToString()));
            Version = reader["Version"].ToString();
            Tags = reader["Tags"].ToString();
            CoverList = reader["CoverList"].ToString();
            Cover = reader["Cover"].ToString();
            Preview = reader["Preview"].ToString();
            Time = Convert.ToInt32(reader["Time"]);
            PP = Convert.ToDouble(reader["PP"]);
            AIM = Convert.ToDouble(reader["AIM"]);
            SPEED = Convert.ToDouble(reader["SPEED"]);
            ACCURACYATT = Convert.ToDouble(reader["ACCURACYATT"]);
            Grade = reader["Grade"].ToString();
            FCPP = Convert.ToDouble(reader["FCPP"]);
        }
    }
}
