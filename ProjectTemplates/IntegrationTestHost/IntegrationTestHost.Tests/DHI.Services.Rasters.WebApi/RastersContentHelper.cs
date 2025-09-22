namespace IntegrationTestHost.Tests
{
    using System.Text;
    using System.Text.Json;

    public static class RastersContentHelper
    {
        public static StringContent GetStringContent(object obj)
        {
            return new StringContent(JsonSerializer.Serialize(obj, DHI.Services.Rasters.WebApi.SerializerOptionsDefault.Options), Encoding.UTF8, "application/json");
        }
    }
}
