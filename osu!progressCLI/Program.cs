using Microsoft.Win32;
using osu1progressbar.Game.MemoryProvider;
using osu_progressCLI;
using osu_progressCLI.Webserver.Server;
using Velopack;
using Velopack.Sources;

class Program
{
    static async Task Main(string[] args)
    {
        VelopackApp
            .Build()
            .WithRestarted(
                (v) =>
                {
                    setup();
                }
            )
            .WithFirstRun(
                (v) =>
                {
                    setup();
                }
            )
            .WithBeforeUninstallFastCallback(
                (v) =>
                {
                    if (File.Exists(Credentials.credentialsFilePath))
                        File.Delete(Credentials.credentialsFilePath);

                    if (File.Exists(Credentials.loginwithosuFilePath))
                        File.Delete(Credentials.loginwithosuFilePath);
                }
            )
            .Run();

        Logger.SetConsoleLogLevel(Logger.Severity.Warning);
        if (args.Length > 0)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-v")
                {
                    Logger.SetConsoleLogLevel(Logger.Severity.Debug);
                    setup();
                }
            }
        }
        else
        {
            try
            {
                await UpdateMyApp();
            }
            catch (Exception e)
            {
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
        var mgr = new UpdateManager(
            new GithubSource("https://github.com/JuLi0n21/osu-progressCLI", null, false)
        );

        var newVersion = await mgr.CheckForUpdatesAsync();
        if (newVersion == null)
            return;

        await mgr.DownloadUpdatesAsync(newVersion);

        mgr.ApplyUpdatesAndRestart(newVersion);
    }

    private static void setup()
    {
        var key = Registry.GetValue(
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\osu\shell\open\command",
            "",
            null
        );

        if (key != null)
        {
            string[] keyparts = key.ToString().Split('"');

            string osufolder = Path.GetDirectoryName(keyparts[1]);
            if (osufolder != null)
            {
                Credentials.Instance.UpdateConfig(
                    osufolder: osufolder,
                    songfolder: @$"{osufolder}\Songs"
                );
            }
        }
        if (Directory.Exists(Webserver.DEFAULT_PATH))
        {
            Directory.Delete(Webserver.DEFAULT_PATH, true);
        }
        Directory.Move("public", Webserver.DEFAULT_PATH);
    }
}
