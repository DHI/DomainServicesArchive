namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Core Time Series Service.
    /// </summary>
    /// <typeparam name="TId">The type of the time series identifier.</typeparam>
    /// <typeparam name="TValue">The type of the time series value.</typeparam>
    public class CoreTimeSeriesService<TId, TValue> : BaseService<TimeSeries<TId, TValue>, TId>, ICoreTimeSeriesService<TId, TValue> where TValue : struct, IComparable<TValue>
    {
        private readonly ICoreTimeSeriesRepository<TId, TValue> _repository;
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesService{TId, TValue}" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public CoreTimeSeriesService(ICoreTimeSeriesRepository<TId, TValue> repository)
            : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeSeriesService{TId, TValue}" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="logger">The logger.</param>
        public CoreTimeSeriesService(ICoreTimeSeriesRepository<TId, TValue> repository, ILogger logger)
            : base(repository, logger)
        {
            _repository = repository;
        }

        /// <summary>
        ///     Gets the value at the given datetime for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="user">The user.</param>
        /// <returns>DataPoint&lt;TValue&gt;.</returns>
        public DataPoint<TValue> GetValue(TId id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            var maybe = _repository.GetValue(id, dateTime, user);
            return maybe.HasValue ? maybe.Value : null;
        }

        /// <summary>
        ///     Gets the values in the given time interval for the time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>ITimeSeriesData&lt;TValue&gt;.</returns>
        public ITimeSeriesData<TValue> GetValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            if (from >= to)
            {
                throw new ArgumentException($"From-DateTime '{from}' must be less than To-DateTime '{to}'.");
            }

            var maybe = _repository.GetValues(id, from, to, user);
            return maybe | new TimeSeriesData<TValue>();
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
            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            if (fromDateTime >= toDateTime)
            {
                throw new ArgumentException($"From-DateTime '{fromDateTime}' must be less than To-DateTime '{toDateTime}'.");
            }

            return _repository.GetAggregatedValue(id, aggregationType, fromDateTime, toDateTime, user);
        }

        /// <summary>
        ///     Gets the list of aggregated value in the given time interval for the ensemble time series with the specified id.
        /// </summary>
        /// <param name="id">The time series identifier.</param>
        /// <param name="aggregationType">The aggregation type.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        public IList<TValue?> GetEnsembleAggregatedValues(TId id, AggregationType aggregationType, DateTime? from = null, DateTime? to = null, ClaimsPrincipal user = null)
        {
            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            if (fromDateTime >= toDateTime)
            {
                throw new ArgumentException($"From-DateTime '{fromDateTime}' must be less than To-DateTime '{toDateTime}'.");
            }

            return _repository.GetEnsembleAggregatedValues(id, aggregationType, fromDateTime, toDateTime, user);
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
            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            if (fromDateTime >= toDateTime)
            {
                throw new ArgumentException($"From-DateTime '{fromDateTime}' must be less than To-DateTime '{toDateTime}'.");
            }

            return _repository.GetEnsembleAggregatedValues(ids, aggregationType, fromDateTime, toDateTime, user);
        }

        /// <summary>
        ///     Gets the values in the given time interval for the list of time series with the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="user">The user.</param>
        /// <returns>IDictionary&lt;TId, ITimeSeriesData&lt;TValue&gt;&gt;.</returns>
        public IDictionary<TId, ITimeSeriesData<TValue>> GetValues(TId[] ids, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            if (from >= to)
            {
                throw new ArgumentException($"From-DateTime '{from}' must be less than To-DateTime '{to}'.");
            }

            return _repository.GetValues(ids, from, to, user);
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
            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            if (fromDateTime >= toDateTime)
            {
                throw new ArgumentException($"From-DateTime '{fromDateTime}' must be less than To-DateTime '{toDateTime}'.");
            }

            return _repository.GetAggregatedValues(ids, aggregationType, fromDateTime, toDateTime, user);
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<ICoreTimeSeriesRepository<TId, TValue>>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<ICoreTimeSeriesRepository<TId, TValue>>(path);
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
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<ICoreTimeSeriesRepository<TId, TValue>>(path, searchPattern);
        }
    }

    /// <inheritdoc />
    public class CoreTimeSeriesService : CoreTimeSeriesService<string, double>
    {
        /// <inheritdoc />
        public CoreTimeSeriesService(ICoreTimeSeriesRepository<string, double> repository)
            : base(repository)
        {
        }

        /// <inheritdoc />
        public CoreTimeSeriesService(ICoreTimeSeriesRepository<string, double> repository, ILogger logger)
            : base(repository, logger)
        {
        }
    }
}