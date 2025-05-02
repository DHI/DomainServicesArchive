namespace DHI.Services.TimeSeries
{
    using System;

    internal class Interval<TValue> where TValue : struct, IComparable<TValue>
    {
        public Interval(TValue start, TValue end)
        {
            if (start.CompareTo(end) > 0)
            {
                throw new ArgumentException($"The start value '{start}' must be less than the end value '{end}.");
            }

            Start = start;
            End = end;
        }

        public TValue Start { get; }

        public TValue End { get; }

        public bool Contains(TValue value)
        {
            return value.CompareTo(Start) >= 0 && End.CompareTo(value) > 0;
        }

        public override string ToString()
        {
            return $@"{Start} - {End}";
        }
    }
}