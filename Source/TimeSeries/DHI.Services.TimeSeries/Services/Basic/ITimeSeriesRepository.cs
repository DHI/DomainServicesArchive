namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface ITimeSeriesRepository<TId, TValue> : ICoreTimeSeriesRepository<TId, TValue> where TValue : struct, IComparable<TValue>
    {
        Maybe<TimeSeries<TId, TValue>> GetWithValues(TId id, ClaimsPrincipal user = null);

        Maybe<TimeSeries<TId, TValue>> GetWithValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null);

        IDictionary<TId, TimeSeries<TId, TValue>> GetWithValues(IEnumerable<TId> ids, ClaimsPrincipal user = null);

        IDictionary<TId, TimeSeries<TId, TValue>> GetWithValues(IEnumerable<TId> ids, DateTime from, DateTime to, ClaimsPrincipal user = null);

        bool ContainsDateTime(TId id, DateTime dateTime, ClaimsPrincipal user = null);

        Maybe<SortedSet<DateTime>> GetDateTimes(TId id, ClaimsPrincipal user = null);

        Maybe<DateTime> GetFirstDateTime(TId id, ClaimsPrincipal user = null);

        IDictionary<TId, DataPoint<TValue>> GetFirstValue(IEnumerable<TId> ids, ClaimsPrincipal user = null);

        Maybe<DataPoint<TValue>> GetFirstValue(TId id, ClaimsPrincipal user = null);

        Maybe<DataPoint<TValue>> GetFirstValueAfter(TId id, DateTime dateTime, ClaimsPrincipal user = null);

        Maybe<DateTime> GetLastDateTime(TId id, ClaimsPrincipal user = null);

        IDictionary<TId, DataPoint<TValue>> GetLastValue(IEnumerable<TId> ids, ClaimsPrincipal user = null);

        Maybe<DataPoint<TValue>> GetLastValue(TId id, ClaimsPrincipal user = null);

        Maybe<DataPoint<TValue>> GetLastValueBefore(TId id, DateTime dateTime, ClaimsPrincipal user = null);

        Maybe<ITimeSeriesData<TValue>> GetValues(TId id, ClaimsPrincipal user = null);

        IDictionary<TId, ITimeSeriesData<TValue>> GetValues(IEnumerable<TId> ids, ClaimsPrincipal user = null);
    }
}