﻿using osu_progressCLI;
using System.Text.Json;
using System.Text.RegularExpressions;

public class Util
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


    public static string getBackground(string filepath) {
        Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Getting Background from{filepath}");
        if (File.Exists($"{Credentials.Instance.GetConfig().songfolder}/{filepath}"))
        {
            string fileContents = File.ReadAllText($@"{Credentials.Instance.GetConfig().songfolder}/{filepath}"); // Read the contents of the file

            string pattern = @"\d+,\d+,""(?<image_filename>[^""]+\.[a-zA-Z]+)"",\d+,\d+";

            Match match = Regex.Match(fileContents, pattern);

            if (match.Success)
            {
                string background = match.Groups["image_filename"].Value;
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Found Background Image: {background}");
                return background;
            }
        }
            try {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"No Background Image Found Finding Backup!");
                string bgDirectory = Path.Combine(Credentials.Instance.GetConfig().osufolder, "Data/bg");
                string btDirectory = Path.Combine(Credentials.Instance.GetConfig().osufolder, "Data/bt");

                string[] bgFiles = Directory.GetFiles(bgDirectory);
                string[] btFiles = Directory.GetFiles(btDirectory);

                if (bgFiles.Length > 0)
                {
                    Random random = new Random();
                    int index = random.Next(bgFiles.Length);
                    File.Copy(Path.GetFullPath(bgFiles[index]), $"public/img/{Path.GetFileName(bgFiles[index])}", true);
                    return Path.GetFileName(bgFiles[index]);
                }
                else if (btFiles.Length > 0)
                {
                    Random random = new Random();
                    int index = random.Next(btFiles.Length);
                    File.Copy(Path.GetFullPath(btFiles[index]), $"public/img/{Path.GetFileName(btFiles[index])}", true);
                    return Path.GetFileName(btFiles[index]);
                }
            } catch (Exception e){
                Logger.Log(Logger.Severity.Error, Logger.Framework.Misc, $"{e.Message}");
            }
        return null;
    }
}