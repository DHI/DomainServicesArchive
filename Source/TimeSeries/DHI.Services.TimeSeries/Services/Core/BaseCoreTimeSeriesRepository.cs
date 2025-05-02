namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public abstract class BaseCoreTimeSeriesRepository<TId, TValue> : ICoreTimeSeriesRepository<TId, TValue> where TValue : struct, IComparable<TValue>
    {
        public abstract Maybe<TimeSeries<TId, TValue>> Get(TId id, ClaimsPrincipal user = null);

        public abstract Maybe<DataPoint<TValue>> GetValue(TId id, DateTime dateTime, ClaimsPrincipal user = null);

        public abstract Maybe<ITimeSeriesData<TValue>> GetValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null); 

        public virtual IDictionary<TId, ITimeSeriesData<TValue>> GetValues(IEnumerable<TId> ids, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            var dictionary = new Dictionary<TId, ITimeSeriesData<TValue>>();
            foreach (var id in ids)
            {
                var maybe = GetValues(id, from, to, user);
                if (maybe.HasValue)
                {
                    dictionary[id] = maybe.Value;
                }
            }

            return dictionary;
        }

        public virtual TValue? GetAggregatedValue(TId id, AggregationType aggregationType, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            var maybe = GetValues(id, from, to, user);
            return !maybe.HasValue ? null : aggregationType.GetValue(maybe.Value);
        }

        public virtual IDictionary<TId, TValue?> GetAggregatedValues(IEnumerable<TId> ids, AggregationType aggregationType, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            var values = new Dictionary<TId, TValue?>();
            foreach (var id in ids)
            {
                var maybe = GetValues(id, from, to, user);
                if (maybe.HasValue)
                {
                    values.Add(id, aggregationType.GetValue(maybe.Value));
                }
            }

            return values;
        }

        public virtual IList<TValue?> GetEnsembleAggregatedValues(TId id, AggregationType aggregationType, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            throw new NotSupportedException("This repository does not support ensemble time series.");
        }

        public virtual IDictionary<TId, IList<TValue?>> GetEnsembleAggregatedValues(IEnumerable<TId> ids, AggregationType aggregationType, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            throw new NotSupportedException("This repository does not support ensemble time series.");
        }

    }
}