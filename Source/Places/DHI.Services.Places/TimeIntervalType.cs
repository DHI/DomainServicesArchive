namespace DHI.Services.Places
{
    /// <summary>
    ///     Time interval types for defining temporal subsets.
    /// </summary>
    public enum TimeIntervalType
    {
        /// <summary>
        ///     Fixed start and end date/time.
        /// </summary>
        Fixed,

        /// <summary>
        ///     Relative to now.
        /// </summary>
        RelativeToNow,

        /// <summary>
        ///     Relative to a specified date/time.
        /// </summary>
        RelativeToDateTime,

        /// <summary>
        ///     All data in time series.
        /// </summary>
        All
    }
}