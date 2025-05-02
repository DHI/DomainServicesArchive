namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    /// <summary>
    ///     Connection for creation of a GroupedMapService
    /// </summary>
    public class GroupedMapServiceConnection : BaseConnection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupedMapServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public GroupedMapServiceConnection(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Gets or sets the map source connection string.
        /// </summary>
        /// <value>The map source connection string.</value>
        public virtual string MapSourceConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the map source properties.
        /// </summary>
        /// <value>The map source properties.</value>
        public Parameters MapSourceProperties { get; set; }

        /// <summary>
        ///     Gets or sets the type of the map source.
        /// </summary>
        /// <value>The type of the map source.</value>
        public string MapSourceType { get; set; }

        /// <summary>
        ///     Gets or sets the map style connection string.
        /// </summary>
        /// <value>The map style connection string.</value>
        public virtual string MapStyleConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets the type of the map style repository.
        /// </summary>
        /// <value>The type of the map style repository.</value>
        public string MapStyleRepositoryType { get; set; }

        /// <summary>
        ///     Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string path = null) where TConnection : GroupedMapServiceConnection
        {
            var connectionType = new ConnectionType("GroupedMapServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("MapSourceType", GroupedMapService.GetMapSourceTypes(path)));
            connectionType.ProviderTypes.Add(new ProviderType("MapStyleRepositoryType", GroupedMapService.GetMapStyleRepositoryTypes(path), false));
            connectionType.ProviderArguments.Add(new ProviderArgument("MapSourceConnectionString", typeof(string)));
            connectionType.ProviderArguments.Add(new ProviderArgument("MapStyleConnectionString", typeof(string), false));
            connectionType.ProviderArguments.Add(new ProviderArgument("MapSourceProperties", typeof(Parameters)));
            return connectionType;
        }

        /// <summary>
        ///     Creates a GroupedMapService instance.
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
                var mapSource = (IGroupedMapSource)Activator.CreateInstance(mapSourceType, MapSourceConnectionString, MapSourceProperties);
                return new GroupedMapService(mapSource, mapStyleService);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        /// <summary>
        ///     Determines whether the MapSourceProperties property should be serialized.
        /// </summary>
        public bool ShouldSerializeMapSourceProperties()
        {
            return !(MapSourceProperties is null) && MapSourceProperties.Any();
        }
    }
}