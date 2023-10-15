using osu1progressbar.Game.Database;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace osu_progressCLI
{
    internal class DifficultyAttributes
    {

        public static double CalculateFcWithAcc(string folderName, string fileName, double Acc = 100, int mods = 0, int mode = 0) {
            double pp = 0;


            string fullPath = Path.Combine(Credentials.Instance.GetConfig().songfolder, folderName, fileName);
            string command = $"dotnet PerformanceCalculator.dll simulate {ModeConverter(mode)} \"{fullPath}\" --accuracy {Acc} --percent-combo 100 {ModParser.PPCalcMods(mods)}";

            string output = cmdOutput("osu-tools", command);
            try {
                pp = ParsePPFromOutput(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return pp;
        }

        public static double CalculateFcWithAcc(int id, double Acc = 100, int mods = 0, int mode = 0)
        {
            double pp = 0;


            string command = $"dotnet PerformanceCalculator.dll simulate {ModeConverter(mode)} {id} --accuracy {Acc} --percent-combo 100 {ModParser.PPCalcMods(mods)}";

            string output = cmdOutput("osu-tools", command);
            try
            {
                pp = ParsePPFromOutput(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return pp;
        }

        //add mods (need to be parsed from bit format to string 
        public static PerfomanceAttributes CalculatePP(string folderName, string fileName, int mods, int missCount, int mehCount, int goodCount, int perfectcount, int combo, int mode = 0)
        {
            PerfomanceAttributes perfomanceAttributes = new PerfomanceAttributes();

            string fullPath = Path.Combine(Credentials.Instance.GetConfig().songfolder, folderName, fileName);
            string command = $"dotnet PerformanceCalculator.dll simulate {ModeConverter(mode)} \"{fullPath}\" --combo {combo} --misses {missCount} --mehs {mehCount} --goods {goodCount} {ModParser.PPCalcMods(mods)}";

            string output = cmdOutput("osu-tools",command);

            try
            {
                perfomanceAttributes.pp = ParsePPFromOutput(output);
                perfomanceAttributes.aim = ParseAimFromOutput(output);
                perfomanceAttributes.speed = ParseSpeedFromOutput(output);
                perfomanceAttributes.accuracy = ParseAccuracyFromOutput(output);
                perfomanceAttributes.starrating = ParseStarRatingFromOutput(output);
                perfomanceAttributes.Maxcombo = ParseMaxComboFromOutput(output);
                perfomanceAttributes.grade = CalculateGrade(perfectcount, goodCount, mehCount, missCount);

                return perfomanceAttributes;
            }
            catch (InvalidOperationException)
            {
                return perfomanceAttributes;
            }
        }



        private static string cmdOutput(string path,string command) {

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

        private static double ParseAimFromOutput(string output)
        {
            string pattern = @"aim\s+:\s+(?<aim>[\d.]+)";

            Match match = Regex.Match(output, pattern);

            if (match.Success)
            {

                return double.Parse(match.Groups["aim"].Value) / 100; // Parse as double and 
            }
            else
            {

                throw new InvalidOperationException("aim value not found in the output.");
            }
        }

        private static double ParseSpeedFromOutput(string output)
        {
            string pattern = @"speed\s+:\s+(?<speed>[\d.]+)";

            Match match = Regex.Match(output, pattern);

            if (match.Success)
            {

                return double.Parse(match.Groups["speed"].Value) / 100; // Parse as double and 
            }
            else
            {

                throw new InvalidOperationException("speed value not found in the output.");
            }
        }

        private static double ParseAccuracyFromOutput(string output)
        {
            string pattern = @"accuracy\s+:\s+(?<accuracy>[\d.]+)";

            MatchCollection match = Regex.Matches(output, pattern);

            if (match.Count > 1)
            {

                return double.Parse(match[1].Groups["accuracy"].Value) / 100; // Parse as double and 
            }
            else
            {

                throw new InvalidOperationException("accuracy value not found in the output.");
            }
        }

        private static double ParseStarRatingFromOutput(string output)
        {
            string pattern = @"star rating\s+:\s+(?<starrating>[\d.]+)";

            Match match = Regex.Match(output, pattern);

            if (match.Success)
            {

                return double.Parse(match.Groups["starrating"].Value) / 100; // Parse as double and 
            }
            else
            {

                throw new InvalidOperationException("starrating value not found in the output.");
            }
        }

        private static int ParseMaxComboFromOutput(string output)
        {
            string pattern = @"max combo\s+:\s+(?<maxcombo>[\d.]+)";

           Match match = Regex.Match(output, pattern);

            if (match.Success)
            {
                return (int)Math.Round(double.Parse(match.Groups["maxcombo"].Value) / 100 ); // Parse as double and 
            }
            else
            {

                throw new InvalidOperationException("max combo value not found in the output.");
            }
        }

        private static double ParsePPFromOutput(string output)
        {
            string pattern = @"pp\s+:\s+(?<pp>[\d.]+)";

            Match match = Regex.Match(output, pattern);

            if (match.Success)
            {

                return double.Parse(match.Groups["pp"].Value) / 100; // Parse as double and 
            }
            else
            {

                throw new InvalidOperationException("PP value not found in the output.");
            }
        }


        private static string CalculateGrade(int total300s, int total100s, int total50s, int misses)
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
        public string grade { get; set; }
    }


}
