namespace DHI.Services.GIS.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Maps;
    using WebApiCore;

    /// <summary>
    ///     CachedMapServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    /// <seealso cref="Maps.CachedMapServiceConnection" />
    public class CachedMapServiceConnection : Maps.CachedMapServiceConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedMapServiceConnection"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public CachedMapServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        public override object Create()
        {
            try
            {
                MapStyleService mapStyleService = null;
                if (MapStyleRepositoryType != null && MapStyleConnectionString != null)
                {
                    var mapStyleRepositoryType = Type.GetType(MapStyleRepositoryType, true);
                    var mapStyleRepository = (IMapStyleRepository)Activator.CreateInstance(mapStyleRepositoryType, MapStyleConnectionString.Resolve());
                    mapStyleService = new MapStyleService(mapStyleRepository);
                }

                var mapSourceType = Type.GetType(MapSourceType, true);
                var mapSource = (IMapSource)Activator.CreateInstance(mapSourceType, MapSourceConnectionString.Resolve(), MapSourceProperties);
                var cachedMapSource = new CachedMapSource(mapSource, MapSourceProperties);
                return new MapService(cachedMapSource, mapStyleService);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}