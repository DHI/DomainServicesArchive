namespace DHI.Services.Documents
{
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Class representing document metadata
    /// </summary>
    public class Document<TId> : BaseGroupedEntity<TId>
    {
        /// <inheritdoc />
        [JsonConstructor]
        public Document(TId id, string name, string group) : base(id, name, group)
        {
        }

        /// <inheritdoc />
        public Document(TId id, string name) : base(id, name, null)
        {
        }
    }

    /// <summary>
    ///     Class representing document metadata
    /// </summary>
    public class Document : Document<string>
    {
        /// <inheritdoc />
        public Document(string id, string name, string group) : base(id, name, group)
        {
        }

        /// <inheritdoc />
        public Document(string id, string name) : base(id, name)
        {
        }
    }
}