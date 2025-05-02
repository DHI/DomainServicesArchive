namespace DHI.Services.TimeSeries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ExtensionMethods
    {
        public static DateTime Add(this DateTime dateTime, Period period)
        {
            switch (period)
            {
                case Period.Hourly:
                    return dateTime.AddHours(1);
                case Period.Daily:
                    return dateTime.AddDays(1);
                case Period.Weekly:
                    return dateTime.AddDays(7);
                case Period.Monthly:
                    return dateTime.AddMonths(1);
                case Period.Quarterly:
                    return dateTime.AddMonths(3);
                case Period.Yearly:
                    return dateTime.AddYears(1);
                default:
                    throw new NotSupportedException($"Period '{period}' is not supported.");
            }
        }

        public static DateTime Subtract(this DateTime dateTime, Period period)
        {
            switch (period)
            {
                case Period.Hourly:
                    return dateTime.AddHours(-1);
                case Period.Daily:
                    return dateTime.AddDays(-1);
                case Period.Weekly:
                    return dateTime.AddDays(-7);
                case Period.Monthly:
                    return dateTime.AddMonths(-1);
                case Period.Quarterly:
                    return dateTime.AddMonths(-3);
                case Period.Yearly:
                    return dateTime.AddYears(-1);
                default:
                    throw new NotSupportedException($"Period '{period}' is not supported.");
            }
        }

        public static DateTime First(this IList<DateTime> dateTimes, Period period)
        {
            switch (period)
            {
                case Period.Hourly:
                    return dateTimes.First();
                case Period.Daily:
                    return dateTimes.First();
                case Period.Weekly:
                    return dateTimes.First();
                case Period.Monthly:
                    return new DateTime(dateTimes.First().Year, dateTimes.First().Month, 1);
                case Period.Quarterly:
                    return new DateTime(dateTimes.First().Year, dateTimes.First().Month, 1);
                case Period.Yearly:
                    return new DateTime(dateTimes.First().Year, dateTimes.First().Month, 1);
                default:
                    throw new NotSupportedException($"Period '{period}' is not supported.");
            }
        }

        //https://stackoverflow.com/questions/38039/how-can-i-get-the-datetime-for-the-start-of-the-week
        public static DateTime StartOfWeek(this DateTime dateTime)
        {
            //https://thetylt.com/culture/sunday-first-or-last-day-of-the-week#:~:text=According%20to%20international%20standard%20ISO,the%207th%20and%20final%20day.
            //According to link above, monday is the first day of the week
            int diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
            return dateTime.AddDays(-1 * diff).Date;
        }

        public static DateTime StartOfQuarter(this DateTime dateTime) 
        {
            return new DateTime(dateTime.Year, ((dateTime.Month - 1) / 3) * 3 + 1, 1);
        }

        public static IEnumerable<IGrouping<DateTime, DataPoint<TValue>>> GroupBy<TValue>(this ITimeSeriesData<TValue> data, Period period) where TValue : struct, IComparable<TValue>
        {
            var points = data.ToSortedSet();
            IEnumerable<IGrouping<DateTime, DataPoint<TValue>>> groups;
            switch (period)
            {
                case Period.Hourly:
                    groups = points.GroupBy(point => new DateTime(point.DateTime.Year, point.DateTime.Month, point.DateTime.Day, point.DateTime.Hour, 0, 0));
                    break;
                case Period.Daily:
                    groups = points.GroupBy(point => new DateTime(point.DateTime.Year, point.DateTime.Month, point.DateTime.Day));
                    break;
                case Period.Monthly:
                    groups = points.GroupBy(point => new DateTime(point.DateTime.Year, point.DateTime.Month, 1));
                    break;
                case Period.Yearly:
                    groups = points.GroupBy(point => new DateTime(point.DateTime.Year, 1, 1));
                    break;
                default:
                    throw new NotSupportedException($"Period '{period}' is not supported.");
            }

            return groups;
        }
    }
}