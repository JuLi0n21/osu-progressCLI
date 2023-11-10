using osu_progressCLI;
using osu_progressCLI.Webserver.Server;
using osu1progressbar.Game.MemoryProvider;

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

        //Task.Run(() =>  ScoreImporter.ImportScores("imports/shit.csv"));

        Console.WriteLine("Welcome to osu!progress");
        Console.WriteLine("If this is ur first time running read the README.txt");
        Console.WriteLine("Keep this Terminal Open or the Progamm will stop if u want it run in the background follow the guide on the github!");
        Logger.Log(Logger.Severity.Info, Logger.Framework.Misc, "Initialzing Components");

        OsuMemoryProvider memoryProvider = new OsuMemoryProvider("osu!");
        Credentials crendtials = Credentials.Instance;
        ApiController apiController = ApiController.Instance;

        memoryProvider.Run();
        memoryProvider.ReadDelay = 1;

        /*
        Task listenTask = Task.Run(async () =>
        {
            while (true)
            {
                await Webserver.Instance().listen();
            }
        });
        await listenTask;
        */

        await Webserver.Instance().start();

        memoryProvider.Stop();
    }
}