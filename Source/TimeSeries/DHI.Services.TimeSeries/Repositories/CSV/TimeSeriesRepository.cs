namespace DHI.Services.TimeSeries.CSV
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using DHI.Services.TimeSeries;

    public class TimeSeriesRepository : BaseGroupedDiscreteTimeSeriesRepository<string, double>
    {
        private readonly Dictionary<string, KeyValuePair<DateTime, TimeSeriesData<double>>> _data = new Dictionary<string, KeyValuePair<DateTime, TimeSeriesData<double>>>();
        private readonly string _rootFolder;

        public TimeSeriesRepository(string rootFolder)
        {
            _rootFolder = rootFolder ?? throw new ArgumentNullException(nameof(rootFolder));
            _rootFolder = _rootFolder.EndsWith("\\") ? _rootFolder : _rootFolder + "\\";

            if (!Directory.Exists(_rootFolder))
            {
                throw new ArgumentException($"Folder '{rootFolder}' does not exist");
            }
        }

        public override bool ContainsGroup(string group, ClaimsPrincipal user = null)
        {
            return Directory.Exists(Path.Combine(_rootFolder, group));
        }

        public override IEnumerable<TimeSeries<string, double>> GetByGroup(string group, ClaimsPrincipal user = null)
        {
            var timeSeriesList = new List<TimeSeries<string, double>>();
            foreach (var fullName in GetFullNames(group))
            {
                timeSeriesList.Add(new TimeSeries<string, double>(fullName, fullName, group));
            }

            return timeSeriesList;
        }

        public override IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
        {
            var timeSeriesList = new List<string>();
            var csvFiles = Directory.GetFiles(Path.Combine(_rootFolder, group), "*.csv", SearchOption.AllDirectories);
            foreach (var filePath in csvFiles)
            {
                using var reader = new StreamReader(filePath);
                var items = reader.ReadLine().Split(';').ToList();
                items.RemoveAt(0);
                timeSeriesList.AddRange(items.Select(r => filePath.Replace(_rootFolder, "").Replace(@"\", "/") + ";" + r));
            }

            return timeSeriesList;
        }

        public override IEnumerable<TimeSeries<string, double>> GetAll(ClaimsPrincipal user = null)
        {
            return GetByGroup(string.Empty);
        }

        public override Maybe<TimeSeries<string, double>> Get(string id, ClaimsPrincipal user = null)
        {
            var group = id.Contains("/") ? id.Substring(id.LastIndexOf("/", StringComparison.Ordinal)) : string.Empty;
            return !Contains(id) ? Maybe.Empty<TimeSeries<string, double>>() : new TimeSeries<string, double>(id, id, group) { DataType = TimeSeriesDataType.Instantaneous }.ToMaybe();
        }

        public override bool Contains(string id, ClaimsPrincipal user = null)
        {
            var file = Path.Combine(_rootFolder, TimeSeriesId.Parse(id).RelativeFilePath);
            var item = TimeSeriesId.Parse(id).ObjId;
            if (!File.Exists(file))
            {
                return false;
            }

            using var reader = new StreamReader(file);
            return reader.ReadLine().Split(';').ToList().Contains(item);
        }

        public override Maybe<ITimeSeriesData<double>> GetValues(string id, ClaimsPrincipal user = null)
        {
            var values = new TimeSeriesData<double>();
            var file = Path.Combine(_rootFolder, TimeSeriesId.Parse(id).RelativeFilePath);
            if (!File.Exists(file))
            {
                return Maybe.Empty<ITimeSeriesData<double>>();
            }

            var item = TimeSeriesId.Parse(id).ObjId;

            var fileInfo = new FileInfo(file);
            if (_data.ContainsKey(id) && _data[id].Key == fileInfo.LastWriteTime)
            {
                return _data[id].Value.ToMaybe<ITimeSeriesData<double>>();
            }

            if (_data.ContainsKey(id) && _data[id].Key != fileInfo.LastWriteTime)
            {
                _data.Remove(id);
            }

            var lines = File.ReadAllLines(file, Encoding.Default).ToList();
            var header = lines[0].Split(';').ToList();
            var dateTimeFormat = header[0];
            var index = header.IndexOf(item);
            if (index == -1)
            {
                return Maybe.Empty<ITimeSeriesData<double>>();
            }

            for (var i = 1; i < lines.Count; i++)
            {
                try
                {
                    var parts = lines[i].Split(';').ToList();
                    var dateTime = DateTime.ParseExact(parts[0], dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);

                    if (double.TryParse(parts[index].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                    {
                        values.Append(dateTime, value);
                    }
                    else
                    {
                        values.Append(dateTime, null);
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception("Cannot parse line " + lines[i] + ". Exception: " + exception.Message);
                }
            }

            _data.Add(id, new KeyValuePair<DateTime, TimeSeriesData<double>>(fileInfo.LastWriteTime, values));

            return _data[id].Value.ToMaybe<ITimeSeriesData<double>>();
        }

        public override void SetValues(string id, ITimeSeriesData<double> data, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }
    }
}