namespace DHI.Services.TimeSteps.WebApi.Host.Test
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
    }
}