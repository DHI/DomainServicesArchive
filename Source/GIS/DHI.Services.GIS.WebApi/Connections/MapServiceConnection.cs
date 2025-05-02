namespace DHI.Services.GIS.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Maps;
    using WebApiCore;

    /// <summary>
    ///     GisServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    /// <seealso cref="Maps.MapServiceConnection" />
    public class MapServiceConnection : Maps.MapServiceConnection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MapServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public MapServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a MapService instance.
        /// </summary>
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
                return new MapService(mapSource, mapStyleService);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}