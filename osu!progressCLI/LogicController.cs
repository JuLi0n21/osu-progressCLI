using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using osu_progressCLI;
using osu_progressCLI.server;
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
        private static string replayname;

        public Stopwatch screenTimeStopWatch;
        public Stopwatch BanchoTimeStopWatch;
        private Stopwatch stopwatch;
        public Stopwatch timeSinceStartedPlaying;

        private Webserver webserver;


        FileSystemWatcher watcher = null;
        static string folderPath = @$"{Credentials.Instance.GetConfig().osufolder}\Data\r"; // Replace with your folder path
        static string newestFile;
        static DateTime newestFileTime = DateTime.MinValue;

        public int startime;
        public LogicController()
        {
            db = new DatabaseController();
            db.Init();

            screenTimeStopWatch = new Stopwatch();
            screenTimeStopWatch.Start();

            BanchoTimeStopWatch = new Stopwatch();
            BanchoTimeStopWatch.Start();

            timeSinceStartedPlaying = new Stopwatch();

            stopwatch = Stopwatch.StartNew();

            Thread watcherThread = new Thread(new ThreadStart(StartFileSystemWatcher));
            watcherThread.Start();

            Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Instanciated LogicController");
        }

        private void StartFileSystemWatcher()
        {
            watcher = new FileSystemWatcher(folderPath);
            watcher.Created += OnFileCreated;
            watcher.EnableRaisingEvents = true;
        }

        private static void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            DateTime fileCreationTime = File.GetCreationTime(e.FullPath);
            if (fileCreationTime > newestFileTime)
            {
                newestFile = e.Name;
                newestFileTime = fileCreationTime;
                if (newestFile.EndsWith(".osr")) { 
                    replayname = newestFile;
                }
            }
        }


        //the first time gets currentliy ignored cause it gets restarted after first entry (and is probably not running cause its set to stop in the memoryprovider incase no osu is found
        public bool Logiccheck(OsuBaseAddresses Values)
        {
            OsuBaseAddresses NewValues = new OsuBaseAddresses();
            NewValues = Copy.DeepCopy(Values);   
            try
            {

                if (CurrentScreen != "Playing" && NewValues.GeneralData.OsuStatus.ToString() == "Playing") { 
                    timeSinceStartedPlaying = Stopwatch.StartNew();
                    startime = NewValues.GeneralData.AudioTime;
                }

                if (CurrentScreen != NewValues.GeneralData.OsuStatus.ToString())
                {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, $"Screentime: {screenTimeStopWatch.ElapsedMilliseconds / 1000}s {CurrentScreen}");
                    db.UpdateTimeWasted(oldRawStatus, screenTimeStopWatch.ElapsedMilliseconds / 1000);
                    screenTimeStopWatch.Restart();
                }

                //DOESNT WORK WITH THE COPYED DATA (IDK WHY)
                if (BanchoUserStatus != Values.BanchoUser.BanchoStatus.ToString())
                {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, $"Banchotime: {BanchoTimeStopWatch.ElapsedMilliseconds / 1000}s {BanchoUserStatus}");
                    db.UpdateBanchoTime(BanchoUserStatus, BanchoTimeStopWatch.ElapsedMilliseconds / 1000);
                    BanchoTimeStopWatch.Restart();
                }

                if (!isReplay && CurrentScreen == "Playing" && NewValues.GeneralData.OsuStatus.ToString() == "ResultsScreen") {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Pass Detected Waiting for Replay");

                    Task.Run(() =>
                    {
                        watcher.WaitForChanged(WatcherChangeTypes.Created);
                        Logger.Log(Logger.Severity.Debug, Logger.Framework.Logic, $"old values: {NewValues.Player.Hit300} new values: {Values.Player.Hit300}");
                        Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Replay found");
                        db.InsertScore(NewValues, NewValues.GeneralData.AudioTime / 1000, "Pass", replayname);
                        Logger.Log(Logger.Severity.Debug, Logger.Framework.Logic, $"old values: {NewValues.Player.Hit300} new values: {Values.Player.Hit300}");
                        replayname = null;
                    });
                } 
                else if (!isReplay && CurrentScreen == "Playing" && NewValues.GeneralData.OsuStatus.ToString() != "Playing")
                {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Cancel Detected");

                    Task.Run(() =>
                    {
                        if (NewValues.Player.HP == 0)
                        {
                            db.InsertScore(NewValues, NewValues.GeneralData.AudioTime / 1000, "Fail");
                        }
                        else {
                            db.InsertScore(NewValues, NewValues.GeneralData.AudioTime / 1000, "Cancel");
                        }
                      
                    });
                }

                //can be buggy with broken game (fix ur game then wat)
                if ((NewValues.GeneralData.OsuStatus.ToString() == "Playing") && (Audiotime > NewValues.GeneralData.AudioTime) && (timeSinceStartedPlaying.ElapsedMilliseconds > 2500) && (NewValues.Player.Score >= 1000))  {
                    
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Retry Detected");
                    db.InsertScore(NewValues, NewValues.GeneralData.AudioTime / 1000, "Retry");
                };

               

                CurrentScreen = NewValues.GeneralData.OsuStatus.ToString();
               // Console.WriteLine($"{CurrentScreen}, {NewValues.GeneralData.OsuStatus}");
                BanchoUserStatus = Values.BanchoUser.BanchoStatus.ToString();
                oldRawStatus = NewValues.GeneralData.RawStatus;
                Audiotime = NewValues.GeneralData.AudioTime;
                isReplay = NewValues.Player.IsReplay;

                Task.Run(async () => await Webserver.Instance().SendData("baseaddresses", Values));

                return true;
            }
           
            catch (Exception ex)
            {
                Logger.Log(Logger.Severity.Error, Logger.Framework.Logic, $"{ex.Message}");
                return false;
            }
        }
    }
}