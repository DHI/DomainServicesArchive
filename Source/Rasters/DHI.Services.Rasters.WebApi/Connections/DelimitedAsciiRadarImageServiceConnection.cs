namespace DHI.Services.Rasters.WebApi
{
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using Radar;
    using Radar.DELIMITEDASCII;
    using WebApiCore;

    /// <summary>
    ///     RadarImageServiceConnection for DELIMITEDASCII images supporting connection string resolvation of [AppData].
    /// </summary>
    public class DelimitedAsciiRadarImageServiceConnection : RadarImageServiceConnection<AsciiImage> 
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DelimitedAsciiRadarImageServiceConnection" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public DelimitedAsciiRadarImageServiceConnection(string id, string name) : base(id, name)
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
                return new RadarImageService<AsciiImage>((IRasterRepository<AsciiImage>)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}