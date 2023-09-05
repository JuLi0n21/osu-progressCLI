using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu1progressbar.Game.Database;
using osu1progressbar.Game.MemoryProvider;
using OsuMemoryDataProvider.OsuMemoryModels;


namespace osu1progressbar.Game.Logicstuff
{
    public class LogicController
    {
        private DatabaseController db;

        private string CurrentScreen = null;
        private int oldRawStatus = -1;
        private string BanchoUserStatus = null;
        private int Audiotime;

        private Stopwatch screenTimeStopWatch;
        private Stopwatch userTimeStopWatch;
        private Stopwatch stopwatch;
        private Stopwatch timeSinceStartedPlaying;
        public LogicController()
        {
            db = new DatabaseController();
            db.Init();

            screenTimeStopWatch = new Stopwatch();
            screenTimeStopWatch.Start();

            userTimeStopWatch = new Stopwatch();
            userTimeStopWatch.Start();

            stopwatch = Stopwatch.StartNew();

            timeSinceStartedPlaying = new Stopwatch();
        }

        public bool Logiccheck(OsuBaseAddresses NewValues)
        {
            try
            {
               
                string oldvalue = NewValues.ToString();

                if (CurrentScreen == "SongSelect" && NewValues.GeneralData.OsuStatus.ToString() == "Playing") { 
                    timeSinceStartedPlaying = Stopwatch.StartNew();

                }

                if (CurrentScreen == "Playing" && NewValues.GeneralData.OsuStatus.ToString() == "SongSelect")
                {
                    Console.WriteLine("Song Failed detected");
                    db.InsertScore(NewValues);
                    timeSinceStartedPlaying.Reset();
                }

                if (CurrentScreen == "Playing" && NewValues.GeneralData.OsuStatus.ToString() == "ResultsScreen")
                {
                    Console.WriteLine("Song Passed detected");
                    db.InsertScore(NewValues);
                    timeSinceStartedPlaying.Reset();
                }

                if (CurrentScreen != NewValues.GeneralData.OsuStatus.ToString())
                { 
                    Console.WriteLine("Spend: " + (DateTime.Now + ": " + screenTimeStopWatch.ElapsedMilliseconds) + "ms in: " + CurrentScreen);
                    db.UpdateTimeWasted(oldRawStatus, screenTimeStopWatch.ElapsedMilliseconds);
                    screenTimeStopWatch.Restart();
                }

                if (BanchoUserStatus != NewValues.BanchoUser.BanchoStatus.ToString())
                {
                    Console.WriteLine("Spend: " + (DateTime.Now + ": " + screenTimeStopWatch.ElapsedMilliseconds) + "ms as: " + BanchoUserStatus);
                    db.UpdateBanchoTime(BanchoUserStatus, screenTimeStopWatch.ElapsedMilliseconds);
                    userTimeStopWatch.Restart();
                }

                if ((NewValues.GeneralData.OsuStatus.ToString() == "Playing") && (Audiotime > NewValues.GeneralData.AudioTime) && (timeSinceStartedPlaying.ElapsedMilliseconds > 500) && (NewValues.Player.Hit300 > 0))  {
                    Console.WriteLine("Retry detected");
                    db.InsertScore(NewValues);
                    timeSinceStartedPlaying.Restart();
                    };
                 
                CurrentScreen = NewValues.GeneralData.OsuStatus.ToString();
                BanchoUserStatus = NewValues.BanchoUser.BanchoStatus.ToString();
                oldRawStatus = NewValues.GeneralData.RawStatus;
                Audiotime = NewValues.GeneralData.AudioTime;

                if (stopwatch.ElapsedMilliseconds > 10000)
                {
                    DateTime from = DateTime.Now;
                    from = from.Subtract(TimeSpan.FromDays(2));
                    DateTime to = DateTime.Now;
                    db.GetScores(from, to).ForEach(score =>
                    {
                       // Console.WriteLine(score.ToString());
                    });

                    //Console.Write(db.GetScores(from, to).Count());
                    stopwatch.Restart();
                }

                return true;
            }
           
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
