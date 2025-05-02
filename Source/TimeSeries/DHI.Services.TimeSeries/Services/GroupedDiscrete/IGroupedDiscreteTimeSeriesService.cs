namespace DHI.Services.TimeSeries
{
    /// <summary>
    ///     Interface IGroupedDiscreteTimeSeriesService
    /// </summary>
    /// <typeparam name="TId">The type of the time series identifier.</typeparam>
    /// <typeparam name="TValue">The type of the time series values.</typeparam>
    public interface IGroupedDiscreteTimeSeriesService<TId, TValue> : IDiscreteTimeSeriesService<TId, TValue>, IGroupedService<TimeSeries<TId, TValue>> where TValue : struct
    {
    }
}