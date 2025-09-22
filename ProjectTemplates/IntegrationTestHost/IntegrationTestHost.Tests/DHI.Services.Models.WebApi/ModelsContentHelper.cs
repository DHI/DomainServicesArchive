namespace IntegrationTestHost.Tests
{
    using System.Text;
    using System.Text.Json;

    public static class ModelsContentHelper
    {
        public static StringContent GetStringContent(object obj)
        {
            return new StringContent(
            JsonSerializer.Serialize(obj, DHI.Services.Models.WebApi.SerializerOptionsDefault.Options),
            Encoding.UTF8,
            "application/json");
        }
    }
}
