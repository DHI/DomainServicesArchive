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

    public class UpdatableTimeSeriesRepository : BaseUpdatableTimeSeriesRepository<string, double>
    {
        private readonly Dictionary<string, Tuple<DateTime, SortedDictionary<DateTime, double?>>> _cache;
        private readonly string _rootFolder;

        public UpdatableTimeSeriesRepository(string rootFolder)
        {
            _rootFolder = rootFolder ?? throw new ArgumentNullException(nameof(rootFolder));
            _rootFolder = _rootFolder.EndsWith("\\") ? _rootFolder : _rootFolder + "\\";
            if (!Directory.Exists(_rootFolder))
            {
                throw new ArgumentException($"Folder '{rootFolder}' does not exist");
            }

            _cache = new Dictionary<string, Tuple<DateTime, SortedDictionary<DateTime, double?>>>();
        }

        public override void Add(TimeSeries<string, double> timeSeries, ClaimsPrincipal user = null)
        {
            var filePath = Path.Combine(_rootFolder, timeSeries.Id) + ".csv";
            var csv = timeSeries.Data.ToSortedSet().Select(point => string.Format(CultureInfo.InvariantCulture, "{0};{1}", point.DateTime, point.Value)).ToList();
            File.WriteAllLines(filePath, csv);
        }

        public override IEnumerable<TimeSeries<string, double>> GetAll(ClaimsPrincipal user = null)
        {
            var ids = Directory.GetFiles(Path.Combine(_rootFolder), "*.csv").Select(Path.GetFileNameWithoutExtension).ToList();
            return ids.Select(id => new TimeSeries<string, double>(id, id)).ToList();
        }

        public override Maybe<ITimeSeriesData<double>> GetValues(string id, ClaimsPrincipal user = null)
        {
            try
            {
                _RefreshCache(id);
                var dict = _cache[id].Item2;
                return new TimeSeriesData<double>(dict.Keys.ToList(), dict.Values.ToList()).ToMaybe<ITimeSeriesData<double>>();
            }
            catch
            {
                return Maybe.Empty<ITimeSeriesData<double>>();
            }
        }

        public override void Remove(string id, ClaimsPrincipal user = null)
        {
            if (_cache.ContainsKey(id))
            {
                _cache.Remove(id);
            }

            var filePath = Path.Combine(_rootFolder, id) + ".csv";
            File.Delete(filePath);
        }

        public override void SetValues(string id, ITimeSeriesData<double> data, ClaimsPrincipal user = null)
        {
            _RefreshCache(id);
            var sortedData = data.ToSortedDictionary();
            var timeSeries = _cache[id].Item2;
            foreach (var point in sortedData)
            {
                timeSeries.Add(point.Key, point.Value);
            }

            var filePath = Path.Combine(_rootFolder, id) + ".csv";
            var csv = timeSeries.Select(point => string.Format(CultureInfo.InvariantCulture, "{0};{1}", point.Key, point.Value)).ToList();
            File.WriteAllLines(filePath, csv);
        }

        public override void Update(TimeSeries<string, double> timeSeries, ClaimsPrincipal user = null)
        {
            Add(timeSeries);
        }

        public override void RemoveValues(string id, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public override void RemoveValues(string id, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        private void _RefreshCache(string id)
        {
            var values = new SortedDictionary<DateTime, double?>();
            var filePath = Path.Combine(_rootFolder, id) + ".csv";
            var fileInfo = new FileInfo(filePath);
            if (!_cache.ContainsKey(id) || _cache[id].Item1 != fileInfo.LastWriteTime)
            {
                if (_cache.ContainsKey(id) && _cache[id].Item1 != fileInfo.LastWriteTime)
                {
                    _cache.Remove(id);
                }

                var lines = File.ReadAllLines(filePath, Encoding.Default).ToList();
                foreach (var line in lines)
                {
                    var parts = line.Split(';').ToList();
                    var dateTime = DateTime.Parse(parts[0], CultureInfo.InvariantCulture);
                    var value = double.Parse(parts[1].Replace(",", "."), CultureInfo.InvariantCulture);
                    values.Add(dateTime, value);
                }

                _cache.Add(id, new Tuple<DateTime, SortedDictionary<DateTime, double?>>(fileInfo.LastWriteTime, values));
            }
        }
    }
}