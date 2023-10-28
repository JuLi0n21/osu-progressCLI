using System.Text.Json;

public class Copy
{
    public static T DeepCopy<T>(T input)
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(input);
            return JsonSerializer.Deserialize<T>(jsonString);
        }
        catch (Exception e) {
            Logger.Log(Logger.Severity.Error, Logger.Framework.Misc, @$"{e.Message}");
            return default(T);
        }
    }
}