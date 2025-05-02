namespace DHI.Services
{
    /// <summary>
    ///     Interface IConnectionRepository
    /// </summary>
    public interface IConnectionRepository : IRepository<IConnection, string>, IDiscreteRepository<IConnection, string>, IUpdatableRepository<IConnection, string>
    {
    }
}