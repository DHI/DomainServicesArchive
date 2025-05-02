namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public class InMemoryTimeSeriesRepository<TId, TValue> : BaseUpdatableTimeSeriesRepository<TId, TValue> where TValue : struct, IComparable<TValue>
    {
        private readonly Dictionary<TId, TimeSeries<TId, TValue>> _repository;

        public InMemoryTimeSeriesRepository()
        {
            _repository = new Dictionary<TId, TimeSeries<TId, TValue>>();
        }

        public InMemoryTimeSeriesRepository(IEnumerable<TimeSeries<TId, TValue>> timeSeriesList) : this()
        {
            foreach (var timeSeries in timeSeriesList)
            {
                Add(timeSeries);
            }
        }

        public override void Add(TimeSeries<TId, TValue> timeSeries, ClaimsPrincipal user = null)
        {
            var ts = CreateTimeSeries(timeSeries, timeSeries.Data);
            _repository.Add(ts.Id, ts);
        }

        /// <summary>
        ///     Removes the time series with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="claimsPrincipal">The claims principal.</param>
        public override void Remove(TId id, ClaimsPrincipal claimsPrincipal = null)
        {
            if (Contains(id))
            {
                _repository.Remove(id);
            }
        }

        public override Maybe<TimeSeries<TId, TValue>> Get(TId id, ClaimsPrincipal user = null)
        {
            if (Contains(id))
            {
                return new Maybe<TimeSeries<TId, TValue>>(CreateTimeSeries(_repository[id]));
            }

            return Maybe.Empty<TimeSeries<TId, TValue>>();
        }

        public override IEnumerable<TimeSeries<TId, TValue>> GetAll(ClaimsPrincipal claimsPrincipal = null)
        {
            foreach (var timeSeries in _repository.Values)
            {
                yield return CreateTimeSeries(timeSeries);
            }
        }

        public IEnumerable<TimeSeries<TId, TValue>> GetAllWithValues()
        {
            foreach (var timeSeries in _repository.Values)
            {
                yield return CreateTimeSeries(timeSeries, timeSeries.Data);
            }
        }

        public override Maybe<ITimeSeriesData<TValue>> GetValues(TId id, ClaimsPrincipal user = null)
        {
            if (!Contains(id))
            {
                return Maybe.Empty<ITimeSeriesData<TValue>>();
            }

            var sortedData = _repository[id].Data.ToSortedDictionary();
            ITimeSeriesData<TValue> tsDataSorted = new TimeSeriesData<TValue>(sortedData.Keys.ToList(), sortedData.Values.ToList());
            return tsDataSorted.ToMaybe();
        }

        public override Maybe<ITimeSeriesData<TValue>> GetValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            if (!Contains(id))
            {
                return Maybe.Empty<ITimeSeriesData<TValue>>();
            }

            var sortedData = _repository[id].Data.ToSortedDictionary();
            var sortedDataWithinTimePeriod = sortedData.Where(d => d.Key >= from && d.Key <= to).ToList();

            if (!sortedDataWithinTimePeriod.Any())
            {
                return Maybe.Empty<ITimeSeriesData<TValue>>();
            }

            ITimeSeriesData<TValue> tsDataSorted = new TimeSeriesData<TValue>();
            foreach (var dataPoint in sortedDataWithinTimePeriod)
            {
                tsDataSorted.Append(dataPoint.Key, dataPoint.Value);
            }

            return tsDataSorted.ToMaybe();
        }

        public override void SetValues(TId id, ITimeSeriesData<TValue> data, ClaimsPrincipal user = null)
        {
            // New values are appended and not chronologically sorted.
            // However, GetValues() sorts by time before returning.
            var timeSeries = _repository[id];

            foreach (var dataPoint in data.DateTimes.Zip(data.Values, (t, v) => new {t, v}))
            {
                var dateTimeIndex = timeSeries.Data.DateTimes.IndexOf(dataPoint.t);
                if (dateTimeIndex > -1)
                {
                    timeSeries.Data.Values[dateTimeIndex] = dataPoint.v;
                }
                else
                {
                    timeSeries.Data.Append(dataPoint.t, dataPoint.v);
                }
            }
        }

        public override void Update(TimeSeries<TId, TValue> timeSeries, ClaimsPrincipal user = null)
        {
            var existingTimeSeriesData = GetInternal(timeSeries.Id).Data;
            _repository[timeSeries.Id] = CreateTimeSeries(timeSeries, existingTimeSeriesData);
            SetValues(timeSeries.Id, timeSeries.Data);
        }

        public override void RemoveValues(TId id, ClaimsPrincipal user = null)
        {
            var timeSeries = GetInternal(id);
            timeSeries.Data.DateTimes.Clear();
            timeSeries.Data.Values.Clear();
        }

        public override void RemoveValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            var timeSeries = GetInternal(id);

            var indices = timeSeries.Data.DateTimes.Select((dateTime, i) => new {dateTime, i}).Where(x => x.dateTime > from && x.dateTime < to).Select(x => x.i).ToArray();
            foreach (var index in indices.OrderByDescending(i => i))
            {
                timeSeries.Data.DateTimes.RemoveAt(index);
                timeSeries.Data.Values.RemoveAt(index);
            }
        }

        public override int Count(ClaimsPrincipal user = null)
        {
            return _repository.Count;
        }

        public override bool Contains(TId id, ClaimsPrincipal user = null)
        {
            return _repository.ContainsKey(id);
        }

        public override IEnumerable<TId> GetIds(ClaimsPrincipal user = null)
        {
            return _repository.Keys;
        }

        public override Maybe<DataPoint<TValue>> GetValue(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            return Contains(id) ? GetInternal(id).Data.Get(dateTime) : Maybe.Empty<DataPoint<TValue>>();
        }

        private TimeSeries<TId, TValue> GetInternal(TId id)
        {
            // Return actual stored time series object
            if (_repository.TryGetValue(id, out var timeSeries))
            {
                return timeSeries;
            }

            throw new Exception($"Time series with id '{id}' does not exist.");
        }

        private static TimeSeries<TId, TValue> CreateTimeSeries(TimeSeries<TId, TValue> timeSeries, ITimeSeriesData<TValue> newData = null)
        {
            var newTimeSeriesData = CreateTimeSeriesData(newData);

            var timeSeriesWithNewData = new TimeSeries<TId, TValue>(timeSeries.Id, timeSeries.Name, timeSeries.Group, newTimeSeriesData)
            {
                DataType = timeSeries.DataType,
                Dimension = timeSeries.Dimension,
                Quantity = timeSeries.Quantity,
                Unit = timeSeries.Unit
            };

            foreach (var metaData in timeSeries.Metadata)
            {
                timeSeriesWithNewData.Metadata.Add(metaData.Key, metaData.Value);
            }

            return timeSeriesWithNewData;
        }

        private static TimeSeriesData<TValue> CreateTimeSeriesData(ITimeSeriesData<TValue> data)
        {
            return data == null ? null : new TimeSeriesData<TValue>(data.DateTimes.ToList(), data.Values.ToList());
        }
    }
}