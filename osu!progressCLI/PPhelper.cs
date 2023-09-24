using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace osu_progressCLI
{
    internal class PPhelper
    {
        private const string CalculatorPath = "osu-tools\\PerformanceCalculator";

        public static double CalculatePP(string folderName, string fileName, int mods, int missCount, int mehCount, int goodCount, int combo)
        {

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
                double pp = ParsePPFromOutput(output);
                return pp;
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        }

        private static double ParsePPFromOutput(string output)
        {
            // Define a regular expression pattern to extract the "pp" value
            string pattern = @"pp\s+:\s+(?<pp>[\d.]+)";

            // Use Regex to match the pattern in the output
            Match match = Regex.Match(output, pattern);

            if (match.Success)
            {
                 
                return double.Parse(match.Groups["pp"].Value)/100; // Parse as double and 
            }
            else
            {
                
                throw new InvalidOperationException("PP value not found in the output.");
            }
        }
    }
}
