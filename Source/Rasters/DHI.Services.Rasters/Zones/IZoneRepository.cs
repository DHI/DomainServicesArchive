namespace DHI.Services.Rasters.Zones
{
    using System.Security.Claims;

    /// <summary>
    ///     Zone repository abstraction.
    /// </summary>
    public interface IZoneRepository : IRepository<Zone, string>, IDiscreteRepository<Zone, string>, IUpdatableRepository<Zone, string>
    {
        /// <summary>
        /// Determines whether the repository contains a zone with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if the specified name contains name; otherwise, <c>false</c>.</returns>
        bool ContainsName(string name, ClaimsPrincipal user = null);
    }
}