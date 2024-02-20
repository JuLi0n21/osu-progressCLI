using Newtonsoft.Json.Linq;
using osu1progressbar.Game.Database;
using osu1progressbar.Game.MemoryProvider;
using osu_progressCLI;
using osu_progressCLI.Datatypes;
using osu_progressCLI.Webserver.Server;
using Velopack;
using Fleck;
using Velopack.Sources;

class Program
{
    static async Task Main(string[] args)
    {
        VelopackApp.Build().WithRestarted((v) => { 
            // Move Config/Db/public to Parent Dir 
        }).WithFirstRun((v) =>
        {
            // Move Config/Db/public to Parent Dir
        }).Run();
        

        Logger.SetConsoleLogLevel(Logger.Severity.Warning);
        if (args.Length > 0)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-v")
                {
                    Logger.SetConsoleLogLevel(Logger.Severity.Debug);
                }
            }
        } else
        {   try
            {
                await UpdateMyApp();
            } catch (Exception e) {
                Console.WriteLine($"Automatic Updating Failed: {e.Message}");
            }
            
        }

        Console.WriteLine($"Welcome to osu!progress");
        Console.WriteLine("If this is ur first time running read the README.txt");
        Console.WriteLine(
            "Keep this Terminal Open or the Progamm will stop if u want it run in the background follow the guide on the github!"
        );

        Task.Run(() => ScoreImporter.Instance.StartImporting());

        Logger.Log(Logger.Severity.Info, Logger.Framework.Misc, "Initialzing Components");

        OsuMemoryProvider memoryProvider = new OsuMemoryProvider("osu!");

        memoryProvider.Run();
        memoryProvider.ReadDelay = 1;

        await Webserver.Instance().start();

        memoryProvider.Stop();
    }

    private static async Task UpdateMyApp()
    {
        var mgr = new UpdateManager(new GithubSource("https://github.com/JuLi0n21/autoupdatetest", null, false));    

        var newVersion = await mgr.CheckForUpdatesAsync();
        if (newVersion == null)
            return; 

        await mgr.DownloadUpdatesAsync(newVersion);

        mgr.ApplyUpdatesAndRestart(newVersion);
    }
}
