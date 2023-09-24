using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu1progressbar.Game.Logicstuff;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using OsuMemoryDataProvider.OsuMemoryModels.Direct;


//make it slow down when osu! not found so it can easily run in the background wihtouht wasting much cpu cycles

namespace osu1progressbar.Game.MemoryProvider
{
    public class OsuMemoryProvider
    {
        private readonly string osuWindowTitle;
        public int ReadDelay { get; set; } = 200;
        private readonly object minMaxLock = new object();
        private readonly int throttledDelay = 1000;
        private double memoryReadTimeMin = double.PositiveInfinity;
        private double memoryReadTimeMax = double.NegativeInfinity;
        public string access_token { get; set; }

        LogicController logic;

        private OsuBaseAddresses baseAddresses;
     


        private readonly StructuredOsuMemoryReader sreader;
        private CancellationTokenSource cts = new CancellationTokenSource();

        public OsuMemoryProvider(string osuWindowTitle)
        {
            sreader = StructuredOsuMemoryReader.Instance.GetInstanceForWindowTitleHint(osuWindowTitle);
            baseAddresses = new OsuBaseAddresses();
            
            logic = new LogicController();
            Console.WriteLine("Instancated OsuMemoryProvider...");
        }


        ~OsuMemoryProvider()
        {
            cts.Cancel();
        }

        //HELPER FUMCTIONS COPIED FROM StructuredOsuMemoryProviderTester 
        private T ReadProperty<T>(object readObj, string propName, T defaultValue = default) where T : struct
        {
            if (sreader.TryReadProperty(readObj, propName, out var readResult))
                return (T)readResult;

            return defaultValue;
        }

        private T ReadClassProperty<T>(object readObj, string propName, T defaultValue = default) where T : class
        {
            if (sreader.TryReadProperty(readObj, propName, out var readResult))
                return (T)readResult;

            return defaultValue;
        }

        private int ReadInt(object readObj, string propName)
            => ReadProperty(readObj, propName, -5);
        private short ReadShort(object readObj, string propName)
            => ReadProperty<short>(readObj, propName, -5);

        private float ReadFloat(object readObj, string propName)
            => ReadProperty(readObj, propName, -5f);

        private string ReadString(object readObj, string propName)
            => ReadClassProperty(readObj, propName, "INVALID READ");

        public async void Run()
        {
            Console.WriteLine("OsuMemoryProvider Run call...");
            sreader.InvalidRead += SreaderOnInvalidRead;
            await Task.Run(async () =>
            {
                //Console.WriteLine("Starting memoryReader");
                Stopwatch stopwatch;
                double readTimeMs, readTimeMsMin, readTimeMsMax;
                sreader.WithTimes = true;
                var readUsingProperty = false;
                //var baseAddresses = new OsuBaseAddresses();
                while (true)
                {

                    if (cts.IsCancellationRequested) return;

                    if (!sreader.CanRead)
                    {

                        Console.WriteLine("Waiting for Osu! Process");
                        Console.CursorLeft = 0;
                        Console.CursorTop = Console.CursorTop - 1;
                        
                        //ReadDelay = throttledDelay;
                        await Task.Delay(ReadDelay);
                        continue;
                    }

                    stopwatch = Stopwatch.StartNew();
                    if (readUsingProperty)
                    {
                        baseAddresses.Beatmap.Id = ReadInt(baseAddresses.Beatmap, nameof(CurrentBeatmap.Id));
                        baseAddresses.Beatmap.SetId = ReadInt(baseAddresses.Beatmap, nameof(CurrentBeatmap.SetId));
                        baseAddresses.Beatmap.MapString = ReadString(baseAddresses.Beatmap, nameof(CurrentBeatmap.MapString));
                        baseAddresses.Beatmap.FolderName = ReadString(baseAddresses.Beatmap, nameof(CurrentBeatmap.FolderName));
                        baseAddresses.Beatmap.OsuFileName = ReadString(baseAddresses.Beatmap, nameof(CurrentBeatmap.OsuFileName));
                        baseAddresses.Beatmap.Md5 = ReadString(baseAddresses.Beatmap, nameof(CurrentBeatmap.Md5));
                        baseAddresses.Beatmap.Ar = ReadFloat(baseAddresses.Beatmap, nameof(CurrentBeatmap.Ar));
                        baseAddresses.Beatmap.Cs = ReadFloat(baseAddresses.Beatmap, nameof(CurrentBeatmap.Cs));
                        baseAddresses.Beatmap.Hp = ReadFloat(baseAddresses.Beatmap, nameof(CurrentBeatmap.Hp));
                        baseAddresses.Beatmap.Od = ReadFloat(baseAddresses.Beatmap, nameof(CurrentBeatmap.Od));
                        baseAddresses.Beatmap.Status = ReadShort(baseAddresses.Beatmap, nameof(CurrentBeatmap.Status));
                        baseAddresses.Skin.Folder = ReadString(baseAddresses.Skin, nameof(Skin.Folder));
                        baseAddresses.GeneralData.RawStatus = ReadInt(baseAddresses.GeneralData, nameof(GeneralData.RawStatus));
                        baseAddresses.GeneralData.GameMode = ReadInt(baseAddresses.GeneralData, nameof(GeneralData.GameMode));
                        baseAddresses.GeneralData.Retries = ReadInt(baseAddresses.GeneralData, nameof(GeneralData.Retries));
                        baseAddresses.GeneralData.AudioTime = ReadInt(baseAddresses.GeneralData, nameof(GeneralData.AudioTime));
                        baseAddresses.GeneralData.Mods = ReadInt(baseAddresses.GeneralData, nameof(GeneralData.Mods));
                        Console.WriteLine(JsonConvert.SerializeObject(baseAddresses, Formatting.Indented));
                    }
                    else
                    {
                        sreader.TryRead(baseAddresses.Beatmap);
                        sreader.TryRead(baseAddresses.Skin);
                        sreader.TryRead(baseAddresses.GeneralData);
                        sreader.TryRead(baseAddresses.BanchoUser);
                    }

                    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.SongSelect)
                        sreader.TryRead(baseAddresses.SongSelectionScores);
                    else
                        baseAddresses.SongSelectionScores.Scores.Clear();

                    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.ResultsScreen)
                        sreader.TryRead(baseAddresses.ResultsScreen);

                    if (baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.Playing)
                    {
                        sreader.TryRead(baseAddresses.Player);
                        //TODO: flag needed for single/multi player detection (should be read once per play in singleplayer)
                        sreader.TryRead(baseAddresses.LeaderBoard);
                        sreader.TryRead(baseAddresses.KeyOverlay);
                        if (readUsingProperty)
                        {
                            //Testing reading of reference types(other than string)
                            sreader.TryReadProperty(baseAddresses.Player, nameof(Player.Mods), out var dummyResult);
                        }
                    }
                    else
                    {
                        baseAddresses.LeaderBoard.Players.Clear();
                    }

                    var hitErrors = baseAddresses.Player?.HitErrors;
                    if (hitErrors != null)
                    {
                        var hitErrorsCount = hitErrors.Count;
                        hitErrors.Clear();
                        hitErrors.Add(hitErrorsCount);
                    }

                    stopwatch.Stop();
                    readTimeMs = stopwatch.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond;
                    lock (minMaxLock)
                    {
                        if (readTimeMs < memoryReadTimeMin) memoryReadTimeMin = readTimeMs;
                        if (readTimeMs > memoryReadTimeMax) memoryReadTimeMax = readTimeMs;
                        // copy value since we're inside lock
                        readTimeMsMin = memoryReadTimeMin;
                        readTimeMsMax = memoryReadTimeMax;
                    }

                    //OsuBaseAddressesBindable.TriggerChange();
                    //Console.WriteLine(JsonConvert.SerializeObject(baseAddresses, Formatting.Indented),LoggingTarget.Information,LogLevel.Debug);
                    logic.Logiccheck(baseAddresses);
                    sreader.ReadTimes.Clear();
                    await Task.Delay(ReadDelay);
                }
            }, cts.Token);
        }

        public void Stop()
        {
            cts.Cancel();
        }
        private void SreaderOnInvalidRead(object sender, (object readObject, string propPath) e)
        {
            try
            {
                //Console.WriteLine($"{DateTime.Now:T} Error reading {e.propPath}{Environment.NewLine}");
            }
            catch (ObjectDisposedException)
            {

            }
        }

        public string GetAllDataJson()
        {
            if (!sreader.CanRead) return "Osu not Found!";

            return JsonConvert.SerializeObject(baseAddresses, Formatting.Indented);
        }

        public OsuBaseAddresses GetBaseAddresses()
        {
            return baseAddresses;
        }

    }
}
