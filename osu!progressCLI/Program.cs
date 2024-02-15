using Newtonsoft.Json.Linq;
using osu1progressbar.Game.MemoryProvider;
using osu_progressCLI;
using osu_progressCLI.Webserver.Server;
using osu1progressbar.Game.Database;
using osu_progressCLI.Datatypes;

class Program
{
    static async Task Main(string[] args)
    {
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
        }

        Task.Run(() =>  ScoreImporter.Instance.StartImporting());

        Console.WriteLine("Welcome to osu!progress");
        Console.WriteLine("If this is ur first time running read the README.txt");
        Console.WriteLine(
            "Keep this Terminal Open or the Progamm will stop if u want it run in the background follow the guide on the github!"
        );
        Logger.Log(Logger.Severity.Info, Logger.Framework.Misc, "Initialzing Components");

        OsuMemoryProvider memoryProvider = new OsuMemoryProvider("osu!");

        memoryProvider.Run();
        memoryProvider.ReadDelay = 1;

        DatabaseController databaseController = new DatabaseController();
        List<Score> scores = databaseController.GetPotentcialtopplays(250);

        //Console.WriteLine($"{job.ToString()}");
        await Webserver.Instance().start();

        memoryProvider.Stop();
    }
}
