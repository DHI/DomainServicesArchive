namespace DHI.Services.Security.WebApi.Converters
{
    using System.Text.Json;

    internal static class JsonSerializerOptionsExtensions
    {
        public static string ToNamingPolicy(this JsonSerializerOptions options, string name)
            => options.PropertyNamingPolicy?.ConvertName(name) ?? name;
    }
}
