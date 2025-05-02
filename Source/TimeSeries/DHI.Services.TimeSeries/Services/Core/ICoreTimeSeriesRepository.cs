namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface ICoreTimeSeriesRepository<TId, TValue> : IRepository<TimeSeries<TId, TValue>, TId> where TValue : struct, IComparable<TValue>
    {
        Maybe<DataPoint<TValue>> GetValue(TId id, DateTime dateTime, ClaimsPrincipal user = null);

        Maybe<ITimeSeriesData<TValue>> GetValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null);

        IDictionary<TId, ITimeSeriesData<TValue>> GetValues(IEnumerable<TId> ids, DateTime from, DateTime to, ClaimsPrincipal user = null);
        
        TValue? GetAggregatedValue(TId id, AggregationType aggregationType, DateTime from, DateTime to, ClaimsPrincipal user = null);

        IDictionary<TId, TValue?> GetAggregatedValues(IEnumerable<TId> ids, AggregationType aggregationType, DateTime from, DateTime to, ClaimsPrincipal user = null);

        IList<TValue?> GetEnsembleAggregatedValues(TId id, AggregationType aggregationType, DateTime from, DateTime to, ClaimsPrincipal user = null);

        IDictionary<TId, IList<TValue?>> GetEnsembleAggregatedValues(IEnumerable<TId> ids, AggregationType aggregationType, DateTime from, DateTime to, ClaimsPrincipal user = null);
    }
}