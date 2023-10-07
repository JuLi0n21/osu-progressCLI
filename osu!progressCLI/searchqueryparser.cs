using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class QueryParser
{
    public List<string> SearchTerms { get; } = new List<string>();
    public Dictionary<string, List<string>> QueryParams { get; } = new Dictionary<string, List<string>>();
    public List<string> ParametersWithoutOperators { get; } = new List<string>();

    public void Parse(string queryString)
    {
        string[] parts = queryString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        List<string> parameters = new List<string>();

        foreach (string part in parts)
        {
            Match match = Regex.Match(part, @"^(?<name>[a-zA-Z]+)(?<operator>[<=>]=?)(?<value>[0-9.]+)$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                parameters.Add(part);
            }
            else
            {
                SearchTerms.Add(part);
            }
        }

        foreach (string param in parameters)
        {
            Match match = Regex.Match(param, @"^(?<name>[a-zA-Z]+)(?<operator>[<=>]=?)(?<value>[0-9.]+)$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string paramName = match.Groups["name"].Value.ToLower();
                string paramOperator = match.Groups["operator"].Value;
                string paramValue = match.Groups["value"].Value;

                if (!QueryParams.ContainsKey(paramName))
                {
                    QueryParams[paramName] = new List<string>();
                }

                QueryParams[paramName].Add($"{paramOperator} {paramValue}");

                if (QueryParams[paramName].Count > 2)
                {
                    QueryParams[paramName].RemoveAt(0);
                }
            }
            else
            {
                ParametersWithoutOperators.Add(param);
            }
        }
    }
    public static string Filter(string queryString)
    {
        //string queryString = " Ichinose cs<=4 sadadfsasf cs>=2 cs<=4 hp>=6";

        QueryParser parser = new QueryParser();
        parser.Parse(queryString);

        StringBuilder commandBuilder = new StringBuilder();

        if (parser.SearchTerms.Count > 0)
        {
            commandBuilder.Append("AND (");
            foreach (string term in parser.SearchTerms)
            {
                commandBuilder.Append($"Osufilename LIKE '%{term}%' OR ");
            }

            // Remove the last " OR " from the StringBuilder
            commandBuilder.Remove(commandBuilder.Length - 4, 4);
            commandBuilder.Append(") ");
        }

        foreach (var kvp in parser.QueryParams)
        {
            foreach (string kv in kvp.Value)
            {
                commandBuilder.Append($"AND {kvp.Key} {kv} ");
            }
        }

        // Print the generated SQL query
       // Console.WriteLine(commandBuilder.ToString());
        return commandBuilder.ToString();
    }

}
