namespace DHI.Services
{
    /// <summary>
    /// Class ConnectionService.
    /// </summary>
    public class ConnectionService : BaseUpdatableDiscreteService<IConnection, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public ConnectionService(IConnectionRepository repository)
            : base(repository)
        {
        }
    }
}