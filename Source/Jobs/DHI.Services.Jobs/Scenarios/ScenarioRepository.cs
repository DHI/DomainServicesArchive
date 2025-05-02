namespace DHI.Services.Jobs.Scenarios
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;

    /// <summary>
    ///     Scenario Repository
    /// </summary>
    public class ScenarioRepository : JsonRepository<Scenario, string>, IScenarioRepository
    {
        private static readonly Func<JsonConverter[]> _requiredConverters = () =>
        {
            return new JsonConverter[]
            {
                new DictionaryTypeResolverConverter<string, Scenario>(),
            };
        };

        public ScenarioRepository(string filePath, IEnumerable<JsonConverter> converters = null)
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

        public IEnumerable<Scenario> Get(DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            return Get(s => s.DateTime > from && s.DateTime < to);
        }

        public IEnumerable<Scenario> Get(Query<Scenario> query, ClaimsPrincipal user = null)
        {
            return Get(query.ToExpression());
        }
    }
}