using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DHI.Services.TimeSeries.Test")]

namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ThresholdValues : SortedSet<double>
    {
        private readonly List<Interval<double>> _intervals;

        public ThresholdValues()
        {
            _intervals = new List<Interval<double>>();
        }

        public ThresholdValues(double first, double last, int numberOfValues = 10) : this()
        {
            if (first.CompareTo(last) >= 0)
            {
                throw new ArgumentException($"The last value '{last}' must be larger than the first value '{first}.");
            }

            var intervalLength = (last - first) / (numberOfValues - 1);
            for (var i = 0; i < numberOfValues; i++)
            {
                Add(first + i * intervalLength);
            }
        }

        public IEnumerable<Interval<double>> Intervals => _intervals.ToArray();

        public new bool Add(double value)
        {
            if (base.Add(value))
            {
                UpdateIntervals();
                return true;
            }

            return false;
        }

        public Interval<double> GetInterval(double? value)
        {
            if (value == null)
            {
                return null;
            }

            var currentValue = value;
            Func<int, int, Interval<double>> bisection = null;
            bisection = (left, right) =>
            {
                if (right - left == 1) // we are almost there...
                {
                    if (currentValue.Value.CompareTo(_intervals[left].End) < 0 && currentValue.Value.CompareTo(_intervals[left].Start) >= 0)
                    {
                        return _intervals[left];
                    }

                    return _intervals[right];
                }

                var mid = (left + right) / 2;
                if (currentValue.Value.CompareTo(_intervals[mid].End) < 0)
                {
                    return bisection(left, mid);
                }

                return bisection(mid, right);
            };

            return bisection(0, _intervals.Count);
        }

        private void UpdateIntervals()
        {
            _intervals.Clear();
            var valueList = this.ToList(); // enable indexing
            if (Count > 0)
            {
                var interval = new Interval<double>(double.MinValue, valueList.Min());
                _intervals.Add(interval);
                for (var i = 0; i < Count - 1; i++)
                {
                    interval = new Interval<double>(valueList[i], valueList[i + 1]);
                    _intervals.Add(interval);
                }

                interval = new Interval<double>(valueList.Max(), double.MaxValue);
                _intervals.Add(interval);
            }
        }
    }
}