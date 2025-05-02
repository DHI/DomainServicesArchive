namespace DHI.Services.Jobs.Web
{
    using System;
    using System.Globalization;

    internal static class Datetime
    {
        public static DateTime Parse(string s)
        {
            var standardFormat = DateTime.TryParseExact(s, Properties.Settings.Default.UriDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var datetime);
            return standardFormat ? datetime : DateTime.ParseExact(s, Properties.Settings.Default.UriDateTimeFormat + ".fff", CultureInfo.InvariantCulture);
        }
    }
}