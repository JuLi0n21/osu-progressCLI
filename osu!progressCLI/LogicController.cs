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

        private string PreviousScreen = null;
        private int PreviousRawStatus = -1;
        private string PreviousBanchoStatus = null;
        private int Audiotime;
        private bool isReplay = false;
        private static string replayname;

        public Stopwatch screenTimeStopWatch;
        public Stopwatch BanchoTimeStopWatch;
        public Stopwatch timeSinceStartedPlaying;

        FileSystemWatcher watcher = null;
        static string folderPath = @$"{Credentials.Instance.GetConfig().osufolder}\Data\r";
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


        //the first time gets currentliy ignored cause it gets restarted after first entry
        //(and is probably not running cause its set to stop in the memoryprovider incase no osu is found
        public bool Logiccheck(OsuBaseAddresses Values)
        {
            OsuBaseAddresses NewValues = new OsuBaseAddresses();
            NewValues = Util.DeepCopy(Values);
            if (NewValues != null) {

                if (PreviousScreen != "Playing" && NewValues.GeneralData.OsuStatus.ToString() == "Playing") {
                    timeSinceStartedPlaying = Stopwatch.StartNew();
                    startime = NewValues.GeneralData.AudioTime;
                }

                if (PreviousScreen != NewValues.GeneralData.OsuStatus.ToString())
                {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, $"Screentime: {screenTimeStopWatch.ElapsedMilliseconds / 1000}s {PreviousScreen}");
                    db.UpdateTimeWasted(PreviousRawStatus, screenTimeStopWatch.ElapsedMilliseconds / 1000);
                    screenTimeStopWatch.Restart();
                }

                //DOESNT WORK WITH THE COPYED DATA (IDK WHY)
                if (PreviousBanchoStatus != Values.BanchoUser.BanchoStatus.ToString())
                {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, $"Banchotime: {BanchoTimeStopWatch.ElapsedMilliseconds / 1000}s {PreviousBanchoStatus}");
                    db.UpdateBanchoTime(PreviousBanchoStatus, BanchoTimeStopWatch.ElapsedMilliseconds / 1000);
                    BanchoTimeStopWatch.Restart();
                }

                if (!isReplay && PreviousScreen == "Playing" && NewValues.GeneralData.OsuStatus.ToString() == "ResultsScreen")
                {
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
                else if (!isReplay && PreviousScreen == "Playing" && NewValues.GeneralData.OsuStatus.ToString() == "MultiplayerResultsscreen") {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Multiplay detected");
                    if (NewValues.Player.Score >= 1000)
                    {
                        Task.Run(() =>
                        {
                            if (NewValues.Player.HP == 0)
                            {
                                db.InsertScore(NewValues, NewValues.GeneralData.AudioTime / 1000, "Fail");
                                Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Multiplay Failed");
                            }
                            else
                            {
                                db.InsertScore(NewValues, NewValues.GeneralData.AudioTime / 1000, "Pass");
                                Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Multiplay Passed");
                            }

                        });
                    }
                    else
                    {
                        Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Score Threshhold of 1000 not Reached");
                    }
                }
                else if (!isReplay && PreviousScreen == "Playing" && NewValues.GeneralData.OsuStatus.ToString() != "Playing")
                {
                    if (NewValues.Player.Score >= 1000)
                    {
                        Task.Run(() =>
                        {
                            if (NewValues.Player.HP == 0)
                            {
                                db.InsertScore(NewValues, NewValues.GeneralData.AudioTime / 1000, "Fail");
                                Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Cancel detected");
                            }
                            else
                            {
                                db.InsertScore(NewValues, NewValues.GeneralData.AudioTime / 1000, "Cancel");
                                Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Fail detected");
                            }

                        });
                    }
                    else {
                        Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Score Threshhold of 1000 not Reached");
                    }
                }

                //can be buggy with broken game (fix ur game then wat)
                if ((NewValues.GeneralData.OsuStatus.ToString() == "Playing") && (Audiotime > NewValues.GeneralData.AudioTime) && (NewValues.Player.Score >= 1000) && (timeSinceStartedPlaying.ElapsedMilliseconds > 1000)) {

                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Retry Detected");
                    db.InsertScore(NewValues, NewValues.GeneralData.AudioTime / 1000, "Retry");
                };



                PreviousScreen = NewValues.GeneralData.OsuStatus.ToString();
                PreviousBanchoStatus = Values.BanchoUser.BanchoStatus.ToString();
                PreviousRawStatus = NewValues.GeneralData.RawStatus;
                Audiotime = NewValues.GeneralData.AudioTime;
                isReplay = NewValues.Player.IsReplay;

                Task.Run(async () => await Webserver.Instance().SendData("baseaddresses", Values));

                return true;
            }
            return false;
        } 
    }
}