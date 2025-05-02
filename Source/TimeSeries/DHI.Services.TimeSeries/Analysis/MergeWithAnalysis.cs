namespace DHI.Services.TimeSeries
{
    /// <summary>
    /// Class MergeWithAnalysis.
    /// </summary>
    public static class MergeWithAnalysis
    {
        /// <summary>
        ///     Merges some time series data with some other time series data.
        ///     Other overlapping time series values are overwritten.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="other">The other time series data.</param>
        /// <returns>A value tuple with the merged time series data and a count of the overwritten and appended time steps.</returns>
        public static (ITimeSeriesData<double> data, int overwriteCount, int appendCount) MergeWith(this ITimeSeriesData<double> data, ITimeSeriesData<double> other)
        {
            var points = data.ToSortedSet();
            var appendCount = 0;
            var overwriteCount = 0;
            foreach (var point in points)
            {
                var index = other.DateTimes.IndexOf(point.DateTime);
                if (index != -1)
                {
                    overwriteCount++;
                    other.Values[index] = point.Value;
                }
                else
                {
                    appendCount++;
                    other.Append(point.DateTime, point.Value);
                }
            }

            var otherSortedSet = other.ToSortedSet();
            return (new TimeSeriesData<double>(otherSortedSet), overwriteCount, appendCount);
        }


        /// <summary>
        ///     Merges some time series data with some other time series data.
        ///     Other overlapping time series values are overwritten.
        /// </summary>
        /// <param name="data">The time series data.</param>
        /// <param name="other">The other time series data.</param>
        /// <returns>A value tuple with the merged time series data and a count of the overwritten and appended time steps.</returns>
        public static (ITimeSeriesDataWFlag<double, TFlag> data, int overwriteCount, int appendCount) MergeWith<TFlag>(this ITimeSeriesDataWFlag<double, TFlag> data, ITimeSeriesDataWFlag<double, TFlag> other)
        {
            var points = data.ToSortedSet();
            var appendCount = 0;
            var overwriteCount = 0;
            foreach (var point in points)
            {
                var index = other.DateTimes.IndexOf(point.DateTime);
                if (index != -1)
                {
                    overwriteCount++;
                    other.Values[index] = point.Value;
                    other.Flags[index] = point.Flag;
                }
                else
                {
                    appendCount++;
                    other.Append(point.DateTime, point.Value, point.Flag);
                }
            }

            var otherSortedSet = other.ToSortedSet();
            return (new TimeSeriesDataWFlag<double, TFlag>(otherSortedSet), overwriteCount, appendCount);
        }
    }
}
