using osu1progressbar.Game.Database;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace osu_progressCLI
{
    internal class DifficultyAttributes
    {

        public static double CalculateFcWithAcc(string folderName, string fileName, double Acc = 100, int mods = 0, int mode = 0) {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Calculating PP: {Acc}, {mods}, {mode}");

            string fullPath = Path.Combine(Credentials.Instance.GetConfig().songfolder, folderName, fileName);

            string command = $"dotnet PerformanceCalculator.dll simulate {ModeConverter(mode)} \"{fullPath}\" --accuracy {Acc} --percent-combo 100 {ModParser.PPCalcMods(mods)}";

            string output = cmdOutput("osu-tools", command);
            
            PerfomanceAttributes attributes = ParseOutput(output);

            return attributes.pp;
        }

        public static double CalculateFcWithAcc(int id, double Acc = 100, int mods = 0, int mode = 0)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Calculating PP from ID:{id}, Acc: {Acc}, Mods: {mods}, Mode: {ModeConverter(mode)}({mode})");
            string command = $"dotnet PerformanceCalculator.dll simulate {ModeConverter(mode)} {id} --accuracy {Acc} --percent-combo 100 {ModParser.PPCalcMods(mods)}";

            string output = cmdOutput("osu-tools", command);

            PerfomanceAttributes attributes = ParseOutput(output);

            return attributes.pp;
        }

        public static PerfomanceAttributes CalculatePP(string folderName, string fileName, int mods, int missCount, int mehCount, int goodCount, int perfectcount, int combo, int mode = 0)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Calculating PP: Miss:{missCount}, Meh:{mehCount}, Good:{goodCount}, Perfect:{perfectcount}, Combo:{combo} Mods: {mods}, Mode: {ModeConverter(mode)}({mode})");
            PerfomanceAttributes perfomanceAttributes = new PerfomanceAttributes();

            string fullPath = Path.Combine(Credentials.Instance.GetConfig().songfolder, folderName, fileName);
            string command = $"dotnet PerformanceCalculator.dll simulate {ModeConverter(mode)} \"{fullPath}\" --combo {combo} --misses {missCount} --mehs {mehCount} --goods {goodCount} {ModParser.PPCalcMods(mods)}";

            string output = cmdOutput("osu-tools",command);

            perfomanceAttributes = ParseOutput(output);
            perfomanceAttributes.grade = CalculateGrade(perfectcount, goodCount, mehCount, missCount);

            return perfomanceAttributes;
        }

        public static PerfomanceAttributes CalculatePP(int Beatmapid, int mods, int missCount, int mehCount, int goodCount, int perfectcount, int combo, int mode = 0)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Calculating PP from id: ID:{Beatmapid} Miss:{missCount}, Meh:{mehCount}, Good:{goodCount}, Perfect:{perfectcount}, Combo:{combo} Mods: {mods}, Mode: {ModeConverter(mode)}({mode})");

            PerfomanceAttributes perfomanceAttributes = new PerfomanceAttributes();

            string command = $"dotnet PerformanceCalculator.dll simulate {ModeConverter(mode)} {Beatmapid} --combo {combo} --misses {missCount} --mehs {mehCount} --goods {goodCount} {ModParser.PPCalcMods(mods)}";

            string output = cmdOutput("osu-tools", command);
            
            perfomanceAttributes = ParseOutput(output);
            perfomanceAttributes.grade = CalculateGrade(perfectcount, goodCount, mehCount, missCount);

            return perfomanceAttributes;
        }

        private static PerfomanceAttributes ParseOutput(string output)
        {
            string pattern = @"star rating\s+:\s+(?<starrating>[\d.]+)|pp\s+:\s+(?<pp>[\d.]+)|max combo\s+:\s+(?<maxcombo>[\d.]+)|accuracy\s+:\s+(?<accuracy>[\d.]+)|speed\s+:\s+(?<speed>[\d.]+)|aim\s+:\s+(?<aim>[\d.]+)";

            Regex regex = new Regex(pattern);

            MatchCollection matches = regex.Matches(output);

            PerfomanceAttributes attributes = new PerfomanceAttributes();

            foreach (Match match in matches)
            {
                if (match.Success)
                {

                    if (match.Groups["speed"].Success)
                    {
                        attributes.speed = double.Parse(match.Groups["speed"].Value) / 100;
                        Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"speed {attributes.speed}");
                    }
                    if (match.Groups["maxcombo"].Success)
                    {
                        attributes.Maxcombo = int.Parse(double.Parse(match.Groups["maxcombo"].Value).ToString()) / 100;
                        Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"maxcombo: {attributes.Maxcombo}");
                    }
                    if (match.Groups["pp"].Success)
                    {
                        attributes.pp = double.Parse(match.Groups["pp"].Value) / 100;
                        Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"pp: {attributes.pp}") ;
                    }
                    if (match.Groups["accuracy"].Success)
                    {
                        attributes.accuracy = double.Parse(match.Groups["accuracy"].Value) / 100;
                        Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"accuracy: {attributes.accuracy}");
                    }
                    if (match.Groups["aim"].Success)
                    {
                        attributes.aim = double.Parse(match.Groups["aim"].Value) / 100;
                        Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"aim: {attributes.aim}");
                    }
                }
            }
            return attributes;
        }

        private static string cmdOutput(string path,string command) {

            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Starting Shell: path:{path} cmd:{command}");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = path,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
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

        private static string CalculateGrade(int total300s, int total100s, int total50s, int misses)
        {
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Calculating Grade: 300:{total300s} 100:{total100s} 50:{total50s} X:{misses}");
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
                Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"Error while Calculating Grade: {e.Message}");
                return "E";        
            }
            
        }

        public static void StartMissAnalyzer(int id) {

            Logger.Log(Logger.Severity.Info, Logger.Framework.Misc, $"Starting OsuMissAnalyzer for scoreid: {id}");
            DatabaseController database = new DatabaseController();

            Dictionary<string, object> score = database.GetScore(id);

            string command = $"OsuMissAnalyzer.exe \"{Credentials.Instance.GetConfig().osufolder}\\Data\\r\\{score["Replay"]}\" \"{Credentials.Instance.GetConfig().songfolder}\\{score["Foldername"]}\"";
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"MissAnalyzer Command: {command}");

            string output = cmdOutput("OsuMissAnalyzer", command);
            Logger.Log(Logger.Severity.Debug, Logger.Framework.Misc, $"MissAnalyzer Output: {output}");

        }

        public static string ModeConverter(int mode) { 
        
            switch (mode)
            { 
                case 0 :
                    return "osu";
                
                case 1 :
                    return "taiko";

                case 2 :
                    return "catch";

                case 3 :
                    return "mania";

                default :
                    return "osu";
            }
        }
    }

    public class PerfomanceAttributes {
       public double aim { get; set; }
       public double speed { get;set; }
       public double accuracy { get; set; }
       public double pp { get; set; }
        public double starrating { get; set; }
        public int Maxcombo { get; set; }
        public string grade { get; set; } = "E";
    }


}
