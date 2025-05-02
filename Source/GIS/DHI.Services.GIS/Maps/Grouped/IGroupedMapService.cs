namespace DHI.Services.GIS.Maps
{
    public interface IGroupedMapService: IMapService,
        IGroupedService<Layer>,
        IService<Layer, string>,
        IDiscreteService<Layer, string>,
        IStreamableService<string>
    {
    }
}
