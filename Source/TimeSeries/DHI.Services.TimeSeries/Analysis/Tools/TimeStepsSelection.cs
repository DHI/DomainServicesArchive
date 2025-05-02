namespace DHI.Services.TimeSeries
{
    /// <summary>
    ///     Time Steps Selection type
    /// </summary>
    public enum TimeStepsSelection
    {
        /// <summary>
        ///     All time steps in all time series
        /// </summary>
        All,

        /// <summary>
        ///     The common time steps only
        /// </summary>
        CommonOnly,

        /// <summary>
        ///     The time steps from the first time series only
        /// </summary>
        FirstOnly
    }
}