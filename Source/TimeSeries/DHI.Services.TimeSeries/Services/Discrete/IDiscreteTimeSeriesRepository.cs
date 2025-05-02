namespace DHI.Services.TimeSeries
{
    using System;
    using System.Security.Claims;

    public interface IDiscreteTimeSeriesRepository<TId, TValue> : ITimeSeriesRepository<TId, TValue>, IDiscreteRepository<TimeSeries<TId, TValue>, TId> where TValue : struct, IComparable<TValue>
    {
        void SetValues(TId id, ITimeSeriesData<TValue> data, ClaimsPrincipal user = null);
    }
}