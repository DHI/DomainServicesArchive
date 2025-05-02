namespace DHI.Services.Rasters.Radar
{
    using System;
    using System.Collections.Generic;

    public interface IForecaster
    {
        IRadarImage Get(SortedDictionary<DateTime, IRadarImage> images, TimeSpan forecastPeriod);
    }
}