namespace DHI.Services.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     The Filter class is used for persisting queries representing filters for real-time (SignalR) messages.
    ///     The ID of a filter is a base64-encoding of a JSON serialization of the filter.
    ///     This is to ensure that a Filter repository contains a distinct set of filters (no duplicates).
    /// </summary>
    public sealed class Filter : BaseEntity<string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Filter" /> class.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        /// <param name="dataConnectionId">The data connection identifier.</param>
        /// <param name="queryConditions">The queryConditions.</param>
        [JsonConstructor]
        public Filter(string dataType, string dataConnectionId, IEnumerable<QueryCondition> queryConditions, HashSet<string> transportConnections = null)
        {
            Guard.Against.NullOrEmpty(dataType, nameof(dataType));
            var conditions = queryConditions as QueryCondition[] ?? queryConditions.ToArray();
            DataType = dataType;
            DataConnectionId = dataConnectionId;
            QueryConditions = conditions;
            Id = SerializeAndEncodeId();
            TransportConnections = transportConnections ?? new HashSet<string>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Filter"/> class.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        /// <param name="queryConditions">The query conditions.</param>
        public Filter(string dataType, IEnumerable<QueryCondition> queryConditions) : this(dataType, null, queryConditions)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Filter"/> class.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        /// <param name="dataConnectionId">The data connection identifier.</param>
        public Filter(string dataType, string dataConnectionId) : this(dataType, dataConnectionId, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Filter"/> class.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        public Filter(string dataType) : this(dataType, null, null)
        {
        }

        /// <summary>
        ///     Gets the query conditions.
        /// </summary>
        public IEnumerable<QueryCondition> QueryConditions { get; }

        /// <summary>
        ///     Gets the real-time transport connections.
        /// </summary>
        public HashSet<string> TransportConnections { get; }

        /// <summary>
        ///     Gets the type of the data.
        /// </summary>
        public string DataType { get; }

        /// <summary>
        ///     Gets the identifier of the data connection.
        /// </summary>
        public string DataConnectionId { get; }

        /// <summary>
        ///     Gets the json serializer settings.
        /// </summary>
        [JsonIgnore]
        public static JsonSerializerOptions SerializerOption
        {
            get
            {
                var _serializerOptions = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };
                _serializerOptions.Converters.Add(new JsonStringEnumConverter());
                return _serializerOptions;
            }
        }


        /// <summary>
        ///     Gets the json deserializer settings.
        /// </summary>
        [JsonIgnore]
        public static JsonSerializerOptions DeserializerOption
        {
            get
            {
                var _options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };
                _options.Converters.Add(new JsonStringEnumConverter());
                _options.Converters.Add(new Converters.ObjectToInferredTypeConverter());
                return _options;
            }
        }


        private string SerializeAndEncodeId()
        {
            object o;
            if (DataConnectionId is null && QueryConditions is null)
            {
                o = new { dataType = DataType };
            }
            else if (DataConnectionId is null)
            {
                o = new { dataType = DataType, queryConditions = QueryConditions };
            }
            else if (QueryConditions is null)
            {
                o = new { dataType = DataType, dataConnectionId = DataConnectionId };
            }
            else
            {
                o = new { dataType = DataType, dataConnectionId = DataConnectionId, queryConditions = QueryConditions };
            }

            var json = JsonSerializer.Serialize(o, SerializerOption);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }
    }
}