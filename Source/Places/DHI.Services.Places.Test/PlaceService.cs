namespace DHI.Services.Places.Test
{
    using System.Collections.Generic;
    using DHI.Services.Scalars;
    using GIS;
    using TimeSeries;

    public class PlaceService : PlaceService<string, string, int, string>
    {
        public PlaceService(IPlaceRepository<string> repository,
            Dictionary<string, IDiscreteTimeSeriesService<string, double>>? timeSeriesServices,
            Dictionary<string, IScalarService<string, int>>? scalarServices,
            IGisService<string> gisService) : base(repository, timeSeriesServices, scalarServices, gisService)
        {
        }
    }
}