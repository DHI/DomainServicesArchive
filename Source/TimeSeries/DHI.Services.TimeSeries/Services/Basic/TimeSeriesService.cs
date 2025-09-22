namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Basic Time Series Service.
    /// </summary>
    /// <typeparam name="TId">The type of the time series identifier.</typeparam>
    /// <typeparam name="TValue">The type of the time series value.</typeparam>
    public class TimeSeriesService<TId, TValue> : CoreTimeSeriesService<TId, TValue>, ITimeSeriesService<TId, TValue> where TValue : struct, IComparable<TValue>
    {
        private readonly ITimeSeriesRepository<TId, TValue> _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesService{TId, TValue}" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public TimeSeriesService(ITimeSeriesRepository<TId, TValue> repository)
            : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesService{TId, TValue}" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="logger">The logger.</param>
        public TimeSeriesService(ITimeSeriesRepository<TId, TValue> repository, ILogger logger)
            : base(repository, logger)
        {
            _repository = repository;
        }

        /// <summary>
        ///     Gets a time series with values in the given time interval.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>TimeSeries&lt;TId, TValue&gt;.</returns>
        public TimeSeries<TId, TValue> GetWithValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            if (from >= to)
            {
                throw new ArgumentException($"From-DateTime '{from}' must be less than To-DateTime '{to}'.");
            }

            var maybe = _repository.GetWithValues(id, from, to, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"The time series with id '{id}' could not be extracted.");
            }

            return maybe.Value;
        }

        /// <summary>
        ///     Gets a time series with values in the given time interval.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>TimeSeries&lt;TId, TValue&gt;.</returns>
        public TimeSeries<TId, TValue> GetWithValues(TId id, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            Maybe<TimeSeries<TId, TValue>> maybe;
            if (from == null && to == null)
            {
                maybe = _repository.GetWithValues(id, user);
                if (!maybe.HasValue)
                {
                    throw new KeyNotFoundException($"The time series with id '{id}' could not be extracted.");
                }

                return maybe.Value;
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            if (fromDateTime >= toDateTime)
            {
                throw new ArgumentException($"From-DateTime '{fromDateTime}' must be less than To-DateTime '{toDateTime}'.");
            }

            maybe = _repository.GetWithValues(id, fromDateTime, toDateTime, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"The time series with id '{id}' could not be extracted.");
            }

            return maybe.Value;
        }

        /// <summary>
        ///     Gets a time series with values in the given time interval for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>Dictionary&lt;TId, TimeSeries&lt;TId, TValue&gt;&gt;.</returns>
        public IDictionary<TId, TimeSeries<TId, TValue>> GetWithValues(TId[] ids, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            if (from >= to)
            {
                throw new ArgumentException($"From-DateTime '{from}' must be less than To-DateTime '{to}'.");
            }

            return _repository.GetWithValues(ids, from, to, user);
        }

        /// <summary>
        ///     Gets a time series with values in the given time interval for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>Dictionary&lt;TId, TimeSeries&lt;TId, TValue&gt;&gt;.</returns>
        public IDictionary<TId, TimeSeries<TId, TValue>> GetWithValues(TId[] ids, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            if (from == null && to == null)
            {
                return _repository.GetWithValues(ids, user);
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            if (fromDateTime >= toDateTime)
            {
                throw new ArgumentException($"From-DateTime '{fromDateTime}' must be less than To-DateTime '{toDateTime}'.");
            }

            return _repository.GetWithValues(ids, fromDateTime, toDateTime, user);
        }

        /// <summary>
        ///     Gets the date times for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>SortedSet&lt;DateTime&gt;.</returns>
        public SortedSet<DateTime> GetDateTimes(TId id, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetDateTimes(id, user);
            if (!maybe.HasValue)
            {
                throw new KeyNotFoundException($"The time series with id '{id}' could not be extracted.");
            }

            return maybe.Value;
        }

        /// <summary>
        ///     Gets the first date time for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>System.Nullable&lt;DateTime&gt;.</returns>
        public DateTime? GetFirstDateTime(TId id, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetFirstDateTime(id, user);
            return maybe.HasValue ? maybe.Value : (DateTime?)null;
        }

        /// <summary>
        ///     Gets the first value for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        public DataPoint<TValue> GetFirstValue(TId id, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetFirstValue(id, user);
            return maybe.HasValue ? maybe.Value : null;
        }

        /// <summary>
        ///     Gets the first value for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, DataPoint&lt;TValue&gt;&gt;.</returns>
        public IDictionary<TId, DataPoint<TValue>> GetFirstValue(TId[] ids, ClaimsPrincipal user = null)
        {
            return _repository.GetFirstValue(ids, user);
        }

        /// <summary>
        ///     Gets the first value after the given datetime for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        public DataPoint<TValue> GetFirstValueAfter(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            var lastDateTime = _repository.GetLastDateTime(id, user);
            if (!lastDateTime.HasValue)
            {
                return null;
            }

            if (dateTime >= lastDateTime.Value)
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime, $"DateTime value is out of range. Last DateTime is '{lastDateTime.Value}'.");
            }

            return _repository.GetFirstValueAfter(id, dateTime, user).Value;
        }

        /// <summary>
        ///     Gets the last date time for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>System.Nullable&lt;DateTime&gt;.</returns>
        public DateTime? GetLastDateTime(TId id, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetLastDateTime(id, user);
            return maybe.HasValue ? maybe.Value : (DateTime?)null;
        }

        /// <summary>
        ///     Gets the last value for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        public DataPoint<TValue> GetLastValue(TId id, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetLastValue(id, user);
            return maybe.HasValue ? maybe.Value : null;
        }

        /// <summary>
        ///     Gets the last value for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, DataPoint&lt;TValue&gt;&gt;.</returns>
        public IDictionary<TId, DataPoint<TValue>> GetLastValue(TId[] ids, ClaimsPrincipal user = null)
        {
            return _repository.GetLastValue(ids, user);
        }

        /// <summary>
        ///     Gets the last value before the given datetime for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     dateTime - DateTime value is out of range. First DateTime is
        ///     '{firstDateTime.Value}
        /// </exception>
        public DataPoint<TValue> GetLastValueBefore(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            var firstDateTime = _repository.GetFirstDateTime(id, user);
            if (!firstDateTime.HasValue)
            {
                return null;
            }

            if (dateTime <= firstDateTime.Value)
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime), dateTime, $"DateTime value is out of range. First DateTime is '{firstDateTime.Value}'");
            }

            return _repository.GetLastValueBefore(id, dateTime, user).Value;
        }

        /// <summary>
        ///     Gets the values in the given time interval for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>ITimeSeriesData&lt;TValue&gt;.</returns>
        /// <exception cref="System.ArgumentException">From-DateTime '{fromDateTime}' must be less than To-DateTime '{toDateTime}</exception>
        public ITimeSeriesData<TValue> GetValues(TId id, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            Maybe<ITimeSeriesData<TValue>> maybe;
            if (from == null && to == null)
            {
                maybe = _repository.GetValues(id, user);
                return maybe | new TimeSeriesData<TValue>();
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            if (fromDateTime >= toDateTime)
            {
                throw new ArgumentException($"From-DateTime '{fromDateTime}' must be less than To-DateTime '{toDateTime}'.");
            }

            maybe = _repository.GetValues(id, fromDateTime, toDateTime, user);
            return maybe | new TimeSeriesData<TValue>();
        }

        /// <summary>
        ///     Gets the values in the given time interval for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, ITimeSeriesData&lt;TValue&gt;&gt;.</returns>
        public IDictionary<TId, ITimeSeriesData<TValue>> GetValues(TId[] ids, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            if (from == null && to == null)
            {
                return _repository.GetValues(ids, user);
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            if (fromDateTime >= toDateTime)
            {
                throw new ArgumentException($"From-DateTime '{fromDateTime}' must be less than To-DateTime '{toDateTime}'.");
            }

            return _repository.GetValues(ids, fromDateTime, toDateTime, user);
        }

        /// <summary>
        ///     Gets the vectors in the given time interval for the time series components with the specified ids.
        /// </summary>
        /// <param name="idX">The identifier for the time series with the X-components.</param>
        /// <param name="idY">The identifier for the time series with the Y-components.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>ITimeSeriesData&lt;Vector&lt;TValue&gt;&gt;.</returns>
        public ITimeSeriesData<Vector<TValue>> GetVectors(TId idX, TId idY, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            var xData = GetValues(idX, from, to, user);
            var yData = GetValues(idY, from, to, user);
            return new VectorTimeSeriesData<TValue>(xData, yData);
        }

        /// <summary>
        ///     Gets the vectors in the given time interval for the time series components with the specified ids.
        /// </summary>
        /// <param name="idX">The identifier for the time series with the X-components.</param>
        /// <param name="idY">The identifier for the time series with the Y-components.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>ITimeSeriesData&lt;Vector&lt;TValue&gt;&gt;.</returns>
        public ITimeSeriesData<Vector<TValue>> GetVectors(TId idX, TId idY, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            var xData = GetValues(idX, from, to, user);
            var yData = GetValues(idY, from, to, user);
            return new VectorTimeSeriesData<TValue>(xData, yData);
        }

        /// <summary>
        ///     Gets the vectors in the given time interval for the list of time series components with the specified ids.
        /// </summary>
        /// <param name="ids">An array of tuples with the identifiers for the time series with the X- and Y-components.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;string, ITimeSeriesData&lt;Vector&lt;TValue&gt;&gt;&gt;.</returns>
        public IDictionary<string, ITimeSeriesData<Vector<TValue>>> GetVectors((TId, TId)[] ids, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            var dictionary = new Dictionary<string, ITimeSeriesData<Vector<TValue>>>();
            foreach (var (idx, idy) in ids)
            {
                var xData = GetValues(idx, from, to, user);
                var yData = GetValues(idy, from, to, user);
                var vectorData = new VectorTimeSeriesData<TValue>(xData, yData);
                dictionary[$"{idx}; {idy}"] = vectorData;
            }

            return dictionary;
        }

        /// <summary>
        ///     Gets the Vectors in the given time interval for the list of time series components with the specified ids.
        /// </summary>
        /// <param name="ids">An array of tuples with the identifiers for the time series with the X- and Y-components.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;string, ITimeSeriesData&lt;Vector&lt;TValue&gt;&gt;&gt;.</returns>
        public IDictionary<string, ITimeSeriesData<Vector<TValue>>> GetVectors((TId, TId)[] ids, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            var dictionary = new Dictionary<string, ITimeSeriesData<Vector<TValue>>>();
            foreach (var (idx, idy) in ids)
            {
                var xData = GetValues(idx, from, to, user);
                var yData = GetValues(idy, from, to, user);
                var vectorData = new VectorTimeSeriesData<TValue>(xData, yData);
                dictionary[$"{idx}; {idy}"] = vectorData;
            }

            return dictionary;
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public new static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<ITimeSeriesRepository<TId, TValue>>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public new static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<ITimeSeriesRepository<TId, TValue>>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">
        ///     The path where to look for compatible providers. If path is null, the path of the executing assembly is used.
        /// </param>
        /// <param name="searchPattern">
        ///     File name search pattern. Can contain a combination of valid literal path and wildcard (*and ?) characters.
        /// </param>
        /// <returns>Type[].</returns>
        public new static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<ITimeSeriesRepository<TId, TValue>>(path, searchPattern);
        }

        /// <summary>
        ///     Gets the interpolated value at the given datetime for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;System.Double&gt;.</returns>
        public DataPoint<double> GetInterpolatedValue(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            if (typeof(TValue) != typeof(double))
            {
                throw new Exception("Interpolation only works for time series repositories of type <double>.");
            }

            var repository = (ITimeSeriesRepository<TId, double>)_repository;

            var maybe = repository.GetValue(id, dateTime, user);
            if (maybe.HasValue && maybe.Value.Value != null)
            {
                return maybe.Value;
            }

            var lastBefore = GetLastValueBefore(id, dateTime, user);
            while (lastBefore.Value == null)
            {
                lastBefore = GetLastValueBefore(id, dateTime, user);
            }

            var firstAfter = GetFirstValueAfter(id, dateTime, user);
            while (firstAfter.Value == null)
            {
                firstAfter = GetFirstValueAfter(id, firstAfter.DateTime, user);
            }

            if (TryGet(id, out var timeSeries, user))
            {
                return timeSeries.DataType.Interpolate(lastBefore as DataPoint<double>, firstAfter as DataPoint<double>, dateTime);
            }

            throw new ArgumentException("The timeseries could not be found.", nameof(id));

        }

        /// <summary>
        ///     Gets the interpolated values in the given interval for the time series with the specified id.
        ///     The interval end points are included in the time series data returned.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>ITimeSeriesData&lt;System.Double&gt;.</returns>
        public ITimeSeriesData<double> GetValuesWithInterpolatedEndPoints(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            if (typeof(TValue) != typeof(double))
            {
                throw new Exception("Interpolation only works for time series repositories of type <double>.");
            }

            var data = (ITimeSeriesData<double>)GetValues(id, from, to, user);
            if (!data.ContainsDateTime(from))
            {
                var startPoint = GetInterpolatedValue(id, from, user);
                data.Insert(startPoint.DateTime, startPoint.Value);
            }

            if (!data.ContainsDateTime(to))
            {
                var endPoint = GetInterpolatedValue(id, to, user);
                data.Append(endPoint.DateTime, endPoint.Value);
            }

            return data;
        }
    }

    /// <inheritdoc />
    public class TimeSeriesService : TimeSeriesService<string, double>
    {
        /// <inheritdoc />
        public TimeSeriesService(ITimeSeriesRepository<string, double> repository)
            : base(repository)
        {
        }

        /// <inheritdoc />
        public TimeSeriesService(ITimeSeriesRepository<string, double> repository, ILogger logger)
            : base(repository, logger)
        {
        }
    }
}