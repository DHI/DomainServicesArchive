namespace DHI.Services.TimeSeries
{
    /// <summary>
    ///     Interface IGroupedUpdatableTimeSeriesService
    /// </summary>
    /// <typeparam name="TId">The type of the time series identifiers.</typeparam>
    /// <typeparam name="TValue">The type of the time series values.</typeparam>
    public interface IGroupedUpdatableTimeSeriesService<TId, TValue> : IUpdatableTimeSeriesService<TId, TValue>, IGroupedService<TimeSeries<TId, TValue>> where TValue : struct
    {
    }
}