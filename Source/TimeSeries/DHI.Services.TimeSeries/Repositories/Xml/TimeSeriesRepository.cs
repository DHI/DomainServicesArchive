namespace DHI.Services.TimeSeries.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Xml.Linq;
    using System.Xml.XPath;

    /// <inheritdoc />
    public class TimeSeriesRepository : BaseTimeSeriesRepository<string, double>
    {
        private readonly string _xmlFilePath;

        /// <inheritdoc />
        public TimeSeriesRepository(string xmlFilePath)
        {
            _xmlFilePath = xmlFilePath ?? throw new ArgumentNullException(nameof(xmlFilePath));

            if (!File.Exists(xmlFilePath))
            {
                throw new ArgumentException($"Folder '{xmlFilePath}' does not exist");
            }
        }

        public override Maybe<TimeSeries<string, double>> Get(string id, ClaimsPrincipal user = null)
        {
            return new TimeSeries<string, double>(id, id).ToMaybe();
        }

        /// <inheritdoc />
        public override Maybe<ITimeSeriesData<double>> GetValues(string id, ClaimsPrincipal user = null)
        {
            return ReadTimeSeries(id).ToMaybe();
        }

        private ITimeSeriesData<double> ReadTimeSeries(string id)
        {
            var timeSeries = new TimeSeriesXml(id, _xmlFilePath);

            var document = XDocument.Load(timeSeries.FilePath);

            TimeZoneInfo timezoneFrom = null;
            TimeZoneInfo timezoneTo = null;
            if (!string.IsNullOrEmpty(timeSeries.Configuration.TimezoneFrom) && !string.IsNullOrEmpty(timeSeries.Configuration.TimezoneTo))
            {
                try
                {
                    timezoneFrom = TimeZoneInfo.FindSystemTimeZoneById(timeSeries.Configuration.TimezoneFrom);
                }
                catch
                {
                    throw new Exception($"Cannot look up time zone {timeSeries.Configuration.TimezoneFrom}. Options are {string.Join(", ", TimeZoneInfo.GetSystemTimeZones().Select(r => "'" + r.Id + "'"))}");
                }

                try
                {
                    timezoneTo = TimeZoneInfo.FindSystemTimeZoneById(timeSeries.Configuration.TimezoneTo);
                }
                catch
                {
                    throw new Exception($"Cannot look up time zone {timeSeries.Configuration.TimezoneTo}. Options are {string.Join(", ", TimeZoneInfo.GetSystemTimeZones().Select(r => "'" + r.Id + "'"))}");
                }
            }

            List<DateTime?> dateTimes;
            if (!string.IsNullOrEmpty(timeSeries.Configuration.DateTimeAttribute))
            {
                var elements = document.XPathSelectElements(timeSeries.DoReplacements(timeSeries.Configuration.DateTimeQuery)).ToList();
                dateTimes = elements.Select(r => r.Attributes().ToList().Exists(s => s.Name == timeSeries.Configuration.DateTimeAttribute) ? DateTime.ParseExact(r.Attribute(timeSeries.Configuration.DateTimeAttribute).Value, timeSeries.Configuration.DateTimeFormat, CultureInfo.InvariantCulture) : (DateTime?)null).ToList();
            }
            else
            {
                dateTimes = document.XPathSelectElements(timeSeries.DoReplacements(timeSeries.Configuration.DateTimeQuery)).Select(r => (DateTime?)DateTime.ParseExact(r.Value, timeSeries.Configuration.DateTimeFormat, CultureInfo.InvariantCulture)).ToList();
            }

            List<double?> values;
            if (!string.IsNullOrEmpty(timeSeries.Configuration.ValueAttribute))
            {
                var elements = document.XPathSelectElements(timeSeries.DoReplacements(timeSeries.Configuration.ValueQuery)).ToList();
                values = elements.Select(r => r.Attributes().ToList().Exists(s => s.Name == timeSeries.Configuration.ValueAttribute) ? double.Parse(r.Attribute(timeSeries.Configuration.ValueAttribute).Value, CultureInfo.InvariantCulture) : (double?)null).ToList();
            }
            else
            {
                values  = document.XPathSelectElements(timeSeries.DoReplacements(timeSeries.Configuration.ValueQuery)).Select(r => (double?)double.Parse(r.Value, CultureInfo.InvariantCulture)).ToList();
            }

            if (dateTimes.Count != values.Count)
            {
                if (timeSeries.Configuration.SkipIfCannotParse.HasValue && timeSeries.Configuration.SkipIfCannotParse.Value)
                {
                    return new TimeSeriesData<double>();
                }
                throw new Exception("The number of date times does nto equal the number of values");
            }

            var valueList = new List<KeyValuePair<DateTime, double?>>();
            for (var i = 0; i < values.Count; i++)
            {
                if (!values[i].HasValue || !dateTimes[i].HasValue)
                {
                    continue;
                }

                var dateTime = dateTimes[i].Value;
                if (timezoneFrom != null && timezoneTo != null)
                {
                    dateTime = TimeZoneInfo.ConvertTime(dateTime, timezoneFrom, timezoneTo);
                }

                // Remove any if existing
                valueList.RemoveAll(r => r.Key == dateTime);

                valueList.Add(new KeyValuePair<DateTime, double?>(dateTime, values[i].Value));
            }

            valueList = valueList.OrderBy(r => r.Key).ToList();
            return new TimeSeriesData<double>(valueList.Select(dateTime => dateTime.Key).ToList(), valueList.Select(value => value.Value).ToList());
        }
    }
}