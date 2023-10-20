using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu_progressCLI;
using osu_progressCLI.server;
using osu1progressbar.Game.MemoryProvider;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;

class Program
{

    static async Task Main(string[] args)
    {

        Logger.SetConsoleLogLevel(Logger.Severity.Debug);
        Console.WriteLine("Welcome to osu!progress");
        Console.WriteLine("If this is ur first time running read the README.txt");
        Console.WriteLine("Keep this Terminal Open or the Progamm will stop if u want it run in the background follow the guide on the github!");
        Logger.Log(Logger.Severity.Info, Logger.Framework.Misc, "Initialzing Components");

        OsuMemoryProvider memoryProvider = new OsuMemoryProvider("osu!");

        Credentials crendtials = Credentials.Instance; 
        ApiController apiController = ApiController.Instance;

        memoryProvider.Run();
        memoryProvider.ReadDelay = 1;

        Webserver webserver = new Webserver();
        Task listenTask = Task.Run(async () =>
        {
            while (true)
            {
                await webserver.listen(); 
            }
        });
        await listenTask;

        memoryProvider.Stop();
    }

    
}

 
