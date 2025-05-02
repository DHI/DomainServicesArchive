namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public abstract class BaseGroupedDiscreteTimeSeriesRepository<TId, TValue> : BaseDiscreteTimeSeriesRepository<TId, TValue>, IGroupedDiscreteTimeSeriesRepository<TId, TValue> where TValue : struct, IComparable<TValue>
    {
        public abstract bool ContainsGroup(string group, ClaimsPrincipal user = null);

        public abstract IEnumerable<TimeSeries<TId, TValue>> GetByGroup(string group, ClaimsPrincipal user = null);

        public virtual IEnumerable<string> GetFullNames(string group, ClaimsPrincipal user = null)
        {
            return GetByGroup(group, user).Select(timeSeries => timeSeries.FullName).ToArray();
        }

        public virtual IEnumerable<string> GetFullNames(ClaimsPrincipal user = null)
        {
            return GetAll(user).Select(timeSeries => timeSeries.FullName).ToArray();
        }
    }
}