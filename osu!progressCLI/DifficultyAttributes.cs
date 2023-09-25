using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace osu_progressCLI
{
    internal class DifficultyAttributes
    {
        private const string CalculatorPath = "osu-tools\\PerformanceCalculator";

        public static PerfomanceAttributes CalculatePP(string folderName, string fileName, int mods, int missCount, int mehCount, int goodCount, int perfectcount, int combo)
        {
            PerfomanceAttributes perfomanceAttributes = new PerfomanceAttributes();

            string fullPath = Path.Combine(Credentials.Instance.GetConfig().songfolder, folderName, fileName);
            string command = $"dotnet run -- simulate osu \"{fullPath}\" -c {combo} -X {missCount} -M {mehCount} -G {goodCount}";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = CalculatorPath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = psi };
            process.Start();

            StreamWriter sw = process.StandardInput;
            Console.WriteLine(command);
            sw.WriteLine(command);
            sw.WriteLine("exit");

            string output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            try
            {
                perfomanceAttributes.pp = ParsePPFromOutput(output);
                perfomanceAttributes.aim = ParseAimFromOutput(output);
                perfomanceAttributes.speed = ParseSpeedFromOutput(output);
                perfomanceAttributes.accuracy = ParseAccuracyFromOutput(output);
                perfomanceAttributes.grade = CalculateGrade(perfectcount, goodCount, mehCount, missCount);

                Console.WriteLine(perfomanceAttributes.pp + " | " + perfomanceAttributes.aim + " | " + perfomanceAttributes.speed + " | " + perfomanceAttributes.accuracy + " | " + perfomanceAttributes.grade);
                return perfomanceAttributes;
            }
            catch (InvalidOperationException)
            {
                return perfomanceAttributes;
            }
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
    }

    public class PerfomanceAttributes {
       public double aim { get; set; }
       public double speed { get;set; }
       public double accuracy { get; set; }
       public double pp { get; set; } 
        public string grade { get; set; }
    }
}
