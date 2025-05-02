namespace DHI.Services.TimeSeries.Json
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Text.Json;

    internal class TimeSeriesJson
    {
        public string FilePath { get; }

        private Dictionary<string, string> Replacements { get; } = new Dictionary<string, string>();

        public TimeSeriesJsonConfiguration Configuration { get; }

        public TimeSeriesJson(string id, string jsonFilePath)
        {
            Guard.Against.NullOrEmpty(id, nameof(id));
            var tokens = id.Split(';');
            if (tokens.Length == 0)
            {
                throw new ArgumentException("Could not parse time series id string '" + id + "'. A times series id has the following format:\n\n" +
                                            "<path>;Key1=Value1\n\n" +
                                            "Time series id example: data/test.json;[name1]=value1", nameof(id));
            }

            for (int i = 1; i < tokens.Length; i++)
            {
                Replacements.Add(tokens[i].Split('=')[0], tokens[i].Split('=')[1]);
            }

            if (File.Exists(jsonFilePath))
            {
                //Configuration = JsonConvert.DeserializeObject<TimeSeriesJsonConfiguration>(File.ReadAllText(jsonFilePath));
                Configuration = JsonSerializer.Deserialize<TimeSeriesJsonConfiguration>(File.ReadAllText(jsonFilePath), new JsonSerializerOptions() {
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                });
            }
            else
            {
                throw new ArgumentException($"The file {jsonFilePath} does not exist");
            }

            if (File.Exists(tokens[0]))
            {
                FilePath = tokens[0];
            }
            else if (!string.IsNullOrEmpty(Configuration.RootFilePath) && File.Exists(Path.Combine(Configuration.RootFilePath, tokens[0])))
            {
                FilePath = Path.Combine(Configuration.RootFilePath, tokens[0]);
            }
            else if (File.Exists(Path.Combine(Path.GetDirectoryName(jsonFilePath), tokens[0])))
            {
                FilePath = Path.Combine(Path.GetDirectoryName(jsonFilePath), tokens[0]);
            }
            else
            {
                throw new ArgumentException($"The file {tokens[0]} does not exist");
            }
        }

        public string DoReplacements(string input)
        {
            foreach (var replacement in Replacements)
            {
                input = input.Replace(replacement.Key, replacement.Value);
            }
            return input;
        }
    }
}