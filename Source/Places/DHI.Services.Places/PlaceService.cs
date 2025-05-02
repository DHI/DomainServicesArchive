using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DHI.Services.Places.Test")]

namespace DHI.Services.Places
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using GIS;
    using Scalars;
    using SkiaSharp;
    using Spatial;
    using TimeSeries;
    using FeatureCollection = Spatial.FeatureCollection;

    public class PlaceService<TTimeSeriesId, TScalarId, TScalarFlag, TCollectionId> :
        BaseGroupedUpdatableDiscreteService<Place<TCollectionId>, string>
        where TScalarFlag : struct
        where TCollectionId : notnull
    {
        private readonly IGisService<TCollectionId> _gisService;
        private readonly IPlaceRepository<TCollectionId> _repository;
        private readonly Dictionary<string, IScalarService<TScalarId, TScalarFlag>>? _scalarServices;
        private readonly Dictionary<string, IDiscreteTimeSeriesService<TTimeSeriesId, double>>? _timeSeriesServices;

        public PlaceService(IPlaceRepository<TCollectionId> repository,
            Dictionary<string, IDiscreteTimeSeriesService<TTimeSeriesId, double>>? timeSeriesServices,
            Dictionary<string, IScalarService<TScalarId, TScalarFlag>>? scalarServices,
            IGisService<TCollectionId> gisService)
            : base(repository)
        {
            Guard.Against.Null(gisService, nameof(gisService));
            Guard.Against.Null(repository, nameof(repository));
            _repository = repository;
            _gisService = gisService;
            if (timeSeriesServices != null)
            {
                _timeSeriesServices = timeSeriesServices;
            }

            if (scalarServices != null)
            {
                _scalarServices = scalarServices;
            }
        }

        protected virtual IPlaceRepository<TCollectionId> Repository => _repository;
        protected virtual IGisService<TCollectionId> GisService => _gisService;
        protected virtual Dictionary<string, IScalarService<TScalarId, TScalarFlag>>? ScalarServices => _scalarServices;
        protected virtual Dictionary<string, IDiscreteTimeSeriesService<TTimeSeriesId, double>>? TimeSeriesServices => _timeSeriesServices;


        /// <summary>
        ///     Get the specified place.
        /// </summary>
        /// <param name="id">The place id.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public override Place<TCollectionId> Get(string id, ClaimsPrincipal? user = null)
        {
            if (!_repository.Contains(id, user) || !base.TryGet(id, out var place, user))
            {
                throw new KeyNotFoundException($"Place with id '{id}' was not found.");
            }

            return place;
        }

        /// <summary>
        ///     Adds the specified place.
        /// </summary>
        /// <param name="place">The place.</param>
        /// <param name="user">The user.</param>
        public override void Add(Place<TCollectionId> place, ClaimsPrincipal? user = null)
        {
            var featureId = place.FeatureId;
            if (!_gisService.Exists(featureId.FeatureCollectionId))
            {
                throw new ArgumentException($"Feature collection with ID '{featureId.FeatureCollectionId}' does not exist.", nameof(place));
            }

            var query = new Query<IFeature>
            {
                new(featureId.AttributeKey, QueryOperator.Equal, featureId.AttributeValue)
            };

            if (!_gisService.Get(featureId.FeatureCollectionId, query, false).Features.Any())
            {
                throw new ArgumentException($"A feature with attribute value '{featureId.AttributeValue}' in attribute key '{featureId.AttributeKey}' was not found in feature collection with ID '{featureId.FeatureCollectionId}'.", nameof(place));
            }

            foreach (var indicator in place.Indicators.Values)
            {
                CheckIndicator(indicator);
            }

            if (!_repository.Contains(place.Id, user))
            {
                var cancelEventArgs = new CancelEventArgs<Place<TCollectionId>>(place);
                OnAdding(cancelEventArgs);
                if (!cancelEventArgs.Cancel)
                {
                    _repository.Add(place, user);
                    OnAdded(place);
                }
            }
            else
            {
                throw new ArgumentException($"Place with id '{place.Id}' already exists.");
            }
        }

        //protected override bool Contains(Place<TCollectionId> entity, ClaimsPrincipal user = null) => Contains(entity.FullName, user);

        /// <summary>
        ///     Adds an indicator to the specified place.
        /// </summary>
        /// <param name="placeId">The place identifier.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="indicator">The indicator.</param>
        /// <param name="user">The user.</param>
        public void AddIndicator(string placeId, string type, Indicator indicator, ClaimsPrincipal? user = null)
        {
            if (!_repository.Contains(placeId, user) || !base.TryGet(placeId, out var place, user))
            {
                throw new KeyNotFoundException($"Place with id '{placeId}' was not found.");
            }

            if (place.Indicators.ContainsKey(type))
            {
                throw new ArgumentException($"An indicator of type '{type}' already exists at the place '{placeId}'.", nameof(type));
            }

            CheckIndicator(indicator);
            place.Indicators.Add(type, indicator);
            Update(place, user);
        }

        /// <summary>
        ///     Removes an indicator from the specified place.
        /// </summary>
        /// <param name="placeId">The place identifier.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="user">The user.</param>
        public void RemoveIndicator(string placeId, string type, ClaimsPrincipal? user = null)
        {
            if (!_repository.Contains(placeId, user) || !base.TryGet(placeId, out var place, user))
            {
                throw new KeyNotFoundException($"Place with id '{placeId}' was not found.");
            }

            if (!place.Indicators.ContainsKey(type))
            {
                throw new ArgumentException($"An indicator of type '{type}' is not defined at the place '{placeId}'.", nameof(type));
            }

            place.Indicators.Remove(type);
            Update(place, user);
        }

        /// <summary>
        ///     Updates an indicator at the specified place.
        /// </summary>
        /// <param name="placeId">The place identifier.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="indicator">The indicator.</param>
        /// <param name="user">The user.</param>
        public void UpdateIndicator(string placeId, string type, Indicator indicator, ClaimsPrincipal? user = null)
        {
            if (!_repository.Contains(placeId, user) || !base.TryGet(placeId, out var place, user))
            {
                throw new KeyNotFoundException($"Place with id '{placeId}' was not found.");
            }

            if (!place.Indicators.ContainsKey(type))
            {
                throw new ArgumentException($"An indicator of type '{type}' is not defined at the place '{placeId}'.", nameof(type));
            }

            place.Indicators[type] = indicator;
            Update(place, user);
        }

        /// <summary>
        ///     Gets all indicators in the repository.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>a dictionary with key place id, which contains another dictionary with the indicator type as key</returns>
        public IDictionary<string, IDictionary<string, Indicator>> GetIndicators(ClaimsPrincipal? user = null)
        {
            return _repository.GetIndicators(user);
        }

        /// <summary>
        ///     Gets all indicators at the specified place.
        /// </summary>
        /// <param name="placeId">The place identifier.</param>
        /// <param name="user">The user.</param>
        public IDictionary<string, Indicator> GetIndicatorsByPlace(string placeId, ClaimsPrincipal? user = null)
        {
            Guard.Against.NullOrEmpty(placeId, nameof(placeId));
            return _repository.GetIndicatorsByPlace(placeId, user);
        }

        /// <summary>
        ///     Gets all indicators of the specified indicator type.
        /// </summary>
        /// <param name="type">The indicator type.</param>
        /// <param name="user">The user.</param>
        public IDictionary<string, Indicator> GetIndicatorsByType(string type, ClaimsPrincipal? user = null)
        {
            Guard.Against.NullOrEmpty(type, nameof(type));
            return _repository.GetIndicatorsByType(type, user);
        }

        /// <summary>
        ///     Gets all indicators of the specified indicator type from places within the specified group.
        /// </summary>
        /// <param name="group">The place group.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="user">The user.</param>
        public IDictionary<string, Indicator> GetIndicatorsByGroupAndType(string group, string type, ClaimsPrincipal? user = null)
        {
            Guard.Against.NullOrEmpty(group, nameof(group));
            Guard.Against.NullOrEmpty(type, nameof(type));
            if (!_repository.ContainsGroup(group, user))
            {
                throw new KeyNotFoundException($"Group '{group}' does not exist.");
            }

            return _repository.GetIndicatorsByGroupAndType(group, type, user);
        }

        /// <summary>
        ///     Gets all indicators from places within the specified group.
        /// </summary>
        /// <param name="group">The place group.</param>
        /// <param name="user">The user.</param>
        /// <returns>a dictionary with key place id, which contains another dictionary with the indicator type as key</returns>
        public IDictionary<string, IDictionary<string, Indicator>> GetIndicatorsByGroup(string group, ClaimsPrincipal? user = null)
        {
            Guard.Against.NullOrEmpty(group, nameof(group));
            if (!_repository.ContainsGroup(group, user))
            {
                throw new KeyNotFoundException($"Group '{group}' does not exist.");
            }

            return _repository.GetIndicatorsByGroup(group, user);
        }

        /// <summary>
        ///     Gets the specified indicator at the specified place.
        /// </summary>
        /// <param name="placeId">The place identifier.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="user">The user.</param>
        public Indicator GetIndicator(string placeId, string type, ClaimsPrincipal? user = null)
        {
            if (!_repository.Contains(placeId, user))
            {
                throw new KeyNotFoundException($"Place with id '{placeId}' was not found.");
            }

            var maybe = _repository.GetIndicator(placeId, type, user);
            if (maybe.HasValue)
            {
                return maybe.Value;
            }

            throw new KeyNotFoundException($"An indicator of type '{type}' was not found at '{placeId}'.");
        }

        /// <summary>
        ///     Gets threshold values for all indicators at the specified place.
        /// </summary>
        /// <param name="placeId">The place identifier.</param>
        /// <param name="user">The user.</param>
        public IDictionary<string, IEnumerable<double>> GetThresholdValues(string placeId, ClaimsPrincipal? user = null)
        {
            if (!_repository.Contains(placeId, user))
            {
                throw new KeyNotFoundException($"Place with id '{placeId}' was not found.");
            }

            var indicators = _repository.GetIndicatorsByPlace(placeId, user);
            return indicators.ToDictionary(indicator => indicator.Key, indicator => indicator.Value.GetPalette().ThresholdValues);
        }

        /// <summary>
        ///     Gets the threshold values for the specified indicator at the specified place.
        /// </summary>
        /// <param name="placeId">The place identifier.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="user">The user.</param>
        public IEnumerable<double> GetThresholdValues(string placeId, string type, ClaimsPrincipal? user = null)
        {
            var maybe = _repository.GetIndicator(placeId, type, user);
            if (maybe.HasValue)
            {
                return maybe.Value.GetPalette().ThresholdValues;
            }

            if (!_repository.Contains(placeId, user))
            {
                throw new KeyNotFoundException($"Place with id '{placeId}' was not found.");
            }

            throw new KeyNotFoundException($"An indicator of type '{type}' was not found at '{placeId}'.");
        }

        /// <summary>
        ///     Gets the status of the specified indicator at the specified place.
        /// </summary>
        /// <param name="placeId">The place identifier.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="offsetDateTime">Must be given If the indicator time interval is relative to a specified datetime</param>
        /// <param name="path">Path to inject into the entity ID</param>
        /// <param name="user">The user.</param>
        public Maybe<SKColor> GetIndicatorStatus(string placeId, string type, DateTime? offsetDateTime = null, string? path = null, ClaimsPrincipal? user = null)
        {
            if (!_repository.Contains(placeId, user) || !base.TryGet(placeId, out var place, user))
            {
                throw new KeyNotFoundException($"Place with id '{placeId}' was not found.");
            }

            if (!place.Indicators.ContainsKey(type))
            {
                throw new ArgumentException($"Place '{place.Id}' does not contain an indicator of type '{type}'.", nameof(type));
            }

            return GetIndicatorStatus(place.Indicators[type], offsetDateTime, path, user);
        }

        /// <summary>
        ///     Gets the status all indicators of the specified type.
        /// </summary>
        /// <param name="type">The indicator type.</param>
        /// <param name="group"></param>
        /// <param name="offsetDateTime">Must be given If the indicator time interval is relative to a specified datetime</param>
        /// <param name="path">Path to inject into the entity ID</param>
        /// <param name="user">The user.</param>
        public IDictionary<string, SKColor> GetIndicatorStatusByType(string type, string? group = null, DateTime? offsetDateTime = null, string? path = null, ClaimsPrincipal? user = null)
        {
            var result = new Dictionary<string, SKColor>();
            var indicators = group is null ? GetIndicatorsByType(type, user) : GetIndicatorsByGroupAndType(group, type, user);
            if (indicators.Count == 0)
            {
                throw new ArgumentException($"An indicator of type '{type}' was not found.");
            }

            var colorList = GetIndicatorStatus(indicators.Values.ToList(), offsetDateTime, path, user);
            foreach (var indicator in indicators)
            {
                var maybe = colorList[indicator.Value];
                if (maybe.HasValue)
                {
                    result.Add(indicator.Key, maybe.Value);
                }
            }

            return result;
        }

        /// <summary>
        ///     Gets the status of all indicators in the specified group.
        /// </summary>
        /// <param name="group">the indicator group</param>
        /// <param name="offsetDateTime">Must be given If the indicator time interval is relative to a specified datetime</param>
        /// <param name="path">Path to inject into the entity ID</param>
        /// <param name="user">The user.</param>
        /// <returns>a dictionary, the key is the place id; the key of the dictionary inside is the indicator type.</returns>
        public IDictionary<string, Dictionary<string, SKColor>> GetIndicatorStatusByGroup(string group, DateTime? offsetDateTime = null, string? path = null, ClaimsPrincipal? user = null)
        {
            var result = new Dictionary<string, Dictionary<string, SKColor>>();
            var indicators = GetIndicatorsByGroup(group, user);
            var indicatorList = new List<Indicator>();
            foreach (var keyValue in indicators)
            {
                indicatorList.AddRange(keyValue.Value.Values);
            }

            var colorList = GetIndicatorStatus(indicatorList, offsetDateTime, path, user);
            foreach (var indicator in indicators)
            {
                foreach (var keyValue in indicator.Value)
                {
                    var maybe = colorList[keyValue.Value];
                    if (maybe.HasValue)
                    {
                        if (!result.ContainsKey(indicator.Key))
                        {
                            result.Add(indicator.Key, new Dictionary<string, SKColor> { { keyValue.Key, maybe.Value } });
                        }
                        else
                        {
                            result[indicator.Key].Add(keyValue.Key, maybe.Value);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Gets the status of the specified indicator.
        /// </summary>
        /// <remarks>
        ///     If no data, or only corrupted data, is found, an empty Maybe is returned.
        /// </remarks>
        /// <param name="indicator">The indicator.</param>
        /// <param name="offsetDateTime">Must be given If the indicator time interval is relative to a specified datetime</param>
        /// <param name="path">Path to inject into the entity ID</param>
        /// <param name="user">The user.</param>
        public Maybe<SKColor> GetIndicatorStatus(Indicator indicator, DateTime? offsetDateTime = null, string? path = null, ClaimsPrincipal? user = null)
        {
            var dataSource = indicator.DataSource;
            double? indicatorValue;
            switch (dataSource.Type)
            {
                case DataSourceType.Scalar:
                    if (_scalarServices is null || !_scalarServices.ContainsKey(dataSource.ConnectionId))
                    {
                        throw new Exception($"No scalar service connection with ID '{dataSource.ConnectionId}' could be found.");
                    }

                    var scalarService = _scalarServices[dataSource.ConnectionId];
                    var scalar = scalarService.Get((TScalarId) dataSource.EntityId, user);
                    var maybe = scalar.GetData();
                    if (!maybe.HasValue)
                    {
                        return Maybe.Empty<SKColor>();
                    }

                    indicatorValue = (double) maybe.Value.Value;
                    break;
                case DataSourceType.TimeSeries:
                case DataSourceType.EnsembleTimeSeries:
                    if (_timeSeriesServices is null || !_timeSeriesServices.ContainsKey(dataSource.ConnectionId))
                    {
                        throw new Exception($"No time series service connection with ID '{dataSource.ConnectionId}' could be found.");
                    }

                    if (indicator.TimeInterval is null)
                    {
                        throw new ArgumentException("There is no declarative TimeInterval defined on the given indicator. You must use the overloaded method where you explicitly define the time period instead.", nameof(indicator));
                    }

                    var timeSeriesService = _timeSeriesServices[dataSource.ConnectionId];
                    var timeSeriesId = ResolveTimeSeriesId(dataSource.EntityId, path);
                    var (from, to) = GetPeriod(indicator.TimeInterval, offsetDateTime);

                    if (dataSource.Type == DataSourceType.TimeSeries)
                    {
                        var aggregatedValue = timeSeriesService.GetAggregatedValue((TTimeSeriesId) timeSeriesId, indicator.AggregationType, from, to, user);
                        if (aggregatedValue is null)
                        {
                            return Maybe.Empty<SKColor>();
                        }

                        indicatorValue = aggregatedValue;
                    }
                    else
                    {
                        var aggregatedValues = timeSeriesService.GetEnsembleAggregatedValues((TTimeSeriesId) timeSeriesId, indicator.AggregationType, from, to, user);
                        if (aggregatedValues.All(v => v is null))
                        {
                            return Maybe.Empty<SKColor>();
                        }

                        indicatorValue = GetIndicatorValue(indicator, aggregatedValues);
                    }

                    break;
                default:
                    throw new NotSupportedException($"Data source type '{dataSource.Type}' is not supported.");
            }

            return indicatorValue is null ? Maybe.Empty<SKColor>() : indicator.GetPalette().GetColor((double) indicatorValue).ToMaybe();
        }

        /// <summary>
        ///     Gets the status of the specified indicator list.
        /// </summary>
        /// <param name="indicators">The indicator list.</param>
        /// <param name="offsetDateTime">Must be given If the indicator time interval is relative to a specified datetime</param>
        /// <param name="path">Path to inject into the entity ID</param>
        /// <param name="user">The user.</param>
        public IDictionary<Indicator, Maybe<SKColor>> GetIndicatorStatus(IList<Indicator> indicators, DateTime? offsetDateTime = null, string? path = null, ClaimsPrincipal? user = null)
        {
            var colorList = new Dictionary<Indicator, Maybe<SKColor>>();
            var connectionGroup = indicators.GroupBy(i => i.DataSource.ConnectionId);
            foreach (var connection in connectionGroup)
            {
                var dataSourceGroup = connection.GroupBy(c => c.DataSource.Type);
                foreach (var type in dataSourceGroup)
                {
                    double? indicatorValue;
                    switch (type.Key)
                    {
                        case DataSourceType.Scalar:
                            if (_scalarServices is null || !_scalarServices.ContainsKey(connection.Key))
                            {
                                throw new Exception($"No scalar service with connection ID '{connection.Key}' could be found.");
                            }

                            var scalarService = _scalarServices[connection.Key];
                            foreach (var indicator in type)
                            {
                                var scalar = scalarService.Get((TScalarId) indicator.DataSource.EntityId, user);
                                var maybe = scalar.GetData();
                                indicatorValue = maybe.HasValue ? (double) maybe.Value.Value : null;
                                AddColor(indicatorValue, colorList, indicator);
                            }

                            break;
                        case DataSourceType.TimeSeries:
                        case DataSourceType.EnsembleTimeSeries:
                            if (_timeSeriesServices is null || !_timeSeriesServices.ContainsKey(connection.Key))
                            {
                                throw new Exception($"No time series service connection with ID '{connection.Key}' could be found.");
                            }

                            if (type.Any(i => i.TimeInterval is null))
                            {
                                throw new ArgumentException("There is no declarative TimeInterval defined on one of the given indicators. You must use the overloaded method where you explicitly define the time period instead.", nameof(indicators));
                            }

                            var timeSeriesService = _timeSeriesServices[connection.Key];
                            var timeIntervalGroup = type.GroupBy(i => i.TimeInterval!.ToString());
                            foreach (var interval in timeIntervalGroup)
                            {
                                var (from, to) = GetPeriod(interval.ElementAt(0).TimeInterval!, offsetDateTime);
                                var aggregationTypeGroup = interval.GroupBy(i => i.AggregationType);
                                foreach (var group in aggregationTypeGroup)
                                {
                                    var tsIdList = new List<TTimeSeriesId>();
                                    foreach (var indicator in group)
                                    {
                                        var timeSeriesId = ResolveTimeSeriesId(indicator.DataSource.EntityId, path);
                                        tsIdList.Add((TTimeSeriesId) timeSeriesId);
                                    }

                                    if (type.Key.Equals(DataSourceType.TimeSeries))
                                    {
                                        var aggregatedValues = timeSeriesService.GetAggregatedValues(tsIdList, group.ElementAt(0).AggregationType, from, to, user);
                                        foreach (var value in aggregatedValues)
                                        {
                                            var indicator = group.FirstOrDefault(i => Equals((TTimeSeriesId) ResolveTimeSeriesId(i.DataSource.EntityId, path), value.Key));
                                            indicatorValue = value.Value;
                                            AddColor(indicatorValue, colorList, indicator);
                                        }
                                    }
                                    else
                                    {
                                        var aggregatedValues = timeSeriesService.GetEnsembleAggregatedValues(tsIdList, group.ElementAt(0).AggregationType, from, to, user);
                                        foreach (var value in aggregatedValues)
                                        {
                                            var indicator = group.FirstOrDefault(i => Equals((TTimeSeriesId) ResolveTimeSeriesId(i.DataSource.EntityId, path), value.Key));
                                            indicatorValue = GetIndicatorValue(indicator, value.Value);
                                            AddColor(indicatorValue, colorList, indicator);
                                        }
                                    }
                                }
                            }

                            break;

                        default:
                            throw new NotSupportedException($"Data source type '{type.Key}' is not supported.");
                    }
                }
            }

            return colorList;
        }

        /// <summary>
        ///     Gets the status of the specified indicator list for a specified period from-to.
        /// </summary>
        /// <param name="indicators">The indicator list.</param>
        /// <param name="from">period from date</param>
        /// <param name="to">period to date</param>
        /// <param name="path">Path to inject into the entity ID</param>
        /// <param name="user">The user.</param>
        public IDictionary<Indicator, Maybe<SKColor>> GetIndicatorStatus(IList<Indicator> indicators, DateTime from, DateTime to, string? path = null, ClaimsPrincipal? user = null)
        {
            var colorList = new Dictionary<Indicator, Maybe<SKColor>>();
            var connectionGroup = indicators.GroupBy(i => i.DataSource.ConnectionId);
            foreach (var connection in connectionGroup)
            {
                var dataSourceGroup = connection.GroupBy(c => c.DataSource.Type);
                foreach (var type in dataSourceGroup)
                {
                    double? indicatorValue;
                    switch (type.Key)
                    {
                        case DataSourceType.Scalar:
                            if (_scalarServices is null || !_scalarServices.ContainsKey(connection.Key))
                            {
                                throw new Exception($"No scalar service connection with ID '{connection.Key}' could be found.");
                            }

                            var scalarService = _scalarServices[connection.Key];
                            foreach (var indicator in type)
                            {
                                var scalar = scalarService.Get((TScalarId) indicator.DataSource.EntityId, user);
                                var maybe = scalar.GetData();
                                indicatorValue = maybe.HasValue ? (double) maybe.Value.Value : null;
                                AddColor(indicatorValue, colorList, indicator);
                            }

                            break;
                        case DataSourceType.TimeSeries:
                        case DataSourceType.EnsembleTimeSeries:
                            if (_timeSeriesServices is null || !_timeSeriesServices.ContainsKey(connection.Key))
                            {
                                throw new Exception($"No time series service connection with ID '{connection.Key}' could be found.");
                            }

                            var timeSeriesService = _timeSeriesServices[connection.Key];
                            var aggregationTypeGroup = type.GroupBy(i => i.AggregationType);
                            foreach (var group in aggregationTypeGroup)
                            {
                                var tsIdList = new List<TTimeSeriesId>();
                                foreach (var indicator in group)
                                {
                                    var timeSeriesId = ResolveTimeSeriesId(indicator.DataSource.EntityId, path);
                                    tsIdList.Add((TTimeSeriesId) timeSeriesId);
                                }

                                if (type.Key.Equals(DataSourceType.TimeSeries))
                                {
                                    var aggregatedValues = timeSeriesService.GetAggregatedValues(tsIdList, group.ElementAt(0).AggregationType, from, to, user);
                                    foreach (var value in aggregatedValues)
                                    {
                                        var indicator = group.FirstOrDefault(i => Equals((TTimeSeriesId) ResolveTimeSeriesId(i.DataSource.EntityId, path), value.Key));
                                        indicatorValue = value.Value;
                                        AddColor(indicatorValue, colorList, indicator);
                                    }
                                }
                                else
                                {
                                    var aggregatedValues = timeSeriesService.GetEnsembleAggregatedValues(tsIdList, group.ElementAt(0).AggregationType, from, to, user);
                                    foreach (var value in aggregatedValues)
                                    {
                                        var indicator = group.FirstOrDefault(i => Equals((TTimeSeriesId) ResolveTimeSeriesId(i.DataSource.EntityId, path), value.Key));
                                        indicatorValue = GetIndicatorValue(indicator, value.Value);
                                        AddColor(indicatorValue, colorList, indicator);
                                    }
                                }
                            }

                            break;

                        default:
                            throw new NotSupportedException($"Data source type '{type.Key}' is not supported.");
                    }
                }
            }

            return colorList;
        }

        /// <summary>
        ///     Gets a collection of features, with the indicator status
        ///     In no group is specified, features for all places is returned.
        ///     If a group is specified, only the features for places within the specified group is returned.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="offsetDateTime">Must be given If the indicator time interval is relative to a specified datetime</param>
        /// <param name="path">Path to inject into the entity ID</param>
        /// <param name="user">The user.</param>
        public FeatureCollection GetFeaturesWithIndicatorStatus(string? group = null, DateTime? offsetDateTime = null, string? path = null, ClaimsPrincipal? user = null)
        {
            var featureCollection = GetFeatures(group, user);
            var indicators = group is null ? GetIndicators(user) : GetIndicatorsByGroup(group, user);
            var indicatorList = new List<Indicator>();
            foreach (var keyValue in indicators)
            {
                indicatorList.AddRange(keyValue.Value.Values);
            }

            var statusList = GetIndicatorStatus(indicatorList, offsetDateTime, path, user);
            return featureCollection.AddIndicatorsWithStatus(indicators, statusList);
        }

        /// <summary>
        ///     Gets a collection of features, with the indicator status evaluated over period from-to.
        ///     In no group is specified, features for all places is returned.
        ///     If a group is specified, only the features for places within the specified group is returned.
        /// </summary>
        /// <param name="from">time period start</param>
        /// <param name="to">time period end</param>
        /// <param name="group">The group.</param>
        /// <param name="path">Path to inject into the entity ID</param>
        /// <param name="user">The user.</param>
        public FeatureCollection GetFeaturesWithIndicatorStatus(DateTime from, DateTime to, string? group = null, string? path = null, ClaimsPrincipal? user = null)
        {
            var featureCollection = GetFeatures(group, user);
            var indicators = group is null ? GetIndicators(user) : GetIndicatorsByGroup(group, user);
            var indicatorList = new List<Indicator>();
            foreach (var keyValue in indicators)
            {
                indicatorList.AddRange(keyValue.Value.Values);
            }

            var statusList = GetIndicatorStatus(indicatorList, from, to, path, user);
            return featureCollection.AddIndicatorsWithStatus(indicators, statusList);
        }

        /// <summary>
        ///     Gets a collection of features
        ///     In no group is specified, features for all places is returned.
        ///     If a group is specified, only the features for places within the specified group is returned.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="user">The user.</param>
        public FeatureCollection GetFeatures(string? group = null, ClaimsPrincipal? user = null)
        {
            var featureCollection = new FeatureCollection();
            var places = group is null ? GetAll(user) : GetByGroup(group, user);
            foreach (var place in places)
            {
                var featureId = place.FeatureId;
                var query = new Query<IFeature>
                {
                    new(featureId.AttributeKey, QueryOperator.Equal, featureId.AttributeValue)
                };
                var feature = _gisService.Get(featureId.FeatureCollectionId, query, false).Features.SingleOrDefault();

                if (feature != null)
                {
                    feature.AttributeValues["placeId"] = place.Id;
                    feature.AttributeValues["fullName"] = place.FullName;
                    feature.AttributeValues["name"] = place.Name;
                    feature.AttributeValues["groupLayer"] = place.FeatureId.FeatureCollectionId.ToString().Split('/').Last();
                    featureCollection.Features.Add(feature);
                }
            }

            return featureCollection;
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IPlaceRepository<TCollectionId>>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string? path)
        {
            return Service.GetProviderTypes<IPlaceRepository<TCollectionId>>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">
        ///     The path where to look for compatible providers. If path is null, the path of the executing assembly
        ///     is used.
        /// </param>
        /// <param name="searchPattern">
        ///     File name search pattern. Can contain a combination of valid literal path and wild card
        ///     (* and ?) characters.
        /// </param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IPlaceRepository<TCollectionId>>(path, searchPattern);
        }

        internal double? GetIndicatorValue(Indicator indicator, IList<double?> aggregatedValues)
        {
            double? indicatorValue = null;
            if (indicator.Quantile != null)
            {
                indicatorValue = CalculateQuantile(aggregatedValues.ToList(), (double) indicator.Quantile);
            }
            else
            {
                if (indicator.AggregationType == null)
                {
                    throw new ArgumentException("Indicator is missing an aggregation type.");
                }
                else if (indicator.AggregationType.Equals(AggregationType.Maximum))
                {
                    indicatorValue = aggregatedValues.Max();
                }
                else if (indicator.AggregationType.Equals(AggregationType.Minimum))
                {
                    indicatorValue = aggregatedValues.Min();
                }
                else if (indicator.AggregationType.Equals(AggregationType.Average))
                {
                    indicatorValue = aggregatedValues.Average();
                }
                else if (indicator.AggregationType.Equals(AggregationType.Sum))
                {
                    indicatorValue = aggregatedValues.Sum();
                }
            }

            return indicatorValue;
        }

        internal static object ResolveTimeSeriesId(object timeSeriesId, string? path)
        {
            if (timeSeriesId is string id && id.StartsWith("[Path]"))
            {
                Guard.Against.NullOrEmpty(path, nameof(path));
                timeSeriesId = id.Replace("[Path]", path);
            }

            return timeSeriesId;
        }

        internal double? CalculateQuantile(List<double?> valueList, double quantile)
        {
            if (valueList.Count <= 1)
            {
                return null;
            }

            var sortedList = valueList;
            sortedList.Sort();
            var index = (sortedList.Count - 1) * quantile;
            var lo = Convert.ToInt32(Math.Floor(index));
            var hi = Convert.ToInt32(Math.Ceiling(index));
            var qs = sortedList[lo];
            var h = index - lo;

            return (1 - h) * qs + h * sortedList[hi];
        }

        private static (DateTime from, DateTime to) GetPeriod(TimeInterval timeInterval, DateTime? offsetDateTime)
        {
            (DateTime from, DateTime to) period;
            switch (timeInterval.Type)
            {
                case TimeIntervalType.Fixed:
                case TimeIntervalType.RelativeToNow:
                    period = timeInterval.ToPeriod();
                    break;
                case TimeIntervalType.RelativeToDateTime:
                    period = timeInterval.ToPeriod(offsetDateTime);
                    break;
                case TimeIntervalType.All:
                    period = (DateTime.MinValue, DateTime.MaxValue);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return period;
        }

        private static void AddColor(double? indicatorValue, IDictionary<Indicator, Maybe<SKColor>> colorList, Indicator indicator)
        {
            if (indicatorValue is null)
            {
                colorList.Add(indicator, Maybe.Empty<SKColor>());
            }
            else
            {
                colorList.Add(indicator, indicator.GetPalette().GetColor(indicatorValue.Value).ToMaybe());
            }
        }

        private void CheckIndicator(Indicator indicator)
        {
            var connectionId = indicator.DataSource.ConnectionId;
            switch (indicator.DataSource.Type)
            {
                case DataSourceType.Scalar:
                    if (_scalarServices is null || !_scalarServices.ContainsKey(connectionId))
                    {
                        throw new ArgumentException($"No scalar service connection with ID '{connectionId}' is defined.", nameof(indicator));
                    }

                    var scalarId = (TScalarId) indicator.DataSource.EntityId;
                    var scalarService = _scalarServices[connectionId];
                    if (!scalarService.Exists(scalarId))
                    {
                        throw new ArgumentException($"No scalar with ID '{scalarId}' is defined in scalar service with connection ID '{connectionId}'.", nameof(indicator));
                    }

                    var valueTypeName = scalarService.Get(scalarId).ValueTypeName;
                    if (valueTypeName != "System.Double")
                    {
                        throw new ArgumentException($"Scalar with value type name '{valueTypeName}' is not supported. Only 'System.Double' is supported.", nameof(indicator));
                    }

                    break;
                case DataSourceType.TimeSeries:
                case DataSourceType.EnsembleTimeSeries:
                    if (_timeSeriesServices is null || !_timeSeriesServices.ContainsKey(connectionId))
                    {
                        throw new ArgumentException($"No time series service connection with ID '{connectionId}' is defined.", nameof(indicator));
                    }

                    var timeSeriesId = (TTimeSeriesId) indicator.DataSource.EntityId;

                    if (timeSeriesId.ToStandardString().Contains("[Path]"))
                    {
                        break;
                    }

                    if (!_timeSeriesServices[connectionId].Exists(timeSeriesId))
                    {
                        throw new ArgumentException($"No time series with ID '{timeSeriesId}' is defined in time series service with connection ID '{connectionId}'.", nameof(indicator));
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class PlaceService : PlaceService<string, string, int, string>
    {
        public PlaceService(IPlaceRepository<string> repository,
            Dictionary<string, IDiscreteTimeSeriesService<string, double>>? timeSeriesServices,
            Dictionary<string, IScalarService<string, int>>? scalarServices,
            IGisService<string> gisService) : base(repository, timeSeriesServices, scalarServices, gisService)
        {
        }
    }
}