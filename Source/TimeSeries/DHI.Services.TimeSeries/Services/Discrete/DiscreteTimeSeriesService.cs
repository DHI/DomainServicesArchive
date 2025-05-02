namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Authorization;
    using Logging;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Discrete TimeSeries Service. A discrete time series service handles time series in a repository with a finite
    ///     number of (discrete) time series - each identified by a unique ID.
    /// </summary>
    /// <typeparam name="TId">The type of the time series identifier.</typeparam>
    /// <typeparam name="TValue">The type of the time series values.</typeparam>
    public class DiscreteTimeSeriesService<TId, TValue> : BaseDiscreteService<TimeSeries<TId, TValue>, TId>, IDiscreteTimeSeriesService<TId, TValue> where TValue : struct, IComparable<TValue>
    {
        private readonly IDiscreteTimeSeriesRepository<TId, TValue> _repository;
        private readonly TimeSeriesService<TId, TValue> _timeSeriesService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DiscreteTimeSeriesService{TId, TValue}" /> class.
        /// </summary>
        /// <param name="repository">The time series repository.</param>
        public DiscreteTimeSeriesService(IDiscreteTimeSeriesRepository<TId, TValue> repository) 
            : base(repository)
        {
            _repository = repository;
            _timeSeriesService = new TimeSeriesService<TId, TValue>(repository);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DiscreteTimeSeriesService{TId, TValue}" /> class.
        /// </summary>
        /// <param name="repository">The time series repository.</param>
        /// <param name="logger">The logger.</param>
        public DiscreteTimeSeriesService(IDiscreteTimeSeriesRepository<TId, TValue> repository, ILogger logger)
            : base(repository, logger)
        {
            _repository = repository;
            _timeSeriesService = new TimeSeriesService<TId, TValue>(repository);
        }

        /// <summary>
        ///     Gets a time series with values in the given time interval for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>TimeSeries&lt;TId, TValue&gt;.</returns>
        public TimeSeries<TId, TValue> GetWithValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetWithValues(id, from, to, user);
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
            return _timeSeriesService.GetWithValues(id, from, to, user);
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
            return _timeSeriesService.GetWithValues(ids, from, to, user);
        }

        /// <summary>
        ///     Gets a time series with values in the given time interval for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, TimeSeries&lt;TId, TValue&gt;&gt;.</returns>
        public IDictionary<TId, TimeSeries<TId, TValue>> GetWithValues(TId[] ids, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetWithValues(ids, from, to, user);
        }

        /// <summary>
        ///     Gets the available time steps for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>SortedSet&lt;DateTime&gt;.</returns>
        public SortedSet<DateTime> GetDateTimes(TId id, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetDateTimes(id, user);
        }

        /// <summary>
        ///     Gets the first time step for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>System.Nullable&lt;DateTime&gt;.</returns>
        public DateTime? GetFirstDateTime(TId id, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetFirstDateTime(id, user);
        }

        /// <summary>
        ///     Gets the first value for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        public DataPoint<TValue> GetFirstValue(TId id, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetFirstValue(id, user);
        }

        /// <summary>
        ///     Gets a collection of first values for a collection of time series with the specified identifiers.
        /// </summary>
        /// <param name="ids">The time series identifiers.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, DataPoint&lt;TValue&gt;&gt;.</returns>
        public IDictionary<TId, DataPoint<TValue>> GetFirstValue(TId[] ids, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetFirstValue(ids, user);
        }

        /// <summary>
        ///     Gets the first value after the specified time step for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        public DataPoint<TValue> GetFirstValueAfter(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetFirstValueAfter(id, dateTime, user);
        }

        /// <summary>
        ///     Gets the last time step for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>System.Nullable&lt;DateTime&gt;.</returns>
        public DateTime? GetLastDateTime(TId id, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetLastDateTime(id, user);
        }

        /// <summary>
        ///     Gets the last value for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        public DataPoint<TValue> GetLastValue(TId id, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetLastValue(id, user);
        }

        /// <summary>
        ///     Gets a collection of last values for a collection of time series with the specified identifiers.
        /// </summary>
        /// <param name="ids">The time series identifiers.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, DataPoint&lt;TValue&gt;&gt;.</returns>
        public IDictionary<TId, DataPoint<TValue>> GetLastValue(TId[] ids, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetLastValue(ids, user);
        }

        /// <summary>
        ///     Gets the last value before the specified time step for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        public DataPoint<TValue> GetLastValueBefore(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetLastValueBefore(id, dateTime, user);
        }

        /// <summary>
        ///     Gets the value at the given time step for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        public DataPoint<TValue> GetValue(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetValue(id, dateTime, user);
        }

        /// <summary>
        ///     Gets the values within the given time interval for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">From date time.</param>
        /// <param name="to">To date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>ITimeSeriesData&lt;TValue&gt;.</returns>
        public ITimeSeriesData<TValue> GetValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetValues(id, from, to, user);
        }

        /// <summary>
        ///     Gets the values within the given time interval for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">From date time.</param>
        /// <param name="to">To date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>ITimeSeriesData&lt;TValue&gt;.</returns>
        public ITimeSeriesData<TValue> GetValues(TId id, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetValues(id, from, to, user);
        }

        /// <summary>
        ///     Gets the aggregated value in the given time interval for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        public TValue? GetAggregatedValue(TId id, AggregationType aggregationType, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetAggregatedValue(id, aggregationType, from, to, user);
        }

        /// <summary>
        ///     Gets the list of aggregated value in the given time interval for the ensemble time series with the specified id.
        /// </summary>
        /// <param name="id">The ensemble time series identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        public IList<TValue?> GetEnsembleAggregatedValues(TId id, AggregationType aggregationType, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetEnsembleAggregatedValues(id, aggregationType, from, to, user);
        }

        /// <summary>
        ///     Gets the list of aggregated value in the given time interval for the list of ensemble time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ensemble time series identifiers.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        public IDictionary<TId, IList<TValue?>> GetEnsembleAggregatedValues(IEnumerable<TId> ids, AggregationType aggregationType, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetEnsembleAggregatedValues(ids, aggregationType, from, to, user);
        }

        /// <summary>
        ///     Gets a collection of values within the given time interval for a collection of time series with the specified
        ///     identifiers.
        /// </summary>
        /// <param name="ids">The time series ids.</param>
        /// <param name="from">From date time.</param>
        /// <param name="to">To date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, ITimeSeriesData&lt;TValue&gt;&gt;.</returns>
        public IDictionary<TId, ITimeSeriesData<TValue>> GetValues(TId[] ids, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetValues(ids, from, to, user);
        }

        /// <summary>
        ///     Gets a collection of values within the given time interval for a collection of time series with the specified
        ///     identifiers.
        /// </summary>
        /// <param name="ids">The time series ids.</param>
        /// <param name="from">From date time.</param>
        /// <param name="to">To date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, ITimeSeriesData&lt;TValue&gt;&gt;.</returns>
        public IDictionary<TId, ITimeSeriesData<TValue>> GetValues(TId[] ids, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetValues(ids, from, to, user);
        }

        /// <summary>
        ///     Gets the aggregated value in the given time interval for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The time series identifiers.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        public IDictionary<TId, TValue?> GetAggregatedValues(IEnumerable<TId> ids, AggregationType aggregationType, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetAggregatedValues(ids, aggregationType, from, to, user);
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
            return _timeSeriesService.GetVectors(idX, idY, from, to, user);
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
            return _timeSeriesService.GetVectors(idX, idY, from, to, user);
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
            return _timeSeriesService.GetVectors(ids, from, to, user);
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
            return _timeSeriesService.GetVectors(ids, from, to, user);
        }

        /// <summary>
        ///     Sets some time series values for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="data">The time series data.</param>
        /// <param name="user">The user.</param>
        public void SetValues(TId id, ITimeSeriesData<TValue> data, ClaimsPrincipal user = null)
        {
            _ThrowExceptionIfInvalidID(id, user);
            _repository.SetValues(id, data, user);
            OnValuesSet(id, data, user);
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IDiscreteTimeSeriesRepository<TId, TValue>>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IDiscreteTimeSeriesRepository<TId, TValue>>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">
        ///     The path where to look for compatible providers. If path is null, the path of the executing assembly
        ///     is used.
        /// </param>
        /// <param name="searchPattern">
        ///     File name search pattern. Can contain a combination of valid literal path and wildcard
        ///     (*and ?) characters.
        /// </param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IDiscreteTimeSeriesRepository<TId, TValue>>(path, searchPattern);
        }

        /// <summary>
        ///     Gets the interpolated value at the given time step for a time series with the specified identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;System.Double&gt;.</returns>
        public DataPoint<double> GetInterpolatedValue(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetInterpolatedValue(id, dateTime, user);
        }

        /// <summary>
        ///     Gets the values within the given time interval (with interpolated end points) for a time series with the specified
        ///     identifier.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">From date time.</param>
        /// <param name="to">To date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>ITimeSeriesData&lt;System.Double&gt;.</returns>
        public ITimeSeriesData<double> GetValuesWithInterpolatedEndPoints(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return _timeSeriesService.GetValuesWithInterpolatedEndPoints(id, from, to, user);
        }

        /// <summary>
        ///     Occurs when values are set.
        /// </summary>
        public event EventHandler<EventArgs<(TId id, ITimeSeriesData<TValue> data, string userName)>> ValuesSet;

        /// <summary>
        ///     Called when values are set.
        /// </summary>
        protected virtual void OnValuesSet(TId id, ITimeSeriesData<TValue> data, ClaimsPrincipal user)
        {
            var userName = user?.GetUserId();
            ValuesSet?.Invoke(this, new EventArgs<(TId, ITimeSeriesData<TValue>, string)>((id, data, userName)));
        }

        private void _ThrowExceptionIfInvalidID(TId id, ClaimsPrincipal user = null)
        {
            if (!Exists(id, user))
            {
                throw new KeyNotFoundException($"The time series with id '{id}' was not found.");
            }
        }
    }

    /// <inheritdoc />
    public class DiscreteTimeSeriesService : DiscreteTimeSeriesService<string, double>
    {
        /// <inheritdoc />
        public DiscreteTimeSeriesService(IDiscreteTimeSeriesRepository<string, double> repository) 
            : base(repository)
        {
        }

        /// <inheritdoc />
        public DiscreteTimeSeriesService(IDiscreteTimeSeriesRepository<string, double> repository, ILogger logger)
            : base(repository, logger)
        {
        }
    }
}