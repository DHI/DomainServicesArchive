namespace IntegrationTestHost.Tests
{
    using System.Text;
    using System.Text.Json;

    public static class JobsContentHelper
    {
        public static StringContent GetStringContent(object obj)
        {
            return new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
        }
    }
}
