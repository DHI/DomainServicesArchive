namespace DHI.Services.GIS.Maps
{
    /// <summary>
    ///     Interface IMapStyleRepository
    /// </summary>
    public interface IMapStyleRepository : IRepository<MapStyle, string>, IDiscreteRepository<MapStyle, string>, IUpdatableRepository<MapStyle, string>
    {
    }
}