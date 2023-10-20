using System.Text.Json;

public class Copy
{
    public static T DeepCopy<T>(T input)
    {
        var jsonString = JsonSerializer.Serialize(input);
        return JsonSerializer.Deserialize<T>(jsonString);
    }
}