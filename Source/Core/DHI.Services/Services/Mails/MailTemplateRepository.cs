namespace DHI.Services.Mails
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     JSON Mail Template Repository.
    /// </summary>
    /// <seealso cref="JsonRepository{MailTemplate, String}" />
    /// <seealso cref="IMailTemplateRepository" />
    public class MailTemplateRepository : JsonRepository<MailTemplate, string>, IMailTemplateRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MailTemplateRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public MailTemplateRepository(string filePath)
            : base(filePath)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MailTemplateRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters"><seealso cref="JsonConverter"/> collection</param> 
        /// <param name="comparer">Equality comparer</param>
        public MailTemplateRepository(string filePath,
            IEnumerable<JsonConverter> converters,
            IEqualityComparer<string> comparer = null)
            : base(filePath, converters, comparer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="serializerOptions">Json serializer options</param>
        /// <param name="deserializerOptions">Json serializer options specific for deserialization only. Null will took <paramref name="serializerOptions"/> for deserialized</param> 
        /// <param name="comparer">Equality comparer</param>
        public MailTemplateRepository(string filePath,
            JsonSerializerOptions serializerOptions,
            JsonSerializerOptions deserializerOptions = null,
            IEqualityComparer<string> comparer = null)
            : base(filePath, serializerOptions, deserializerOptions, comparer)
        {
        }
    }
}