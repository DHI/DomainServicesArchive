namespace IntegrationTestHost.Tests
{
    using System.Text;
    using System.Text.Json;

    public static class ConnectionsContentHelper
    {
        public static StringContent GetStringContent<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj, DHI.Services.Connections.WebApi.SerializerOptionsDefault.Options);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
