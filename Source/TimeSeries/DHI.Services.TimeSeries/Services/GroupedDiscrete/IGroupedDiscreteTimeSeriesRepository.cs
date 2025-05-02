namespace DHI.Services.TimeSeries
{
    using System;

    public interface IGroupedDiscreteTimeSeriesRepository<TId, TValue> : IDiscreteTimeSeriesRepository<TId, TValue>, IGroupedRepository<TimeSeries<TId, TValue>> where TValue : struct, IComparable<TValue>
    {
    }
}