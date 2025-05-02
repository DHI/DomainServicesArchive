namespace DHI.Services.TimeSeries.Json
{


    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using Argon;

    /// <inheritdoc />
    public class TimeSeriesRepository : BaseTimeSeriesRepository<string, double>
    {
        private readonly string _jsonFilePath;

        /// <inheritdoc />
        public TimeSeriesRepository(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath ?? throw new ArgumentNullException(nameof(jsonFilePath));

            if (!File.Exists(jsonFilePath))
            {
                throw new ArgumentException($"Folder '{jsonFilePath}' does not exist");
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
            var timeSeries = new TimeSeriesJson(id, _jsonFilePath);
            var text = File.ReadAllText(timeSeries.FilePath, Encoding.Default);
            var jsonReader = new JsonTextReader(new StringReader(text));
            //var jsonReader = new JsonTextReader(new StringReader(text)) { DateParseHandling = DateParseHandling.DateTimeOffset };
            //if (!string.IsNullOrEmpty(timeSeries.Configuration.DateTimeFormat))
            //{
            //    jsonReader.DateFormatString = timeSeries.Configuration.DateTimeFormat;
            //}

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

            JToken o;
            if (text.StartsWith("{"))
            {
                o = JObject.Load(jsonReader);
            }
            else
            {
                o = JArray.Load(jsonReader);
            }

            IEnumerable<JToken> dateTimes = new List<JToken>();
            if (!string.IsNullOrEmpty(timeSeries.Configuration.DateTimeQuery))
            {
                dateTimes = o.SelectTokens(timeSeries.DoReplacements(timeSeries.Configuration.DateTimeQuery));
            }

            var values = o.SelectTokens(timeSeries.DoReplacements(timeSeries.Configuration.ValueQuery));

            var dateTimeArr = dateTimes.ToArray();
            var valuesArr = values.ToArray();

            var valueList = new List<KeyValuePair<DateTime, double?>>();
            for (var i = 0; i < values.Count(); i++)
            {
                var dateTime = DateTime.UtcNow;
                if (dateTimes.Any())
                {
                    if (timeSeries.Configuration.DateTimeAsUnixTime.HasValue && timeSeries.Configuration.DateTimeAsUnixTime.Value)
                    {
                        try
                        {
                            dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds((long)dateTimeArr[i]).ToUniversalTime();
                        }
                        catch
                        {
                            throw new Exception($"Cannot parse {dateTimeArr[i]} to an integer as unix time");
                        }
                    }
                    else
                    {
                        try
                        {
                            //var  datestr = JsonConvert.SerializeObject(dateTimes.ToList()[i].ToString(), Formatting.Indented, new JsonSerializerSettings());
                            // dateTime = DateTime.ParseExact(datestr, timeSeries.Configuration.DateTimeFormat, CultureInfo.InvariantCulture);
                            if (String.IsNullOrEmpty(timeSeries.Configuration.DateTimeFormat)) {
                                dateTime = DateTimeOffset.Parse(dateTimeArr[i].ToString()).DateTime;

                            }
                            else {
                                //var dateStr = DateTime.ParseExact(dateTimes.ToList()[i].ToString(), timeSeries.Configuration.DateTimeFormat, CultureInfo.InvariantCulture);
                                //dateTime = DateTimeOffset.Parse(dateTimes.ToList()[i].ToString(),).DateTime;
                               // dateTime = DateTimeOffset.ParseExact(dateTimes.ToList()[i].ToString(), timeSeries.Configuration.DateTimeFormat).DateTime;

                                if (DateTimeOffset.TryParseExact(dateTimeArr[i].ToString(), timeSeries.Configuration.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset dateTimeOffset))
                                {
                                    dateTime = dateTimeOffset.DateTime;
                                }
                            }
                            //dateTime =  (DateTime)dateTimes.ToList()[i];
                        }
                        catch
                        {
                            throw new Exception($"Cannot parse {dateTimeArr[i]} to a date time");
                        }
                    }
                }

                if (timezoneFrom != null && timezoneTo != null)
                {
                    dateTime = TimeZoneInfo.ConvertTime(dateTime, timezoneFrom, timezoneTo);
                }

                if (values.ToList()[i].Type != JTokenType.Null)
                {
                    var value = (double)valuesArr[i];

                    // Remove any if existing
                    valueList.RemoveAll(r => r.Key == dateTime);

                    valueList.Add(new KeyValuePair<DateTime, double?>(dateTime, value));
                }
            }

            valueList = valueList.OrderBy(r => r.Key).ToList();
            return new TimeSeriesData<double>(valueList.Select(dateTime => dateTime.Key).ToList(), valueList.Select(value => value.Value).ToList());
        }
    }
}