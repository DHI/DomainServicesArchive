namespace DHI.Services.TimeSeries
{
    using System.Collections.Generic;

    /// <summary>
    ///     Interface ITimeSeriesDataWFlag
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <typeparam name="TFlag">The type of the flags.</typeparam>
    /// <seealso cref="ITimeSeriesData{TValue}" />
    public interface ITimeSeriesDataWFlag<TValue, TFlag> : ITimeSeriesData<TValue> where TValue : struct
    {
        /// <summary>
        ///     Gets the flags.
        /// </summary>
        IList<TFlag> Flags { get; }
    }
}