namespace DHI.Services.TimeSeries.Web
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Web.Http.Routing;

    internal class DateTimeConstraint : IHttpRouteConstraint
    {
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (values.TryGetValue(parameterName, out object value) && value != null)
            {
                var standardFormat = DateTime.TryParseExact(values[parameterName].ToString(), Properties.Settings.Default.UriDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime datetime);
                return standardFormat || DateTime.TryParseExact(values[parameterName].ToString(), Properties.Settings.Default.UriDateTimeFormat + ".fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out datetime);
            }

            return false;
        }
    }
}