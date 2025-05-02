namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     Map Service Connection with file based caching.
    /// </summary>
    public class FileCachedMapServiceConnection : MapServiceConnection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FileCachedMapServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public FileCachedMapServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public new static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : FileCachedMapServiceConnection
        {
            var connectionType = new ConnectionType("FileCachedMapServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("MapSourceType", MapService.GetMapSourceTypes(path)));
            connectionType.ProviderTypes.Add(new ProviderType("MapStyleRepositoryType", MapService.GetMapStyleRepositoryTypes(path), false));
            connectionType.ProviderArguments.Add(new ProviderArgument("MapSourceConnectionString", typeof(string)));
            connectionType.ProviderArguments.Add(new ProviderArgument("MapStyleConnectionString", typeof(string), false));
            connectionType.ProviderArguments.Add(new ProviderArgument("MapSourceProperties", typeof(Parameters)));
            return connectionType;
        }

        /// <summary>
        ///     Creates a MapService instance.
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
                    var mapStyleRepository = (IMapStyleRepository)Activator.CreateInstance(mapStyleRepositoryType, MapStyleConnectionString);
                    mapStyleService = new MapStyleService(mapStyleRepository);
                }

                var mapSourceType = Type.GetType(MapSourceType, true);
                var mapSource = (IMapSource)Activator.CreateInstance(mapSourceType, MapSourceConnectionString, MapSourceProperties);
                var cachedMapSource = new FileCachedMapSource(mapSource, MapSourceProperties);
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