namespace DHI.Services.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;

    /// <summary>
    ///     Host Repository.
    /// </summary>
    /// <seealso cref="JsonRepository{Host, String}" />
    /// <seealso cref="IHostRepository" />
    public class HostRepository : JsonRepository<Host, string>, IHostRepository
    {
        private static readonly Func<JsonConverter[]> _requiredConverters = () =>
        {
            return new JsonConverter[]
            {
                new DictionaryTypeResolverConverter<string, Host>(),
                new DictionaryTypeResolverConverter<Guid, Job>()
            };
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters">Custom <seealso cref="JsonConverter"/></param>
        /// <exception cref="FileNotFoundException">File Not Found</exception>
        public HostRepository(string filePath, IEnumerable<JsonConverter> converters = null)
            : base(filePath, _requiredConverters())
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File Not Found", filePath);
            }

            if (converters?.Any() == true)
            {
                ConfigureJsonSerializer((serializer, deserializer) =>
                {
                    serializer.AddConverters(converters);
                    deserializer.AddConverters(converters);
                });
            }
        }

        public void AdjustJobCapacity(int desiredJobCapacity, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public void CreateHost(ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }
    }
}