using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("DHI.Services.Places.Test")]

namespace DHI.Services.Places
{
    using System;
    using System.Globalization;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     A declarative way to define a time period.
    /// </summary>
    [Serializable]
    public class TimeInterval
    {
        [JsonConstructor]
        public TimeInterval(TimeIntervalType type = TimeIntervalType.All)
            : this(type, null, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeInterval" /> class.
        /// </summary>
        /// <param name="type">The interval type.</param>
        /// <param name="start">The interval start.</param> 
        /// <param name="end">The interval end.</param>
        internal TimeInterval(TimeIntervalType type, double? start, double? end)
        {
            Type = type;
            Start = start;
            End = end;
            if (end < start)
            {
                throw new ArgumentException($"Time interval end '{end}' must be larger than or equal to time interval start '{start}'.");
            }

            if (Type == TimeIntervalType.All)
            {
                return;
            }

            Guard.Against.Null(start, nameof(start));
            Guard.Against.Null(end, nameof(end));
        }

        /// <summary>
        ///     Gets or sets the interval type.
        /// </summary>
        public TimeIntervalType Type { get; }

        /// <summary>
        ///     Gets or sets the interval start.
        /// </summary>
        /// <remarks>
        ///     If the time interval type is "Fixed" the Start value represents a so-called "OLE automation date".
        ///     If the time interval type is "RelativeToNow" or "RelativeToDateTime" the Start value represents a number of days.
        ///     For example -7 means 7 days before.
        ///     If the time interval type is "All" the Start value is not used.
        /// </remarks>
        public double? Start { get; } = null;

        /// <summary>
        ///     Gets or sets the interval end.
        /// </summary>
        /// <remarks>
        ///     If the time interval type is "Fixed" the End value represents a so-called "OLE automation date".
        ///     If the time interval type is "RelativeToNow" or "RelativeToDateTime" the End value represents a number of days.
        ///     For example -7 means 7 days before.
        ///     If the time interval type is "All" the End value is not used.
        /// </remarks>
        public double? End { get; } = null;

        /// <summary>
        ///     Creates a fixed time interval.
        /// </summary>
        /// <param name="start">The interval start as an OLE automation date.</param>
        /// <param name="end">The interval end as an OLE automation date.</param>
        public static TimeInterval CreateFixed(double start, double end)
        {
            return new(TimeIntervalType.Fixed, start, end);
        }

        /// <summary>
        ///     Creates a time interval specified relatively to a given date time.
        /// </summary>
        /// <param name="start">The interval start as a timespan in days relative to the given date/time.</param>
        /// <param name="end">The interval end as a timespan in days relative to the given date/time.</param>
        public static TimeInterval CreateRelativeToDateTime(double start, double end)
        {
            return new(TimeIntervalType.RelativeToDateTime, start, end);
        }

        /// <summary>
        ///     Creates a time interval specified relative to the current date time.
        /// </summary>
        /// <param name="start">The interval start as a timespan in days relative to the given date/time.</param>
        /// <param name="end">The interval end as a timespan in days relative to the given date/time.</param>
        public static TimeInterval CreateRelativeToNow(double start, double end)
        {
            return new TimeInterval(TimeIntervalType.RelativeToNow, start, end);
        }

        /// <summary>
        ///     Creates a time interval representing all time steps in a time series.
        /// </summary>
        public static TimeInterval CreateAll()
        {
            return new(TimeIntervalType.All, null, null);
        }

        /// <summary>
        ///     Converts the declarative time interval to an actual time period.
        /// </summary>
        /// <param name="offsetDateTime">The offset date/time. Necessary if the interval type is RelativeToDateTime.</param>
        public (DateTime from, DateTime to) ToPeriod(DateTime? offsetDateTime = null)
        {
            if (Start is null || End is null)
            {
                throw new Exception("Both Start and End values must be defined to retrieve the period.");
            }

            DateTime from;
            DateTime to;
            switch (Type)
            {
                case TimeIntervalType.Fixed:
                    from = DateTime.FromOADate((double)Start);
                    to = DateTime.FromOADate((double)End);
                    break;
                case TimeIntervalType.RelativeToNow:
                    from = DateTime.Now.AddDays((double)Start);
                    to = DateTime.Now.AddDays((double)End);
                    break;
                case TimeIntervalType.RelativeToDateTime:
                    if (offsetDateTime is null)
                    {
                        throw new ArgumentException("The datetime argument must be defined for time intervals relative to a specified datetime.", nameof(offsetDateTime));
                    }

                    from = ((DateTime)offsetDateTime).AddDays((double)Start);
                    to = ((DateTime)offsetDateTime).AddDays((double)End);
                    break;
                default:
                    throw new NotSupportedException($"ToPeriod method is not supported for TimeIntervalType '{Type}'");
            }

            return (from, to);
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            switch (Type)
            {
                case TimeIntervalType.Fixed:
                    return $"{DateTime.FromOADate((double)Start!).ToString(CultureInfo.InvariantCulture)} - {DateTime.FromOADate((double)End!).ToString(CultureInfo.InvariantCulture)}";
                case TimeIntervalType.RelativeToNow:
                    return $"Relative to now. From day {Start} to day {End}";
                case TimeIntervalType.RelativeToDateTime:
                    return $"Relative to latest. From day {Start} to day {End}";
                case TimeIntervalType.All:
                    return "All";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}