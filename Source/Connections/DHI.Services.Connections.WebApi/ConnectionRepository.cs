namespace DHI.Services.Connections.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     JSON file-based ConnectionRepository that is located in the App_Data folder.
    /// </summary>
    /// <seealso cref="DHI.Services.ConnectionRepository" />
    public class ConnectionRepository : DHI.Services.ConnectionRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionRepository" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param> 
        /// <param name="comparer">Equality comparer for entity</param> 
        public ConnectionRepository(string fileName,
            IEqualityComparer<string> comparer = null)
            : base(Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), fileName), comparer)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConnectionRepository" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param> 
        /// <param name="serializerOptions">Json serializer options</param>
        /// <param name="deserializerOptions">Json serializer options specific for deserialization only. Null will took <paramref name="serializerOptions"/> for deserialized</param>
        /// <param name="comparer">Equality comparer for entity</param> 
        public ConnectionRepository(string fileName,
            JsonSerializerOptions serializerOptions,
            JsonSerializerOptions deserializerOptions = null,
            IEqualityComparer<string> comparer = null)
            : base(Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), fileName),
                  serializerOptions,
                  deserializerOptions,
                  comparer)
        {
        }
    }
}