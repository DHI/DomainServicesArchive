namespace DHI.Services.Authorization
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class UserGroupRepository : JsonRepository<UserGroup, string>, IUserGroupRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UserGroupRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public UserGroupRepository(string filePath) : base(filePath)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserGroupRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters"><seealso cref="JsonConverter"/> collection</param> 
        /// <param name="comparer">Equality comparer for entity</param>
        public UserGroupRepository(string filePath,
            IEnumerable<JsonConverter> converters,
            IEqualityComparer<string> comparer = null)
            : base(filePath, converters, comparer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserGroupRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="serializerOptions">Json serializer options</param>
        /// <param name="deserializerOptions">Json serializer options specific for deserialization only. Null will took <paramref name="serializerOptions"/> for deserialized</param> 
        /// <param name="comparer">Equality comparer for entity</param>
        public UserGroupRepository(string filePath,
            System.Text.Json.JsonSerializerOptions serializerOptions,
            System.Text.Json.JsonSerializerOptions deserializerOptions = null,
            IEqualityComparer<string> comparer = null)
            : base(filePath, serializerOptions, deserializerOptions, comparer)
        {
        }
    }
}