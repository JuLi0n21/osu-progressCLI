using System;
using System.IO;

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

    public static void Log(Severity severity, Framework framework, string message)
    {
        string logEntry = $"{DateTime.Now} | {severity.ToString().PadRight(7)} | {framework.ToString().PadRight(8)} | {message}";

        string logFileName = $"{framework}_Log.txt";

        try
        {
            File.AppendAllText(logFileName, logEntry + Environment.NewLine);

            if (severity >= Severity.Error)
            {
                Console.Error.WriteLine(logEntry);
            }
            else if (severity >= currentLogLevel)
            {
                Console.WriteLine(logEntry);
            }
        }
        catch(Exception e){
        
            Logger.Log(Severity.Error,Framework.Misc,e.Message);
        }
    }
}
