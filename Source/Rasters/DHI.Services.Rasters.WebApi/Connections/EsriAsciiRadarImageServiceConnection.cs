namespace DHI.Services.Rasters.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Radar;
    using Radar.IRISCAPPI;
    using WebApiCore;

    /// <summary>
    ///     RadarImageServiceConnection for IRISCAPPI images supporting connection string resolvation of [AppData].
    /// </summary>
    public class IrisCappiRadarImageServiceConnection : RadarImageServiceConnection<RadarImage> 
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IrisCappiRadarImageServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public IrisCappiRadarImageServiceConnection(string id, string name) : base(id, name)
        {
        }

        /// <summary>
        ///     Creates a RadarImageService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(RepositoryType, true);
                var repository = Activator.CreateInstance(repositoryType, ConnectionString.Resolve());
                return new RadarImageService<RadarImage>((IRasterRepository<RadarImage>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}