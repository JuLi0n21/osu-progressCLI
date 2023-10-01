using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu_progressCLI;
using osu_progressCLI.server;
using osu1progressbar.Game.MemoryProvider;


class Program
{

    static async Task Main(string[] args)
    {
        
        Console.WriteLine("Welcome to osu!progress");
        Console.WriteLine("If this is ur first time running read the README.txt");
        OsuMemoryProvider memoryProvider = new OsuMemoryProvider("osu!");

        Credentials crendtials = Credentials.Instance; //instanceted it to load in data already
        ApiController apiController = ApiController.Instance; //to already get a access_token...
        
        //Configmanager congif = Configmanager.Instance; //incase user wants to customize anything could be done here maybe

        memoryProvider.Run();
        memoryProvider.ReadDelay = 1;

      //  QueryParser.Filter("asdfsadfsadfsdfadsafsdafkjsadfk;jlasdkflslj");
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

 
