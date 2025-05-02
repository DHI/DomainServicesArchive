namespace DHI.Services.TimeSeries.Daylight
{
    using System;
    using System.Globalization;
    using System.Linq;

    public class TimeSeriesId
    {
        public TimeSeriesId(string id)
        {
            var arguments = id.Split(';').ToDictionary(r => r.Split('=')[0], r => r.Split('=')[1]);
            if (!(arguments.ContainsKey("Longitude") && arguments.ContainsKey("Latitude")))
            {
                throw new Exception("Longitude and Latitude must be entered");
            }

            foreach (var argument in arguments)
            {
                switch (argument.Key)
                {
                    case "Latitude":
                        if (!double.TryParse(argument.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var latitude))
                        {
                            throw new Exception($"Cannot parse latitude {argument.Value}, please make sure it is in decimal form.");
                        }

                        Latitude = latitude;
                        break;
                    case "Longitude":
                        if (!double.TryParse(argument.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var longitude))
                        {
                            throw new Exception($"Cannot parse longitude {argument.Value}, please make sure it is in decimal form.");
                        }

                        Longitude = longitude;
                        break;
                    case "NightValue":
                        if (!double.TryParse(argument.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var nightValue))
                        {
                            throw new Exception($"Cannot parse night value {argument.Value}.");
                        }

                        NightValue = nightValue;
                        break;
                    case "DayValue":
                        if (!double.TryParse(argument.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var dayValue))
                        {
                            throw new Exception($"Cannot parse day value {argument.Value}.");
                        }

                        DayValue = dayValue;

                        break;
                    case "SunZenith":
                        if (!double.TryParse(argument.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var sunZenith))
                        {
                            throw new Exception($"Cannot parse sun zenith value {argument.Value}.");
                        }

                        SunZenith = sunZenith;

                        break;
                    case "TimeZoneFrom":
                        try
                        {
                            TimeZoneFrom = TimeZoneInfo.FindSystemTimeZoneById(argument.Value);
                        }
                        catch
                        {
                            throw new Exception($"Cannot look up time zone {argument.Value}. Options are {string.Join(", ", TimeZoneInfo.GetSystemTimeZones().Select(r => "'" + r.Id + "'"))}");
                        }

                        break;
                    case "TimeZoneTo":
                        try
                        {
                            TimeZoneTo = TimeZoneInfo.FindSystemTimeZoneById(argument.Value);
                        }
                        catch
                        {
                            throw new Exception($"Cannot look up time zone {argument.Value}. Options are {string.Join(", ", TimeZoneInfo.GetSystemTimeZones().Select(r => "'" + r.Id + "'"))}");
                        }

                        break;
                    default:
                        throw new Exception($"Unknown part of id {argument.Key}.");
                }
            }
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double NightValue { get; set; }
        public double DayValue { get; set; } = 1;
        public TimeZoneInfo TimeZoneFrom { get; set; } = TimeZoneInfo.Utc;
        public TimeZoneInfo TimeZoneTo { get; set; } = TimeZoneInfo.Utc;
        public double SunZenith { get; set; } = 90.833;
    }
}