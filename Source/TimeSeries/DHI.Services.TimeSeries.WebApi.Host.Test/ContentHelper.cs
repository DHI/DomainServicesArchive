namespace DHI.Services.TimeSeries.WebApi.Host.Test
{
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;

    public static class ContentHelper
    {
        public static StringContent GetStringContent(object obj)
        {
            return new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
        }

        public static StringContent GetStringContent(object obj, JsonSerializerOptions options)
        {
            return new StringContent(JsonSerializer.Serialize(obj, options), Encoding.UTF8, "application/json");
        }
    }
}