public static class Logger
{
    public enum Severity
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public enum Framework
    {
        Logic,
        Database,
        Server,
        Network,
        MemoryProvider,
        Calculator,
        Scoreimporter,
        Misc
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    private static Severity currentLogLevel = Severity.Debug;

    public static void SetConsoleLogLevel(Severity level)
    {
        currentLogLevel = level;
    }

    public static async void Log(Severity severity, Framework framework, string message)
    {
        string logEntry =
            $"{DateTime.Now} | {severity.ToString().PadRight(7)} | {framework.ToString().PadRight(8)} | {message}";

        string logFileName = $@"Logs\{framework}_Log.txt";

        try
        {
            await Task.Run(() =>
            {
                using (
                    var fileStream = new FileStream(
                        logFileName,
                        FileMode.Append,
                        FileAccess.Write,
                        FileShare.ReadWrite
                    )
                )
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.WriteLineAsync(logEntry);
                }
            });

            if (severity >= Severity.Error)
            {
                Console.Error.WriteLine(logEntry);
            }
            else if (severity >= currentLogLevel)
            {
                Console.WriteLine(logEntry);
            }
        }
        catch (Exception e)
        {
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }
        }
    }
}
