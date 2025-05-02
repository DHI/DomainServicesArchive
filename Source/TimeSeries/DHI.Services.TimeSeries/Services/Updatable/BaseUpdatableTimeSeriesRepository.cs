namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;

    public abstract class BaseUpdatableTimeSeriesRepository<TId, TValue> : BaseDiscreteTimeSeriesRepository<TId, TValue>, IUpdatableTimeSeriesRepository<TId, TValue> where TValue : struct, IComparable<TValue>
    {
        public abstract void Add(TimeSeries<TId, TValue> entity, ClaimsPrincipal user = null);

        public abstract void Remove(TId id, ClaimsPrincipal user = null);

        public abstract void Update(TimeSeries<TId, TValue> entity, ClaimsPrincipal user = null);

        public virtual void Add(IEnumerable<TimeSeries<TId, TValue>> timeSeriesList, ClaimsPrincipal user = null)
        {
            foreach (var timeSeries in timeSeriesList)
            {
                Add(timeSeries, user);
            }
        }

        public virtual void Update(IEnumerable<TimeSeries<TId, TValue>> timeSeriesList, ClaimsPrincipal user = null)
        {
            foreach (var timeSeries in timeSeriesList)
            {
                Update(timeSeries, user);
            }
        }

        public virtual void Remove(IEnumerable<TId> ids, ClaimsPrincipal user = null)
        {
            foreach (var id in ids)
            {
                Remove(id, user);
            }
        }

        public abstract void RemoveValues(TId id, ClaimsPrincipal user = null);

        public abstract void RemoveValues(TId id, DateTime from, DateTime to, ClaimsPrincipal user = null);
    }
}