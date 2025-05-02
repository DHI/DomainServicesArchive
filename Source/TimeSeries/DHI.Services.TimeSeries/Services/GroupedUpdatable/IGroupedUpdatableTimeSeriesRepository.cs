namespace DHI.Services.TimeSeries
{
    using System;

    public interface IGroupedUpdatableTimeSeriesRepository<TId, TValue> : IGroupedDiscreteTimeSeriesRepository<TId, TValue>, IUpdatableTimeSeriesRepository<TId, TValue> where TValue : struct, IComparable<TValue>
    {
    }
}