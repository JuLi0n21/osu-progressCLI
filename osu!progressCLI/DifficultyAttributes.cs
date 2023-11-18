using osu_progressCLI.Datatypes;
using osu1progressbar.Game.Database;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace osu_progressCLI
{
    internal class DifficultyAttributes
    {

        public static PerfomanceAttributes CalculatePP(string folderName, string fileName, int score, int mods, int missCount, int mehCount, int goodCount, int perfectcount, double accuracy = 0, int combo = 0, int mode = 0)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"Calculating PP: Miss:{missCount}, Meh:{mehCount}, Good:{goodCount}, Perfect:{perfectcount}, Combo:{combo} Mods: {mods}, Mode: {ModeConverter(mode)}({mode})");
            PerfomanceAttributes perfomanceAttributes = new PerfomanceAttributes();

            string fullPath = Path.Combine(Credentials.Instance.GetConfig().songfolder, folderName, fileName);
            string command = $"dotnet PerformanceCalculator.dll simulate {ModeConverter(mode)} \"{fullPath}\" {cmdmodehelper(accuracy, score, mods, missCount, mehCount, goodCount, perfectcount, combo, mode)}";

            string output = cmdOutput("osu-tools", command);

            perfomanceAttributes = ParseOutput(output);
            perfomanceAttributes.grade = CalculateGrade(perfectcount, goodCount, mehCount, missCount);

            return perfomanceAttributes;
        }

        public static PerfomanceAttributes CalculatePP(int Beatmapid, int score, int mods, int missCount, int mehCount, int goodCount, int perfectcount, double accuracy = 0, int combo = 0, int mode = 0)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"Calculating PP from id: ID:{Beatmapid} Miss:{missCount}, Meh:{mehCount}, Good:{goodCount}, Perfect:{perfectcount}, Combo:{combo} Mods: {mods}, Mode: {ModeConverter(mode)}({mode})");

            PerfomanceAttributes perfomanceAttributes = new PerfomanceAttributes();

            string command = $"dotnet PerformanceCalculator.dll simulate {ModeConverter(mode)} {Beatmapid} {cmdmodehelper(accuracy, score, mods, missCount, mehCount, goodCount, perfectcount, combo, mode)}";

            string output = cmdOutput("osu-tools", command);

            perfomanceAttributes = ParseOutput(output);
            perfomanceAttributes.grade = CalculateGrade(perfectcount, goodCount, mehCount, missCount);

            return perfomanceAttributes;
        }

        private static string cmdmodehelper(double accuracy, int score, int mods, int missCount, int mehCount, int goodCount, int perfectcount, int combo, int mode = 0)
        {

            StringBuilder command = new StringBuilder();
            switch (mode)
            {
                case 0:
                    {
                        if (accuracy != 0)
                        {
                            command.Append($" --accuracy {accuracy}");
                        }

                        if (combo != 0)
                        {
                            command.Append($" --combo {combo}");
                        }
                        else
                        {
                            command.Append($" --percent-combo 100");
                        }
                        if (missCount != 0)
                        {
                            command.Append($" --misses {missCount}");
                        }
                        if (mehCount != 0)
                        {
                            command.Append($" --mehs {mehCount}");
                        }
                        if (goodCount != 0)
                        {
                            command.Append($" --goods {goodCount}");
                        }

                        command.Append($" {ModParser.PPCalcMods(mods)}");
                        return command.ToString();
                    }
                case 1: //taiko
                    if (accuracy != 0)
                    {
                        command.Append($" --accuracy {accuracy}");
                    }

                    if (combo != 0)
                    {
                        command.Append($" --combo {combo}");
                    }
                    if (missCount != 0)
                    {
                        command.Append($" --misses {missCount}");
                    }
                    if (goodCount != 0)
                    {
                        command.Append($" --goods {goodCount}");
                    }

                    command.Append($" {ModParser.PPCalcMods(mods)}");
                    return command.ToString();
                case 2: //catch
                    if (accuracy != 0)
                    {
                        command.Append($" --accuracy {accuracy}");
                    }

                    if (combo != 0)
                    {
                        command.Append($" --combo {combo}");
                    }
                    if (missCount != 0)
                    {
                        command.Append($" --misses {missCount}");
                    }

                    command.Append($" {ModParser.PPCalcMods(mods)}");
                    return command.ToString();
                case 3: //mania
                    return $" --score {score} {ModParser.PPCalcMods(mods)}";
            }

            Logger.Log(Logger.Severity.Warning, Logger.Framework.Calculator, $"Mode: {mode} not Supported!");
            return null;
        }

        private static PerfomanceAttributes ParseOutput(string output)
        {
            string pattern = @"star rating\s+:\s+(?<starrating>[\d.]+)|pp\s+:\s+(?<pp>[\d.,]+)|max combo\s+:\s+(?<maxcombo>[\d.,]+)|accuracy\s+:\s+(?<accuracy>[\d.]+)|speed\s+:\s+(?<speed>[\d.]+)|aim\s+:\s+(?<aim>[\d.]+)|overall difficulty\s+:\s+(?<overalldifficulty>[\d.]+)|approach rate\s+:\s+(?<appraochrate>[\d.]+)";

            Regex regex = new Regex(pattern);

            MatchCollection matches = regex.Matches(output);

            PerfomanceAttributes attributes = new PerfomanceAttributes();

            foreach (Match match in matches)
            {
                if (match.Success)
                {

                    if (match.Groups["speed"].Success && double.TryParse(match.Groups["speed"].Value.Replace(".", "").Replace(",", ""), out double speed))
                    {
                        attributes.speed = speed / 100;
                      //  Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"speed {attributes.speed}");
                    }

                    if (match.Groups["maxcombo"].Success && int.TryParse(match.Groups["maxcombo"].Value.Replace(".", "").Replace(",", ""), out int maxCombo))
                    {
                        attributes.Maxcombo = maxCombo / 100;
                      //  Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"maxcombo: {attributes.Maxcombo}");
                    }

                    if (match.Groups["pp"].Success && double.TryParse(match.Groups["pp"].Value.Replace(".", "").Replace(",", ""), out double pp))
                    {
                        attributes.pp = pp / 100;
                       // Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"pp: {attributes.pp}");
                    }

                    if (match.Groups["accuracy"].Success && double.TryParse(match.Groups["accuracy"].Value.Replace(".", "").Replace(",", ""), out double accuracy))
                    {
                        attributes.accuracy = accuracy / 100;
                       // Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"accuracy: {attributes.accuracy}");
                    }

                    if (match.Groups["aim"].Success && double.TryParse(match.Groups["aim"].Value.Replace(".", "").Replace(",", ""), out double aim))
                    {
                        attributes.aim = aim / 100;
                       // Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"aim: {attributes.aim}");
                    }

                    if (match.Groups["appraochrate"].Success && double.TryParse(match.Groups["appraochrate"].Value.Replace(".", "").Replace(",", ""), out double ar))
                    {
                        attributes.ar = ar / 100;
                       // Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"appraochrate: {attributes.ar}");
                    }

                    if (match.Groups["overalldifficulty"].Success && double.TryParse(match.Groups["overalldifficulty"].Value.Replace(".", "").Replace(",", ""), out double od))
                    {
                        attributes.od = od / 100;
                       // Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"overalldifficulty: {attributes.od}");
                    }

                    if (match.Groups["starrating"].Success && double.TryParse(match.Groups["starrating"].Value.Replace(".", "").Replace(",", ""), out double sr))
                    {
                        attributes.starrating = sr / 100;
                       // Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"starrating: {attributes.starrating}");
                    }

                }
            }
            return attributes;
        }

        private static string cmdOutput(string path, string command)
        {

            Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"Starting Shell: path:{path} cmd:{command}");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = path,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            Process process = new Process { StartInfo = psi };
            process.Start();

            StreamWriter sw = process.StandardInput;
            sw.WriteLine(command);
            sw.WriteLine("exit");

            string output = process.StandardOutput.ReadToEnd();
            //Console.WriteLine(output);
            process.WaitForExit();

            return output;
        }

        public static string CalculateGrade(int total300s, int total100s, int total50s, int misses)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"Calculating Grade: 300:{total300s} 100:{total100s} 50:{total50s} X:{misses}");
            try
            {
                int totalnotes = total300s + total100s + total50s + misses;

                double percent300 = (double)total300s / (double)totalnotes;
                double percent50 = (double)total50s / (double)totalnotes;

                if (total300s == totalnotes)
                {
                    return "SS";
                }
                else if (percent300 > 0.9 && percent50 < 0.1 && misses == 0)
                {
                    return "S";
                }
                else if ((percent300 > 0.8 && misses == 0) || percent300 > 0.9)
                {
                    return "A";
                }
                else if ((percent300 > 0.7 && misses == 0) || percent300 > 0.8)
                {
                    return "B";
                }
                else if (percent300 > 0.6)
                {
                    return "C";
                }

                return "D";
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"Error while Calculating Grade: {e.Message}");
                return "E";
            }

        }

        public static void Startshell(string command) {
            cmdOutput("", command);
        }

        public static void StartMissAnalyzer(int id)
        {

            Logger.Log(Logger.Severity.Info, Logger.Framework.Calculator, $"Starting OsuMissAnalyzer for scoreid: {id}");
            DatabaseController database = new DatabaseController();

            Score score = database.GetScore(id);

            string command = $"OsuMissAnalyzer.exe \"{Credentials.Instance.GetConfig().osufolder}\\Data\\r\\{score.Replay}\" \"{Credentials.Instance.GetConfig().songfolder}\\{score.FolderName}\"";
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"MissAnalyzer Command: {command}");
            Task.Run(() =>
            {
                string output = cmdOutput("OsuMissAnalyzer", command);
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Calculator, $"MissAnalyzer Output: {output}");
            });

        }

        public static string ModeConverter(int mode)
        {

            switch (mode)
            {
                case 0:
                    return "osu";

                case 1:
                    return "taiko";

                case 2:
                    return "catch";

                case 3:
                    return "mania";

                default:
                    return "osu";
            }
        }

        public static string ScreenConverter(int RawStatus)
        {
            switch (RawStatus)
            {
                case 0:
                    return "Mainmenu";
                case 1:
                    return "EditingMap";
                case 2:
                    return "Playing";
                case 3:
                    return "GameShutDown";
                case 4:
                    return "SongSelectEdit";
                case 5:
                    return "SongSelect";
                case 6:
                    return "Unknown";
                case 7:
                    return "ResultScreen";
                case 11:
                    return "MultiplayerRooms";
                case 12:
                    return "MultiPlayerRoom";
                case 15:
                    return "OsuDirect";
                case 16:
                    return "OffsetAssistent";
                case 19:
                    return "ProcessingBeatmaps";
                default:
                    return "Unknown";
            }
        }


    }

    public class PerfomanceAttributes
    {
        public double aim { get; set; }
        public double speed { get; set; }
        public double accuracy { get; set; }
        public double pp { get; set; }
        public double starrating { get; set; }
        public double ar { get; set; }
        public double od { get; set; }
        public int Maxcombo { get; set; }
        public string grade { get; set; } = "E";
    }


}
