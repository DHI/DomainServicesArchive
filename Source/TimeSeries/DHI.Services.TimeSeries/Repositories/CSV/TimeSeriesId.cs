namespace DHI.Services.TimeSeries.CSV
{
    using System;

    internal class TimeSeriesId : BaseGroupedFileEntityId
    {
        public TimeSeriesId(string relativeFilePath, string item)
            : base(relativeFilePath, item)
        {
            Guard.Against.NullOrEmpty(item, nameof(item));
        }

        public static TimeSeriesId Parse(string s)
        {
            var tokens = s.Split(';');
            if (tokens.Length != 2)
            {
                throw new ArgumentException("Could not parse time series id string '" + s + "'. A times series id has the following format:\n\n" +
                                            "<path>;<item>\n\n" +
                                            "<path> is the relative file path - e.g. data/test.csv\n" +
                                            "<item> is the item name - e.g. WaterLevel\n\n" +
                                            "Time series id example: data/test.csv;WaterLevel", nameof(s));
            }

            var relativeFilePath = tokens[0];
            var objId = tokens[1];
            return new TimeSeriesId(relativeFilePath, objId);
        }
    }
}