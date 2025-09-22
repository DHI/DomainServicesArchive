namespace IntegrationTestHost.Tests
{
    using System.Text;
    using System.Text.Json;

    public static class GISContentHelper
    {
        public static StringContent GetStringContent<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj, DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public static StringContent GetStringContent(string json) =>
            new StringContent(json, Encoding.UTF8, "application/json");
    }
}
