namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    ///     Time series data extension method for reduction.
    /// </summary>
    public static class ReduceAnalysis
    {
        /// <summary>
        ///     Reduces the specified time series data using the Ramer–Douglas–Peucker algorithm
        ///     but maintains the general shape of the series.
        /// </summary>
        /// <param name="timeSeriesData">The time series data.</param>
        /// <param name="relativeTolerance">
        ///     The tolerance expressed as a percentage of the difference between maximum and minimum value of the input data.
        /// </param>
        /// <param name="minimumCount">The required minimum number of data points to trigger a reduction</param>
        /// <returns>ITimeSeriesData&lt;System.Double&gt;.</returns>
        /// <exception cref="Exception">There should be at least three values in the time series.</exception>
        /// <exception cref="Exception">The relative tolerance should be higher than 0 and lower than 100.</exception>
        public static TimeSeriesData<double> Reduce(this ITimeSeriesData<double> timeSeriesData, double relativeTolerance = 2, int minimumCount = 3000)
        {
            // Adapted code from 'Iterative version of Ramer-Douglas-Peucker line-simplification algorithm' from namekdev.net
            // https://www.namekdev.net/2014/06/iterative-version-of-ramer-douglas-peucker-line-simplification-algorithm/

            if (!timeSeriesData.DateTimes.Any())
            {
                return new TimeSeriesData<double>();
            }

            if (minimumCount < 3)
            {
                throw new ArgumentException("The required minimum number of data points to trigger a reduction should be at least 3.", nameof(minimumCount));
            }

            if (relativeTolerance <= 0.0 || relativeTolerance > 100.0)
            {
                throw new ArgumentException("The relative tolerance should be higher than 0 and lower than 100.", nameof(relativeTolerance));
            }

            if (timeSeriesData.Values.Count <= minimumCount)
            {
                return timeSeriesData as TimeSeriesData<double>;
            }

            var tolerance = timeSeriesData.GetTolerance(relativeTolerance);
            var reducedIndexes = new bool[timeSeriesData.DateTimes.Count];

            if (timeSeriesData.DateTimes.Count > 20000)
            {
                const int chunkSize = 10000;
                var chunks = timeSeriesData.ToChunks(chunkSize);
                Parallel.For(0, chunks.Count, i =>
                {
                    var chunk = chunks[i];
                    var startIndex = i * chunkSize;
                    var chunckReducedIndexes = DouglasPeucker(chunk, tolerance);
                    chunckReducedIndexes.CopyTo(reducedIndexes, startIndex);
                });
            }
            else
            {
                var list = DouglasPeucker(timeSeriesData, tolerance);
                list.CopyTo(reducedIndexes, 0);
            }

            var reduced = timeSeriesData.ToReducedTimeSeries(reducedIndexes);
            return reduced;
        }

        internal static BitArray DouglasPeucker(ITimeSeriesData<double> timeSeriesData, double tolerance)
        {
            var startIndex = 0;
            var endIndex = timeSeriesData.DateTimes.Count - 1;
            var stack = new Stack<Tuple<int, int>>();
            stack.Push(new Tuple<int, int>(startIndex, endIndex));

            var reducedIndexes = new BitArray(timeSeriesData.DateTimes.Count, true);
            reducedIndexes.IgnoreNulls(timeSeriesData);

            while (stack.Count > 0)
            {
                var startEndIndexes = stack.Pop();
                startIndex = startEndIndexes.Item1;
                endIndex = startEndIndexes.Item2;

                if (!reducedIndexes[startIndex] || !reducedIndexes[endIndex])
                {
                    continue;
                }

                var baseLine = timeSeriesData.GetBaseLine(startIndex, endIndex);

                var dmax = timeSeriesData.GetMaxDistance(baseLine, reducedIndexes, out var index);

                if (dmax > tolerance)
                {
                    stack.Push(new Tuple<int, int>(startIndex, index));
                    stack.Push(new Tuple<int, int>(index, endIndex));
                }
                else
                {
                    for (var i = startIndex + 1; i < endIndex; i++)
                    {
                        reducedIndexes[i] = false;
                    }
                }
            }

            return reducedIndexes;
        }

        internal static double GetTolerance(this ITimeSeriesData<double> timeSeriesData, double relativeTolerance)
        {
            return Math.Abs(timeSeriesData.Maximum().Value - timeSeriesData.Minimum().Value) * relativeTolerance / 100;
        }

        internal static BaseLine GetBaseLine(this ITimeSeriesData<double> timeSeriesData, int startIndex, int endIndex)
        {
            var startTime = timeSeriesData.DateTimes[startIndex];
            var endTime = timeSeriesData.DateTimes[endIndex];
            var startValue = timeSeriesData.Values[startIndex].Value;
            var endValue = timeSeriesData.Values[endIndex].Value;
            var slope = (endValue - startValue) / (endTime - startTime).Ticks;
            var baseLine = new BaseLine {StartTime = startTime, StartValue = startValue, Slope = slope, StartIndex = startIndex, EndIndex = endIndex};
            return baseLine;
        }

        internal static double GetMaxDistance(this ITimeSeriesData<double> timeSeriesData, BaseLine baseLine, BitArray bitArray, out int index)
        {
            var dmax = 0.0;
            index = baseLine.StartIndex;
            for (var i = baseLine.StartIndex + 1; i < baseLine.EndIndex; i++)
            {
                if (!bitArray[i])
                {
                    continue;
                }

                var pointTime = timeSeriesData.DateTimes[i];
                var pointValue = timeSeriesData.Values[i].Value;
                var a = baseLine.Slope;
                var b = baseLine.StartValue;
                var startTime = baseLine.StartTime;

                var d = Math.Abs(pointValue - (a * (pointTime - startTime).Ticks + b));

                if (d > dmax)
                {
                    dmax = d;
                    index = i;
                }
            }

            return dmax;
        }

        internal static List<TimeSeriesData<double>> ToChunks(this ITimeSeriesData<double> timeSeriesData, int chunkSize)
        {
            var startIndex = 0;
            var chunks = new List<TimeSeriesData<double>>();
            var endIndex = startIndex;
            while (endIndex < timeSeriesData.DateTimes.Count)
            {
                endIndex = startIndex + chunkSize - 1;
                var chunk = new TimeSeriesData<double>();

                if (endIndex > timeSeriesData.DateTimes.Count)
                {
                    for (var i = startIndex; i < timeSeriesData.DateTimes.Count; i++)
                    {
                        chunk.Append(timeSeriesData.DateTimes[i], timeSeriesData.Values[i]);
                    }
                }
                else
                {
                    for (var i = startIndex; i <= endIndex; i++)
                    {
                        chunk.Append(timeSeriesData.DateTimes[i], timeSeriesData.Values[i]);
                    }
                }

                startIndex = endIndex + 1;
                if (chunk.Count > 0)
                {
                    chunks.Add(chunk);
                }
            }

            return chunks;
        }

        private static void IgnoreNulls(this BitArray bitArray, ITimeSeriesData<double> timeSeriesData)
        {
            for (var i = 0; i < timeSeriesData.DateTimes.Count; i++)
            {
                if (!timeSeriesData.Values[i].HasValue)
                {
                    bitArray[i] = false;
                }
            }
        }

        private static TimeSeriesData<double> ToReducedTimeSeries(this ITimeSeriesData<double> timeSeriesData, bool[] reducedIndexes)
        {
            var reduced = new TimeSeriesData<double>();
            for (var i = 0; i < timeSeriesData.DateTimes.Count; i++)
            {
                if (reducedIndexes[i])
                {
                    reduced.Append(timeSeriesData.DateTimes[i], timeSeriesData.Values[i]);
                }
            }

            return reduced;
        }
    }

    internal struct BaseLine
    {
        public double Slope { get; set; }
        public double StartValue { get; set; }
        public DateTime StartTime { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
}