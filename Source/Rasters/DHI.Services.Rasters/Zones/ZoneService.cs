namespace DHI.Services.Rasters.Zones
{
    using System;
    using System.Security.Claims;

    /// <summary>
    ///     Class ZoneService.
    /// </summary>
    public class ZoneService : BaseUpdatableDiscreteService<Zone, string>
    {
        private readonly IZoneRepository _repository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ZoneService" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public ZoneService(IZoneRepository repository)
            : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IZoneRepository>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IZoneRepository>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">
        ///     The path where to look for compatible providers. If path is null, the path of the executing assembly is used.
        /// </param>
        /// <param name="searchPattern">
        ///     File name search pattern. Can contain a combination of valid literal path and wildcard (*and ?) characters.
        /// </param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IZoneRepository>(path, searchPattern);
        }

        /// <summary>
        ///     Adds the specified zone.
        /// </summary>
        /// <param name="zone">The zone.</param>
        /// <param name="user">The user.</param>
        /// <exception cref="System.ArgumentException">
        ///     Thrown if a zone with the same name already exists or if zone name is null or empty string.
        /// </exception>
        public override void Add(Zone zone, ClaimsPrincipal user = null)
        {
            if (_repository.ContainsName(zone.Name, user))
            {
                throw new ArgumentException($"There is already a zone with the name '{zone.Name}'.", nameof(zone));
            }

            if (zone.Name == null || zone.Name.Equals(string.Empty))
            {
                throw new ArgumentException("Zone name cannot be null or empty string.", nameof(zone));
            }

            if (!zone.PixelWeightsAreValid)
            {
                throw new ArgumentException($"The accumulated pixel weight is {zone.PixelWeightTotal} but must be 1.", nameof(zone));
            }

            base.Add(zone, user);
        }
    }
}