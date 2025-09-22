namespace IntegrationTestHost.Tests
{
    using System.Text;
    using System.Text.Json;

    public static class NotificationsContentHelper
    {
        public static StringContent GetStringContent(object obj)
        {
            return new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
        }
    }
}
