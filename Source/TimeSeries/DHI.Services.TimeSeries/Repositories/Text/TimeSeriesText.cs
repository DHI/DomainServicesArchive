namespace DHI.Services.TimeSeries.Text
{

    using System;
    using System.IO;
    using System.Text.Json;

    internal class TimeSeriesText
    {
        public string FilePath { get; }

        public string Id { get; }

        public TimeSeriesTextConfiguration Configuration { get; }

        public TimeSeriesText(string id, string jsonFilePath)
        {
            Guard.Against.NullOrEmpty(id, nameof(id));
            var tokens = id.Split(';');
            if (tokens.Length != 2)
            {
                throw new ArgumentException("Could not parse time series id string '" + id + "'. A times series id has the following format:\n\n" +
                                            "<path>;<item>\n\n" +
                                            "<path> is the relative file path - e.g. data/test.csv\n" +
                                            "<item> is the item name - e.g. WaterLevel\n\n" +
                                            "Time series id example: data/test.csv;WaterLevel", nameof(id));
            }

            Id = tokens[1];

            if (File.Exists(jsonFilePath))
            {
                //Configuration = JsonConvert.DeserializeObject<TimeSeriesTextConfiguration>(File.ReadAllText(jsonFilePath));
                var json = File.ReadAllText(jsonFilePath);
                var options = new JsonSerializerOptions()
                {
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,

                };
                //options.Converters.Add(new TimeSeriesTextConfigurationConverter());
                Configuration = JsonSerializer.Deserialize<TimeSeriesTextConfiguration>(json, options);
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
    }
}