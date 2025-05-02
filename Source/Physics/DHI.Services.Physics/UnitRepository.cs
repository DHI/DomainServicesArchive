namespace DHI.Services.Physics
{
    using DHI.Services.Physics.Converters;
    using System.Text.Json.Serialization;

    public class UnitRepository : JsonRepository<Unit, string>, IUnitRepository
    {
        public UnitRepository(string filePath)
            : base(filePath, new JsonConverter[] { new UnitConverter(), new DimensionConverter() })
        {
        }
    }
}