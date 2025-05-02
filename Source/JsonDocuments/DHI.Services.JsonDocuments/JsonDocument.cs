namespace DHI.Services.JsonDocuments
{
    using DHI.Services.Authorization;
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization;


    /// <summary>
    ///     Class JsonDocument.
    /// </summary>
    /// <typeparam name="TId">The type of the json document identifier.</typeparam>
    [Serializable]
    public class JsonDocument<TId> : BaseGroupedEntity<TId>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonDocument{TId}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="group">The group.</param>
        /// <param name="data">The json data.</param>
        /// <param name="metadata"></param>
        /// <param name="permissions"></param>
        [JsonConstructor]
        public JsonDocument(TId id, string name, string group, string data, IDictionary<string, object> metadata = null ,IList<Permission> permissions = null) 
            : base(id, name, group, metadata, permissions)
        {
            Guard.Against.NullOrWhiteSpace(data, nameof(data));
            var (isValid, exception) = IsValidJson(data);
            if (!isValid)
            {
                throw new ArgumentException("The given data is not valid JSON format.", nameof(data), exception);
            }

            Data = data;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonDocument{TId}" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="data">The json data.</param>
        public JsonDocument(TId id, string name, string data) : this(id, name, null, data)
        {
        }

        /// <summary>
        ///     Gets or sets the date time.
        /// </summary>
        public DateTime? DateTime { get; set; } = null;

        /// <summary>
        ///     Gets the data.
        /// </summary>
        public string Data { get; private set; }

        /// <summary>
        ///     Soft delete time
        /// </summary>
        public DateTime? Deleted { get; set; }

        /// <summary>
        ///     Gets a JSON document where the data is filtered by the given data selectors.
        /// </summary>
        /// <param name="dataSelectors">The data selectors.</param>
        public JsonDocument<TId> Filter(string[] dataSelectors)
        {
            Data = Argon.JsonConvert.SerializeObject(Argon.JObject.Parse(Data).Filter(dataSelectors));
            return this;
        }

        /// <summary>
        ///     Determines whether the specified string is valid json.
        /// </summary>
        /// <param name="json">The json string.</param>
        private static (bool isValid, Exception exception) IsValidJson(string json)
        {
            json = json.Trim();
            if (!(json.StartsWith("{") && json.EndsWith("}") || json.StartsWith("[") && json.EndsWith("]")))
            {
                return (false, null);
            }

            try
            {

                //JToken.Parse(json);
                JsonObject? jsonObject = JsonNode.Parse(json)?.AsObject();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        }


       

    }

    /// <inheritdoc />
    [Serializable]
    public class JsonDocument : JsonDocument<string>
    {
        /// <inheritdoc />
        [JsonConstructor]
        public JsonDocument(string id, string name, string group, string data, IDictionary<string, object> metadata = null, IList<Permission> permissions = null)
            : base(id, name, group, data, metadata, permissions)
        {
        }

        /// <inheritdoc />
        public JsonDocument(string id, string name, string data)
            : base(id, name, data)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonDocument" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="data">The data.</param>
        public JsonDocument(string name, string data)
            : this(name, name, data)
        {
        }
    }
}