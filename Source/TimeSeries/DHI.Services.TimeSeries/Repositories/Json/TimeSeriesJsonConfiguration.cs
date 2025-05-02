namespace DHI.Services.TimeSeries.Json
{
    public class TimeSeriesJsonConfiguration
    {
        public string RootFilePath { get; set; }

        public string DateTimeQuery { get; set; }

        public string ValueQuery { get; set; }

        public string DateTimeFormat { get; set; }

        public bool? DateTimeAsUnixTime { get; set; }

        public string TimezoneFrom { get; set; }

        public string TimezoneTo { get; set; }
    }
}