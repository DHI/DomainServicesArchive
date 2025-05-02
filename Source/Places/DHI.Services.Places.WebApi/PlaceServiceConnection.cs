namespace DHI.Services.Places.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using GIS;
    using Scalars;
    using TimeSeries;
    using WebApiCore;

    /// <summary>
    ///     PlaceServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    public class PlaceServiceConnection : Places.PlaceServiceConnection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PlaceServiceConnection"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="repositoryType">The repository type.</param>
        /// <param name="gisServiceConnectionId">The GISService connection identifier.</param>
        public PlaceServiceConnection(string id, string name, string connectionString, string repositoryType, string gisServiceConnectionId)
            : base(id, name, connectionString, repositoryType, gisServiceConnectionId)
        {
        }

        /// <summary>
        ///     Creates a PlaceService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = (PlaceRepository)Activator.CreateInstance(repositoryType, ConnectionString.Resolve(), null);
                var gisService = Services.Get<IGisService<string>>(GisServiceConnectionId);
                Dictionary<string, IDiscreteTimeSeriesService<string, double>> timeSeriesServices = null;
                if (TimeSeriesServiceConnectionIds != null)
                {
                    timeSeriesServices = new Dictionary<string, IDiscreteTimeSeriesService<string, double>>();
                    foreach (var connectionId in TimeSeriesServiceConnectionIds)
                    {
                        timeSeriesServices.Add(connectionId, Services.Get<IDiscreteTimeSeriesService<string, double>>(connectionId));
                    }
                }

                Dictionary<string, IScalarService<string, int>> scalarServices = null;
                if (ScalarServiceConnectionIds != null)
                {
                    scalarServices = new Dictionary<string, IScalarService<string, int>>();
                    foreach (var connectionId in ScalarServiceConnectionIds)
                    {
                        scalarServices.Add(connectionId, Services.Get<IScalarService<string, int>>(connectionId));
                    }
                }

                return new PlaceService(repository, timeSeriesServices, scalarServices, gisService);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}