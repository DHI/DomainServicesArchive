namespace DHI.Services.Places
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using GIS;
    using Scalars;
    using TimeSeries;

    public class PlaceServiceConnection<TTimeSeriesId, TScalarId, TScalarFlag, TCollectionId> : BaseConnection
        where TScalarFlag : struct
        where TCollectionId : notnull
    {
        public PlaceServiceConnection(string id, string name, string connectionString, string repositoryType, string gisServiceConnectionId)
            : base(id, name)
        {
            Guard.Against.NullOrEmpty(connectionString, nameof(connectionString));
            Guard.Against.NullOrEmpty(repositoryType, nameof(repositoryType));
            Guard.Against.NullOrEmpty(gisServiceConnectionId, nameof(gisServiceConnectionId));
            ConnectionString = connectionString;
            RepositoryType = repositoryType;
            GisServiceConnectionId = gisServiceConnectionId;
        }

        /// <summary>
        ///     Gets or sets the place repository connection string.
        /// </summary>
        /// <value>The place repository connection string.</value>
        public string ConnectionString { get; }

        /// <summary>
        ///     Gets or sets the type of the place repository.
        /// </summary>
        /// <value>The type of the Place repository.</value>
        public string RepositoryType { get; }

        public string GisServiceConnectionId { get; } 

        public HashSet<string>? TimeSeriesServiceConnectionIds { get; set; }

        public HashSet<string>? ScalarServiceConnectionIds { get; set; }

        /// <summary>
        ///     Creates the connection type.
        /// </summary>
        /// <typeparam name="TConnection">The type of the connection.</typeparam>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>ConnectionType.</returns>
        public static ConnectionType CreateConnectionType<TConnection>(string? path = null) where TConnection 
            : PlaceServiceConnection<TTimeSeriesId, TScalarId, TScalarFlag, TCollectionId>
        {
            var connectionType = new ConnectionType("ScalarServiceConnection", typeof(TConnection));
            connectionType.ProviderTypes.Add(new ProviderType("RepositoryType", PlaceService<TTimeSeriesId, TScalarId, TScalarFlag, TCollectionId>.GetRepositoryTypes(path)));
            connectionType.ProviderArguments.Add(new ProviderArgument("ConnectionString", typeof(string)));
            connectionType.ProviderArguments.Add(new ProviderArgument("GisServiceConnectionId", typeof(string)));
            connectionType.ProviderArguments.Add(new ProviderArgument("TimeSeriesServiceConnectionIds", typeof(HashSet<string>), false));
            connectionType.ProviderArguments.Add(new ProviderArgument("ScalarServiceConnectionIds", typeof(HashSet<string>), false));
            return connectionType;
        }

        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = (IPlaceRepository<TCollectionId>)Activator.CreateInstance(repositoryType, ConnectionString);
                var gisService = Services.Get<IGisService<TCollectionId>>(GisServiceConnectionId);
                Dictionary<string, IDiscreteTimeSeriesService<TTimeSeriesId, double>>? timeSeriesServices = null;
                if (TimeSeriesServiceConnectionIds != null)
                {
                    timeSeriesServices = new Dictionary<string, IDiscreteTimeSeriesService<TTimeSeriesId, double>>();
                    foreach (var connectionId in TimeSeriesServiceConnectionIds)
                    {
                        timeSeriesServices.Add(connectionId, Services.Get<IDiscreteTimeSeriesService<TTimeSeriesId, double>>(connectionId));
                    }
                }

                Dictionary<string, IScalarService<TScalarId, TScalarFlag>>? scalarServices = null;
                if (ScalarServiceConnectionIds != null)
                {
                    scalarServices = new Dictionary<string, IScalarService<TScalarId, TScalarFlag>>();
                    foreach (var connectionId in ScalarServiceConnectionIds)
                    {
                        scalarServices.Add(connectionId, Services.Get<IScalarService<TScalarId, TScalarFlag>>(connectionId));
                    }
                }

                return new PlaceService<TTimeSeriesId, TScalarId, TScalarFlag, TCollectionId>(repository, timeSeriesServices, scalarServices, gisService);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }

    public class PlaceServiceConnection : PlaceServiceConnection<string, string, int, string>
    {
        public PlaceServiceConnection(string id, string name, string connectionString, string repositoryType, string gisServiceConnectionId) 
            : base(id, name, connectionString, repositoryType, gisServiceConnectionId)
        {
        }
    }
}