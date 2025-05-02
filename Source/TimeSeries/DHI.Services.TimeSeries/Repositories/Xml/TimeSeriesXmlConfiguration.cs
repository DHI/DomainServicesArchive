namespace DHI.Services.TimeSeries.Xml
{
    public class TimeSeriesXmlConfiguration
    {
        public string RootFilePath { get; set; }

        public string DateTimeQuery { get; set; }

        public string DateTimeAttribute { get; set; }

        public string ValueQuery { get; set; }

        public string ValueAttribute { get; set; }

        public string TimezoneFrom { get; set; }

        public string TimezoneTo { get; set; }

        public string DateTimeFormat { get; set; } = "yyyy-MM-dd'T'HH:mm:ss+00:00";

        public bool? SkipIfCannotParse { get; set; } = false;
    }
}