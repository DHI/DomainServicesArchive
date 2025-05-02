namespace DHI.Services.TimeSeries.Web
{
    using System;
    using Properties;
    using WebApi.OutputCache.V2;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    internal class CacheTimeSeriesOutputAttribute : CacheOutputAttribute
    {
        public CacheTimeSeriesOutputAttribute()
        {
            ClientTimeSpan = ServerTimeSpan = Convert.ToInt32(Settings.Default.CacheTimeSeriesTimeout.TotalSeconds);
        }
    }
}