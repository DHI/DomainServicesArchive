namespace IntegrationTestHost.Tests
{
    using System.Text;
    using System.Text.Json;

    public static class Helpers
    {
        public static StringContent ToJsonContent(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
    }
}
