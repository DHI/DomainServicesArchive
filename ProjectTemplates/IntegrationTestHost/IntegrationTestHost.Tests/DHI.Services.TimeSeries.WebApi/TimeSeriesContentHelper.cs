namespace IntegrationTestHost.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    public static class TimeSeriesContentHelper
    {
        public static StringContent GetStringContent(object obj)
        {
            return new StringContent(JsonSerializer.Serialize(obj, DHI.Services.TimeSeries.WebApi.SerializerOptionsDefault.Options), Encoding.UTF8, "application/json");
        }
    }
}
