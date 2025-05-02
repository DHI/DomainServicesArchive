namespace DHI.Services.TimeSeries
{
    /// <summary>
    ///     Enum TimeStepTrendType
    /// </summary>
    public enum TimeStepTrendType
    {
        /// <summary>
        ///     Determine if the trend calculation should be compared before the date time
        /// </summary>
        Backwards,

        /// <summary>
        ///     Determine if the trend calculation should be compared after the date time
        /// </summary>
        Forward
    }
}