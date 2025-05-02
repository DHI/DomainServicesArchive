namespace DHI.Services.Connections.WebApi
{

    /// <summary>
    /// Class ConnectionTypeService. This class cannot be inherited.
    /// </summary>
    public sealed class ConnectionTypeService : DHI.Services.ConnectionTypeService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionTypeService"/> class.
        /// </summary>
        /// <param name="path">A path to look for connection types.</param>
        public ConnectionTypeService(string path = null)
            : base(path)
        {
        }
    }
}