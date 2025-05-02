namespace DHI.Services.Jobs.Web
{
    using System;
    using System.Globalization;
    using System.Web;
    using Logging;

    internal static class ExtensionMethods
    {
        public static string Resolve(this string connectionString)
        {
            return HttpContext.Current != null ? connectionString.Replace("[AppData]", HttpContext.Current.Server.MapPath(@"~\App_Data") + @"\") : connectionString;
        }

        public static object ToObject(this string stringValue)
        {
            if (int.TryParse(stringValue, NumberStyles.None, CultureInfo.InvariantCulture, out var intValue))
            {
                return intValue;
            }

            if (double.TryParse(stringValue, NumberStyles.None, CultureInfo.InvariantCulture, out var doubleValue))
            {
                return doubleValue;
            }

            if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeValue))
            {
                return dateTimeValue;
            }

            if (bool.TryParse(stringValue, out var boolValue))
            {
                return boolValue;
            }

            if (Enum.TryParse(stringValue, out LogLevel logLevel))
            {
                return logLevel;
            }

            if (Enum.TryParse(stringValue, out JobStatus jobStatus))
            {
                return jobStatus;
            }

            if (Guid.TryParse(stringValue, out Guid id))
            {
                return id;
            }

            return stringValue;
        }
    }
}