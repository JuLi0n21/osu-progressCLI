using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu1progressbar.Game.Database;
using osu1progressbar.Game.MemoryProvider;
using OsuMemoryDataProvider.OsuMemoryModels;


//detect score fails 
//change saved time to seconds

namespace osu1progressbar.Game.Logicstuff
{
    public class LogicController
    {
        private DatabaseController db;

        private string CurrentScreen = null;
        private int oldRawStatus = -1;
        private string BanchoUserStatus = null;
        private int Audiotime;
        private bool isReplay = false;

        public Stopwatch screenTimeStopWatch;
        public Stopwatch BanchoTimeStopWatch;
        private Stopwatch stopwatch;
        public Stopwatch timeSinceStartedPlaying;

        public int startime;
        public LogicController()
        {
            db = new DatabaseController();
            db.Init();

            screenTimeStopWatch = new Stopwatch();
            screenTimeStopWatch.Start();

            BanchoTimeStopWatch = new Stopwatch();
            BanchoTimeStopWatch.Start();

            stopwatch = Stopwatch.StartNew();

        }

        //the first time gets currentliy ignored cause it gets restarted after first entry (and is probably not running cause its set to stop in the memoryprovider incase no osu is found
        public bool Logiccheck(OsuBaseAddresses NewValues)
        {
            try
            {
               
                string oldvalue = NewValues.ToString();

                if (CurrentScreen != "Playing" && NewValues.GeneralData.OsuStatus.ToString() == "Playing") { 
                    timeSinceStartedPlaying = Stopwatch.StartNew();
                    startime = NewValues.GeneralData.AudioTime;
                }

                if (CurrentScreen != NewValues.GeneralData.OsuStatus.ToString())
                {
                    Console.WriteLine(DateTime.Now + "| " + "Screentime: "  + screenTimeStopWatch.ElapsedMilliseconds / 1000 + "s : " + CurrentScreen);
                    db.UpdateTimeWasted(oldRawStatus, screenTimeStopWatch.ElapsedMilliseconds / 1000);
                    screenTimeStopWatch.Restart();
                }

                if (BanchoUserStatus != NewValues.BanchoUser.BanchoStatus.ToString())
                {
                    Console.WriteLine(DateTime.Now + "| " + "Banchotime: "+ BanchoTimeStopWatch.ElapsedMilliseconds / 1000 + "s : " + BanchoUserStatus);
                    db.UpdateBanchoTime(BanchoUserStatus, BanchoTimeStopWatch.ElapsedMilliseconds / 1000);
                    BanchoTimeStopWatch.Restart();
                }


                if (!isReplay && CurrentScreen == "Playing" && NewValues.GeneralData.OsuStatus.ToString() != "Playing")
                {
                    
                    Console.WriteLine("Play Detected");
                  
                        db.InsertScore(NewValues, NewValues.GeneralData.AudioTime / 1000);
                   
                 
                }

                //can be buggy with broken game (fix ur game then wat)
                if ((NewValues.GeneralData.OsuStatus.ToString() == "Playing") && (Audiotime > NewValues.GeneralData.AudioTime) && (timeSinceStartedPlaying.ElapsedMilliseconds > 2500) && (NewValues.Player.Score >= 1000))  {
                    Console.WriteLine("Retry detected");
                    db.InsertScore(NewValues, NewValues.GeneralData.AudioTime / 1000);
                };
                 
                CurrentScreen = NewValues.GeneralData.OsuStatus.ToString();
                BanchoUserStatus = NewValues.BanchoUser.BanchoStatus.ToString();
                oldRawStatus = NewValues.GeneralData.RawStatus;
                Audiotime = NewValues.GeneralData.AudioTime;
                isReplay = NewValues.Player.IsReplay;

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
