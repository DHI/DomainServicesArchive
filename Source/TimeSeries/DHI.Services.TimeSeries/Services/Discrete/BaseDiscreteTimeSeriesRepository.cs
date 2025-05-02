namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public abstract class BaseDiscreteTimeSeriesRepository<TId, TValue> : BaseTimeSeriesRepository<TId, TValue>, IDiscreteTimeSeriesRepository<TId, TValue> where TValue : struct, IComparable<TValue>
    {
        public virtual int Count(ClaimsPrincipal user = null)
        {
            return GetAll(user).Count();
        }

        public virtual bool Contains(TId id, ClaimsPrincipal user = null)
        {
            return Get(id, user).HasValue;
        }

        public override Maybe<TimeSeries<TId, TValue>> Get(TId id, ClaimsPrincipal user = null)
        {
            var timeSeries = GetAll().FirstOrDefault(e => e.Id.Equals(id));
            return timeSeries?.ToMaybe() ?? Maybe.Empty<TimeSeries<TId, TValue>>();
        }

        public abstract IEnumerable<TimeSeries<TId, TValue>> GetAll(ClaimsPrincipal user = null);

        public virtual IEnumerable<TId> GetIds(ClaimsPrincipal user = null)
        {
            return GetAll(user).Select(e => e.Id).ToArray();
        }

        public abstract override Maybe<ITimeSeriesData<TValue>> GetValues(TId id, ClaimsPrincipal user = null);

        public abstract void SetValues(TId id, ITimeSeriesData<TValue> data, ClaimsPrincipal user = null);
    }
}