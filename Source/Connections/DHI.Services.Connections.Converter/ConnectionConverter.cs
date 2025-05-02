namespace DHI.Services.Connections.Converter
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    [Obsolete("Plase use your configured 'JsonSerializerOptions' directly to create 'DHI.Services.ConnectionRepository'. This package probably will be removed on next release")]
    public class ConnectionConverter
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly JsonSerializerOptions _deserializerOptions;

        /// <summary>
        ///     Initialize a connnection converter with default <seealso cref="JsonSerializerOptions"/>.
        ///     Instantiate with preconfigured default <seealso cref="JsonConverter"/>.
        /// </summary>
        public ConnectionConverter() : this(new JsonSerializerOptions())
        {
        }

        /// <summary>
        ///     Initialize a connnection converter with <seealso cref="JsonSerializerOptions"/>.
        ///     Instantiate with preconfigured default <seealso cref="JsonConverter"/> combined from <paramref name="serializerOptions"/>
        /// </summary>
        public ConnectionConverter(JsonSerializerOptions serializerOptions)
            : this(serializerOptions, null)
        {
        }

        /// <summary>
        ///     Initialize a connnection converter with default <seealso cref="JsonSerializerOptions"/>.
        ///     Instantiate with preconfigured default <seealso cref="JsonConverter"/> combined from <paramref name="converters"/>.
        /// </summary>
        public ConnectionConverter(IEnumerable<JsonConverter> converters)
        {
            _serializerOptions = _deserializerOptions = new JsonSerializerOptions();

            _serializerOptions.AddConverters(converters);
            _deserializerOptions.AddConverters(converters);
        }

        /// <summary>
        ///     Initialize a connnection converter with default <seealso cref="JsonSerializerOptions"/>.
        ///     Instantiate with preconfigured default <seealso cref="JsonSerializerOptions"/> combined with <paramref name="serializerOptions"/> and <paramref name="deserializerOptions"/>.
        /// </summary>
        public ConnectionConverter(JsonSerializerOptions serializerOptions, JsonSerializerOptions deserializerOptions)
        {
            _serializerOptions = serializerOptions == null ? new JsonSerializerOptions() : new JsonSerializerOptions(serializerOptions);
            _deserializerOptions = deserializerOptions ?? new JsonSerializerOptions(_serializerOptions);
        }

        public JsonSerializerOptions SerializerOptions => _serializerOptions;

        public JsonSerializerOptions DeserializerOptions => _deserializerOptions;
    }
}
