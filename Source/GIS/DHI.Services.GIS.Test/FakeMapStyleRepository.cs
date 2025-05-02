namespace DHI.Services.GIS.Test
{
    using System.Collections.Generic;
    using Maps;

    internal class FakeMapStyleRepository : FakeRepository<MapStyle, string>, IMapStyleRepository
    {
        public FakeMapStyleRepository()
        {
        }

        public FakeMapStyleRepository(List<MapStyle> mapStyleList)
            : base(mapStyleList)
        {
        }
    }
}