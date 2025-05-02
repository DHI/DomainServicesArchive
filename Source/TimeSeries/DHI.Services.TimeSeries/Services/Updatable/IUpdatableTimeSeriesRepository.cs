namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface IUpdatableTimeSeriesRepository<TId, TValue> : IDiscreteTimeSeriesRepository<TId, TValue>, IUpdatableRepository<TimeSeries<TId, TValue>, TId> where TValue : struct, IComparable<TValue>
    {
        void Add(IEnumerable<TimeSeries<TId, TValue>> timeSeriesList, ClaimsPrincipal user = null);

        void Update(IEnumerable<TimeSeries<TId, TValue>> timeSeriesList, ClaimsPrincipal user = null);

        void Remove(IEnumerable<TId> ids, ClaimsPrincipal user = null);

        void RemoveValues(TId id, ClaimsPrincipal user = null);

        void RemoveValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null);
    }
}