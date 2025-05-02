namespace DHI.Services.Jobs.Web
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
            if (values.TryGetValue(parameterName, out var value) && value != null)
            {
                DateTime datetime;
                var standardFormat = DateTime.TryParseExact(values[parameterName].ToString(), Properties.Settings.Default.UriDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out datetime);
                return standardFormat || DateTime.TryParseExact(values[parameterName].ToString(), Properties.Settings.Default.UriDateTimeFormat + ".fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out datetime);
            }

            return false;
        }
    }
}