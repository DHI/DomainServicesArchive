namespace DHI.Services.Rasters.Test
{
    using System.Linq;
    using System.Security.Claims;
    using Zones;

    public class FakeZoneRepository : FakeRepository<Zone, string>, IZoneRepository
    {
        public bool ContainsName(string name, ClaimsPrincipal user = null)
        {
            return Entities.Values.Any(z => z.Name.Equals(name));
        }
    }
}