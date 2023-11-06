using osu_progressCLI;
using osu_progressCLI.server;
using osu1progressbar.Game.Database;
using OsuMemoryDataProvider.OsuMemoryModels;
using System.Diagnostics;


//detect score fails 
//change saved time to seconds

namespace osu1progressbar.Game.Logicstuff
{
    /// <summary>
    /// Logic to Track Scores and Times
    /// </summary>
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

            Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Instanciated LogicController");
        }

        private static void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            DateTime fileCreationTime = File.GetCreationTime(e.FullPath);
            if (fileCreationTime > newestFileTime)
            {
                newestFile = e.Name;
                newestFileTime = fileCreationTime;
                if (newestFile.EndsWith(".osr"))
                {
                    replayname = newestFile;
                }
            }
        }


        //the first time gets currentliy ignored cause it gets restarted after first entry
        //(and is probably not running cause its set to stop in the memoryprovider incase no osu is found
        public bool Logiccheck(OsuBaseAddresses Values)
        {
            OsuBaseAddresses Data = new OsuBaseAddresses();
            Data = Util.DeepCopy(Values);
            if (Data != null)
            {

                if (PreviousScreen != "Playing" && Data.GeneralData.OsuStatus.ToString() == "Playing")
                {
                    timeSinceStartedPlaying = Stopwatch.StartNew();
                    startime = Data.GeneralData.AudioTime;
                }

                if (PreviousScreen != Data.GeneralData.OsuStatus.ToString())
                {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, $"Screentime: {screenTimeStopWatch.ElapsedMilliseconds / 1000}s {PreviousScreen}");
                    db.UpdateTimeWasted(PreviousRawStatus, screenTimeStopWatch.ElapsedMilliseconds / 1000);
                    screenTimeStopWatch.Restart();
                }

                if (PreviousBanchoStatus != Data.BanchoUser.BanchoStatus.ToString())
                {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, $"Banchotime: {BanchoTimeStopWatch.ElapsedMilliseconds / 1000}s {PreviousBanchoStatus}");
                    db.UpdateBanchoTime(PreviousBanchoStatus, BanchoTimeStopWatch.ElapsedMilliseconds / 1000);
                    BanchoTimeStopWatch.Restart();
                }

                if (!isReplay && PreviousScreen == "Playing" && Data.GeneralData.OsuStatus.ToString() == "ResultsScreen")
                {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Pass Detected Waiting for Replay");

                    Task.Run(() =>
                    {
                        try
                        {
                            watcher = new FileSystemWatcher(folderPath);
                            watcher.Created += OnFileCreated;
                            watcher.EnableRaisingEvents = true;
                            watcher.WaitForChanged(WatcherChangeTypes.Created);
                            Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Replay found");
                            db.InsertScore(Data, Data.GeneralData.AudioTime / 1000, "Pass", replayname);
                            replayname = null;
                            watcher.Dispose();
                        }
                        catch (Exception e)
                        {
                            Logger.Log(Logger.Severity.Debug, Logger.Framework.Logic, $"{e.Message} Trying to Save score anyway");
                            db.InsertScore(Data, Data.GeneralData.AudioTime / 1000, "Pass", replayname);
                        }
                    });
                }
                else if (!isReplay && PreviousScreen == "Playing" && Data.GeneralData.OsuStatus.ToString() == "MultiplayerResultsscreen")
                {
                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Multiplay detected");
                    if (Data.Player.Score >= 1000)
                    {
                        Task.Run(() =>
                        {
                            if (Data.Player.HP == 0)
                            {
                                db.InsertScore(Data, Data.GeneralData.AudioTime / 1000, "Fail");
                                Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Multiplay Failed");
                            }
                            else
                            {
                                db.InsertScore(Data, Data.GeneralData.AudioTime / 1000, "Pass");
                                Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Multiplay Passed");
                            }

                        });
                    }
                    else
                    {
                        Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Score Threshhold of 1000 not Reached");
                    }
                }
                else if (!isReplay && PreviousScreen == "Playing" && Data.GeneralData.OsuStatus.ToString() != "Playing")
                {
                    if (Data.Player.Score >= 1000)
                    {
                        Task.Run(() =>
                        {
                            if (Data.Player.HP == 0)
                            {
                                db.InsertScore(Data, Data.GeneralData.AudioTime / 1000, "Fail");
                                Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Cancel detected");
                            }
                            else
                            {
                                db.InsertScore(Data, Data.GeneralData.AudioTime / 1000, "Cancel");
                                Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Fail detected");
                            }

                        });
                    }
                    else
                    {
                        Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Score Threshhold of 1000 not Reached");
                    }
                }

                //can be buggy with broken game (fix ur game then wat)
                if ((Data.GeneralData.OsuStatus.ToString() == "Playing") && (Audiotime > Data.GeneralData.AudioTime) && (Data.Player.Score >= 1000) && (timeSinceStartedPlaying.ElapsedMilliseconds > 1000))
                {

                    Logger.Log(Logger.Severity.Info, Logger.Framework.Logic, "Retry Detected");
                    db.InsertScore(Data, Data.GeneralData.AudioTime / 1000, "Retry");
                };



                PreviousScreen = Data.GeneralData.OsuStatus.ToString();
                PreviousBanchoStatus = Data.BanchoUser.BanchoStatus.ToString();
                PreviousRawStatus = Data.GeneralData.RawStatus;
                Audiotime = Data.GeneralData.AudioTime;
                isReplay = Data.Player.IsReplay;

                Task.Run(async () => await Webserver.Instance().SendData("baseaddresses", Data));

                return true;
            }
            return false;
        }
    }
}