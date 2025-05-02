namespace DHI.Services.GIS.Maps
{
    using System;

    public class MapStyleService : BaseUpdatableDiscreteService<MapStyle, string>
    {
        public MapStyleService(IMapStyleRepository repository)
            : base(repository)
        {
        }

        public static Type[] GetRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<IMapStyleRepository>(path);
        }
    }
}