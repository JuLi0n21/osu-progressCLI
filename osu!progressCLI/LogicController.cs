﻿using System;
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
        public Stopwatch userTimeStopWatch;
        private Stopwatch stopwatch;
        public Stopwatch timeSinceStartedPlaying;
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

                if (!isReplay && CurrentScreen == "Playing" && NewValues.GeneralData.OsuStatus.ToString() != "Playing")
                {
                    Console.WriteLine("Play Detected");
                    db.InsertScore(NewValues, timeSinceStartedPlaying.ElapsedMilliseconds);
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

                //can be buggy with broken game (fix ur game then wat)
                if ((NewValues.GeneralData.OsuStatus.ToString() == "Playing") && (Audiotime > NewValues.GeneralData.AudioTime) && (timeSinceStartedPlaying.ElapsedMilliseconds > 500) && (NewValues.Player.Hit300 > 0) && (NewValues.Player.Score >= 10000))  {
                    Console.WriteLine("Retry detected");
                    db.InsertScore(NewValues, timeSinceStartedPlaying.ElapsedMilliseconds);
                    timeSinceStartedPlaying.Restart();
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
