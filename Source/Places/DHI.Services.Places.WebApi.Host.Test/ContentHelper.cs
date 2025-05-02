namespace DHI.Services.Places.WebApi.Host.Test
{
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public static class ContentHelper
    {
        public static StringContent GetStringContent(object obj)
        {
        var json = JsonSerializer.Serialize(obj, SerializerOptionsDefault.Options);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }
    }
}