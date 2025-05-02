namespace DHI.Services.Scalars
{
    using DHI.Services.Converters;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json.Serialization;

    public class ScalarRepository : GroupedJsonRepository<Scalar<string, int>>, IGroupedScalarRepository<string, int>
    {

        private static readonly JsonConverter[] _requiredConverters = new JsonConverter[]
        {
            new ScalarConverter<string, int>(),
            new DictionaryTypeResolverConverter<string, Scalar<string, int>>(isNestedDictionary: true)
        };

        private static IEnumerable<JsonConverter> ConcatConverters(IEnumerable<JsonConverter> converters = null) 
            => converters == null ? _requiredConverters : converters.Concat(_requiredConverters);

        public ScalarRepository(string filePath) : base(filePath, ConcatConverters())
        {
        }

        public ScalarRepository(string filePath, IEnumerable<JsonConverter> converters) : 
            base(filePath, ConcatConverters(converters))
        {
        }

        public void SetData(string id, ScalarData<int> data, ClaimsPrincipal user = null)
        {
            var scalar = Get(id).Value;
            scalar.SetData(data);
            Update(scalar);
        }

        public void SetLocked(string id, bool locked, ClaimsPrincipal user = null)
        {
            var scalar = Get(id).Value;
            scalar.Locked = locked;
            Update(scalar);
        }
    }
}