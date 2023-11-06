using DeepCopy;
using osu_progressCLI;
using System.Text.RegularExpressions;

/// <summary>
/// random Utilies.
/// </summary>
public class Util
{
    public static T DeepCopy<T>(T input)
    {
        return DeepCopier.Copy(input);
    }


    public static string getBackground(string folderpath, string osufile, string fullparentpath = null)
    {

        Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Getting Background from: {folderpath}/{osufile}");

        string filepath = null;

        if (fullparentpath == null)
        {
            filepath = $"{Credentials.Instance.GetConfig().songfolder}/{folderpath}/{osufile}";
        }
        else
        {
            filepath = $"{fullparentpath}/{folderpath}/{osufile}";
        }

        if (File.Exists(filepath))
        {
            string fileContents = File.ReadAllText($@"{filepath}"); // Read the contents of the file

            string pattern = @"\d+,\d+,""(?<image_filename>[^""]+\.[a-zA-Z]+)"",\d+,\d+";

            Match match = Regex.Match(fileContents, pattern);

            if (match.Success)
            {
                string background = match.Groups["image_filename"].Value;
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Found Background Image: {background}");
                return $"/{folderpath}/{background}";
            }

            pattern = @"\d+,\d+,""(?<image_filename>[^""]+\.[a-zA-Z]+)""";

            match = Regex.Match(fileContents, pattern);

            if (match.Success)
            {
                string background = match.Groups["image_filename"].Value;
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Found Background Image: {background}");
                return $"/{folderpath}/{background}";
            }

            //older beatmap versions x,x,"name"
        }
        try
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"No Background Image Found Finding Backup!");
            string bgDirectory = Path.Combine(Credentials.Instance.GetConfig().osufolder, "Data/bg");
            string btDirectory = Path.Combine(Credentials.Instance.GetConfig().osufolder, "Data/bt");

            string[] bgFiles = Directory.GetFiles(bgDirectory);
            string[] btFiles = Directory.GetFiles(btDirectory);

            Console.WriteLine(btDirectory);
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

        }
        catch (Exception e)
        {
            Logger.Log(Logger.Severity.Error, Logger.Framework.Misc, $"{e.Message}");
        }

        return null;
    }

    public static string osufile(string foldername, string version, string fullparentpath = null)
    {

        string[] files = Array.Empty<string>();//empty array
        if (fullparentpath == null)
        {
            files = Directory.GetFiles($"{Credentials.Instance.GetConfig().songfolder}/{foldername}");
        }
        else
        {
            files = Directory.GetFiles($"{fullparentpath}/{foldername}");
        }

        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Contains(version))
                return Path.GetFileName(files[i]);
        }

        return null;
    }
}