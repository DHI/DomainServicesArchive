namespace DHI.Services.Rasters.Zones
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     JSON Zone Repository.
    /// </summary>
    /// <seealso cref="JsonRepository{Zone, String}" />
    /// <seealso cref="IZoneRepository" />
    public class ZoneRepository : JsonRepository<Zone, string>, IZoneRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ZoneRepository" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="converters">the converters.</param>
        public ZoneRepository(string filePath, IEnumerable<JsonConverter> converters = null)
            : base(filePath, converters)
        {
        }

        /// <summary>
        ///     Determines whether the repository contains a zone with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if the specified name contains name; otherwise, <c>false</c>.</returns>
        public bool ContainsName(string name, ClaimsPrincipal user = null)
        {
            return GetAll(user).Any(zone => zone.Name.Equals(name));
        }
    }
}