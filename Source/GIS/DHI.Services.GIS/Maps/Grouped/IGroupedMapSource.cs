namespace DHI.Services.GIS.Maps
{
    public interface IGroupedMapSource : IMapSource,
        IGroupedRepository<Layer>,
        IDiscreteRepository<Layer, string>,
        IRepository<Layer, string>,
        IStreamableRepository<string>
    {
    }
}