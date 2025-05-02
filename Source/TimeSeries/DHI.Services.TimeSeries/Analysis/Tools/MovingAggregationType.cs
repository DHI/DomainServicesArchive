namespace DHI.Services.TimeSeries
{
    public enum MovingAggregationType
    {
        /// <summary>
        ///     Determine if the moving aggregation should be performed on data before the date time
        /// </summary>
        Backwards,

        /// <summary>
        ///     Determine if the moving aggregation should be performed on data ahead the date time
        /// </summary>
        Forward,

        /// <summary>
        ///     Determine if the moving aggregation should be performed equally around date time
        /// </summary>
        Middle
    }
}