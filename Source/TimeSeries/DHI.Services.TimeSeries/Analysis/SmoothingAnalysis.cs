namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    ///     Time series data extension method for smoothing analysis.
    /// </summary>
    public static class SmoothingAnalysis
    {
        /// <summary>
        ///     Smooth the input data and reserve the peaks using the Savitsky-Golay filter.
        /// </summary>
        /// <param name="values">Input data.</param>
        /// <param name="window">An odd number of data points used to estimate the central data.</param>
        /// <param name="order">The order of the estimating polynomial.</param>
        /// <returns>List of doubles.</returns>
        public static List<double> SavitskyGolay(this IEnumerable<double> values, int window, int order = 2)
        {
            if (window % 2 < 1)
            {
                throw new ArgumentException("The window length must be odd.", nameof(window));
            }

            if (order > 5 || order < 0)
            {
                throw new ArgumentException("Fitting polynomial order should be between 0 and 5, order higher than 5 is not supported.", nameof(order));
            }

            if (window <= order + 1)
            {
                throw new ArgumentException("The window length must be higher than the order of the fitting polynomial.");
            }

            var valueList = values as IList<double> ?? values.ToList();
            if (valueList.Count <= window)
            {
                throw new Exception("There should be more values than the window length.");
            }

            var coefficients = CalculateCoefficients(window, order);
            var extended = ExtendData(valueList, window);
            var smoothed = new List<double>();

            for (var i = 0; i < valueList.Count; i++)
            {
                var windowInputs = extended.Skip(i).Take(window).ToArray();
                var smoothedValue = CalculateSmoothedValue(windowInputs, coefficients);
                smoothed.Add(smoothedValue);
            }

            return smoothed;
        }

        /// <summary>
        ///     Smooth the time series data and reserve the peaks using the Savitsky-Golay filter.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="window">An odd number of data points used to estimate the central data.</param>
        /// <param name="order">The order of the estimating polynomial.</param>
        /// <returns>ITimeSeriesData&lt;System.Double&gt;.</returns>
        public static TimeSeriesData<double> Smoothing(this ITimeSeriesData<double> timeSeriesData, int window, int order = 2)
        {
            if (!timeSeriesData.DateTimes.Any())
            {
                return new TimeSeriesData<double>();
            }

            if (window % 2 < 1)
            {
                throw new ArgumentException("The window length must be odd.", nameof(window));
            }

            if (order > 5 || order < 0)
            {
                throw new ArgumentException("Fitting polynomial order should be between 0 and 5, order higher than 5 is not supported.", nameof(order));
            }

            if (window <= order + 1)
            {
                throw new ArgumentException("The window length must be higher than the order of the fitting polynomial.");
            }

            if (timeSeriesData.Values.Count(v => v != null) <= window)
            {
                throw new Exception("There should be more values that are not null than the window length.");
            }

            var timeSeriesFilled = timeSeriesData.Values.Contains(null) ? timeSeriesData.GapFill() : timeSeriesData;

            List<double> smoothedValues;
            if (timeSeriesData.DateTimes.Count > 20000)
            {
                const int chunkSize = 10000;
                var chunks = timeSeriesFilled.ToChunks(chunkSize);
                var smoothedChunks = new double[timeSeriesData.DateTimes.Count];
                Parallel.For(0, chunks.Count, i =>
                {
                    var chunk = chunks[i].Values.Select(v => (double)v);
                    var smoothedChunk = chunk.SavitskyGolay(window, order).ToArray();
                    var startIndex = i * chunkSize;
                    smoothedChunk.CopyTo(smoothedChunks, startIndex);
                });

                smoothedValues = smoothedChunks.ToList();
            }
            else
            {
                var values = timeSeriesFilled.Values.Select(v => (double)v);
                smoothedValues = values.SavitskyGolay(window, order);
            }

            var smoothed = new TimeSeriesData<double>(timeSeriesData.DateTimes, smoothedValues.Select(v => (double?)v).ToList());
            return smoothed;
        }

        internal static double[] CalculateCoefficients(int window, int order)
        {
            var coefficients = new double[window];
            var m = (window - 1) / 2;
            var windowIds = Enumerable.Range(-m, window).ToArray();

            //Equations from
            //Madden, Hannibal H. (1978). "Comments on the Savitzky–Golay convolution method for least-squares-fit smoothing and differentiation of digital data".
            //Anal. Chem. 50 (9): 1383–6. doi:10.1021/ac50031a048.
            switch (order)
            {
                case 0:
                case 1:
                    for (var i = 0; i < window; i++)
                    {
                        coefficients[i] = 1 / Convert.ToDouble(window);
                    }

                    break;

                case 2:
                case 3:
                    for (var i = 0; i < windowIds.Length; i++)
                    {
                        coefficients[i] = 3 * (3 * Math.Pow(m, 2) + 3 * m - 1 - 5 * Math.Pow(windowIds[i], 2)) /
                                          ((2 * m + 3) * (2 * m + 1) * (2 * m - 1));
                    }

                    break;

                case 4:
                case 5:
                    for (var i = 0; i < windowIds.Length; i++)
                    {
                        var a = 15 * Math.Pow(m, 4) + 30 * Math.Pow(m, 3) - 35 * Math.Pow(m, 2) - 50 * m + 12 -
                                35 * (2 * Math.Pow(m, 2) + 2 * m - 3) * Math.Pow(windowIds[i], 2) +
                                63 * Math.Pow(windowIds[i], 4);
                        var b = (2 * m + 5) * (2 * m + 3) * (2 * m + 1) * (2 * m - 1) * (2 * m - 3);
                        coefficients[i] = 15 * a / (4 * b);
                    }

                    break;
            }

            return coefficients;
        }

        private static double CalculateSmoothedValue(double[] windowInputs, double[] coefficients)
        {
            var smoothed = 0.0;
            for (var i = 0; i < windowInputs.Length; i++)
            {
                smoothed += windowInputs[i] * coefficients[i];
            }

            return smoothed;
        }

        // Extend the input data with the mirrored m (m = (window - 1)/2 ) data points at the beginning of the data and m data points at the end of the data.
        // The input data is extended so that the beginning and finishing m points can also be estimated with the polynomial.
        private static List<double> ExtendData(IList<double> input, int window)
        {
            var m = (window - 1) / 2;
            var insert = input.Skip(1).Take(m).Reverse();
            var append = input.Skip(input.Count - m).Take(m).Reverse();
            var extended = insert.Concat(input).Concat(append).ToList();
            return extended;
        }
    }
}