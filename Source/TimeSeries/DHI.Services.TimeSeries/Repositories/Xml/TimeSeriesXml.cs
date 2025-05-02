namespace DHI.Services.TimeSeries.Xml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    internal class TimeSeriesXml
    {
        public string FilePath { get; }

        private Dictionary<string, string> Replacements { get; } = new Dictionary<string, string>();

        public TimeSeriesXmlConfiguration Configuration { get; }

        public TimeSeriesXml(string id, string xmlFilePath)
        {
            Guard.Against.NullOrEmpty(id, nameof(id));
            var tokens = id.Split(';');
            if (tokens.Length == 0)
            {
                throw new ArgumentException("Could not parse time series id string '" + id + "'. A times series id has the following format:\n\n" +
                                            "<path>;Key1=Value1\n\n" +
                                            "Time series id example: data/test.xml;[name1]=value1", nameof(id));
            }

            for (int i = 1; i < tokens.Length; i++)
            {
                Replacements.Add(tokens[i].Split('=')[0], tokens[i].Split('=')[1]);
            }

            if (File.Exists(xmlFilePath))
            {
                var json = File.ReadAllText(xmlFilePath);
                var options = new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,

                };
                Configuration = JsonSerializer.Deserialize<TimeSeriesXmlConfiguration>(json, options);

                //Configuration = JsonConvert.DeserializeObject<TimeSeriesXmlConfiguration>(File.ReadAllText(xmlFilePath));
            }
            else
            {
                throw new ArgumentException($"The file {xmlFilePath} does not exist");
            }

            if (File.Exists(tokens[0]))
            {
                FilePath = tokens[0];
            }
            else if (!string.IsNullOrEmpty(Configuration.RootFilePath) && File.Exists(Path.Combine(Configuration.RootFilePath, tokens[0])))
            {
                FilePath = Path.Combine(Configuration.RootFilePath, tokens[0]);
            }
            else if (File.Exists(Path.Combine(Path.GetDirectoryName(xmlFilePath), tokens[0])))
            {
                FilePath = Path.Combine(Path.GetDirectoryName(xmlFilePath), tokens[0]);
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