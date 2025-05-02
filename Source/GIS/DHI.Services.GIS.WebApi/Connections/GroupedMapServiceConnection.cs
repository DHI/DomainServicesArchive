namespace DHI.Services.GIS.WebApi
{
    using DHI.Services.GIS.Maps;
    using System;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using DHI.Services.WebApiCore;

    /// <summary>
    ///     GroupedMapServiceConnection supporting connection string resolvation of [AppData].
    /// </summary>
    /// <seealso cref="Maps.GroupedMapServiceConnection" />
    public class GroupedMapServiceConnection : Maps.GroupedMapServiceConnection
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
        ///     Creates a GroupedMapService instance.
        /// </summary>
        public override object Create()
        {
            try
            {
                var repositoryType = Type.GetType(MapSourceType, true);
                var repository = Activator.CreateInstance(repositoryType, MapSourceConnectionString.Resolve());
                return new GroupedMapService((IGroupedMapSource)repository);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }
    }
}
