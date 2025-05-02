namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Save and load time series including its equidistant data to text format.
    /// </summary>
    public class EquidistantTimeSeriesSerializer
    {
        protected readonly StringBuilder Content;
        protected readonly CultureInfo Culture;
        protected readonly string Delimiter;
        protected readonly Dictionary<string, int> Indices;
        protected readonly string OutputNumberFormat;

        /// <summary>
        ///     Set serializer destination file path and options.
        /// </summary>
        /// <param name="filePath">Default = temp.txt in OS default temporary directory.</param>
        public EquidistantTimeSeriesSerializer(string filePath = null)
        {
            FilePath = filePath ?? GetTempFilePath();
            Content = new StringBuilder();
            Delimiter = ";";
            Culture = CultureInfo.InvariantCulture;
            OutputNumberFormat = "0.#####";
            Indices = new Dictionary<string, int>
            {
                {"time", 0},
                {"timestep", 1},
                {"id", 2},
                {"quantity", 3},
                {"unit", 4},
                {"values", 5}
            };
        }

        public string FilePath { get; }

        /// <summary>
        ///     Get file path to a log file in the default OS directory.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="timeStampFileName">Add current time to filename.</param>
        public static string GetTempFilePath(string fileName = "temp", bool timeStampFileName = false)
        {
            if (timeStampFileName)
            {
                fileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}";
            }

            return Path.Combine(Path.GetTempPath(), fileName + ".txt");
        }

        /// <summary>
        ///     Write all equidistant time series and data to text file.
        /// </summary>
        /// <param name="timeSeriesList"></param>
        public void Serialize(IEnumerable<TimeSeries<string, double>> timeSeriesList)
        {
            foreach (var timeSeries in timeSeriesList)
            {
                Content.AppendLine(Parse(timeSeries));
            }

            File.AppendAllText(FilePath, Content.ToString());
            Content.Clear();
        }

        /// <summary>
        ///     Read serialized equidistant time series and data from text file.
        /// </summary>
        /// <param name="filePath"></param>
        public IEnumerable<TimeSeries<string, double>> Deserialize(string filePath)
        {
            Validate(filePath);

            foreach (var line in File.ReadAllLines(filePath, Encoding.Default))
            {
                yield return Parse(line);
            }
        }

        protected virtual string Parse(TimeSeries<string, double> timeSeries)
        {
            Validate(timeSeries);

            if (!timeSeries.HasValues)
            {
                return string.Empty;
            }

            var startTime = timeSeries.Data.DateTimes[0].ToString(Culture.DateTimeFormat);
            var timeStep = GetFirstTimeStep(timeSeries).ToString("c");

            return string.Join(Delimiter,
                startTime,
                timeStep,
                timeSeries.Id,
                timeSeries.Quantity,
                timeSeries.Unit,
                ParseValues(timeSeries.Data));
        }

        private static void Validate(TimeSeries<string, double> timeSeries)
        {
            if (!timeSeries.IsEquidistant())
            {
                throw new ArgumentException($"Time series {timeSeries} is not equidistant.");
            }
        }

        private string ParseValues(ITimeSeriesData<double> data)
        {
            var values = data.Values.Select(v => v?.ToString(OutputNumberFormat));
            return string.Join(Delimiter, values);
        }

        private static TimeSpan GetFirstTimeStep(TimeSeries<string, double> timeSeries)
        {
            var time = timeSeries.Data.DateTimes;
            if (time.Count > 1)
            {
                return time[1] - time[0];
            }

            return TimeSpan.Zero;
        }

        private static void Validate(string filepath)
        {
            filepath = filepath ?? throw new ArgumentNullException(nameof(filepath));

            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException(nameof(filepath));
            }
        }

        private TimeSeries<string, double> Parse(string line)
        {
            var data = line.Split(Delimiter[0]);

            if (data.Length < Indices.Values.Max() + 1)
            {
                throw new ArgumentOutOfRangeException(nameof(line));
            }

            var time = Convert.ToDateTime(data[Indices["time"]], Culture.DateTimeFormat);
            var timeStep = TimeSpan.Parse(data[Indices["timestep"]]);

            var tsData = new TimeSeriesData<double>();
            foreach (var value in data.Skip(Indices["values"]))
            {
                tsData.Append(time, Convert.ToDouble(value));
                time = time.Add(timeStep);
            }

            var id = data[Indices["id"]];
            var name = FullName.Parse(id);

            return new TimeSeries<string, double>(id, name.Name, name.Group,
                "",
                data[Indices["quantity"]],
                data[Indices["unit"]],
                tsData);
        }
    }
}