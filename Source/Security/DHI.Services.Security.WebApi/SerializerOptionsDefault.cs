namespace DHI.Services.Security.WebApi
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Security.WebApi.Converters;
    using DHI.Services;

    public sealed class SerializerOptionsDefault
    {
        #region ' Thread-Safe Singleton Constructor '
        private static readonly Lazy<SerializerOptionsDefault> _lazy = new(() => new SerializerOptionsDefault());

        private static SerializerOptionsDefault instance => _lazy.Value;

        private SerializerOptionsDefault()
        {
            _serializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNameCaseInsensitive = true,
            };

            _serializerOptions.AddConverters(_defaultJsonConverters());
        }
        #endregion 

        private readonly JsonSerializerOptions _serializerOptions;

        private readonly static Func<JsonConverter[]> _defaultJsonConverters = () =>
        {
            return new JsonConverter[]
            {
                new ByteArrayConverter(),
                new MailTemplateConverter(),
                new UserGroupConverter(),
                new DHI.Services.Converters.DictionaryTypeResolverConverter<string, Accounts.Account>(),
                new DHI.Services.Converters.DictionaryTypeResolverConverter<string, Mails.MailTemplate>(),
                new DHI.Services.Converters.ObjectToInferredTypeConverter(),
            };
        };

        public static JsonSerializerOptions Options => instance._serializerOptions;
    }
}
