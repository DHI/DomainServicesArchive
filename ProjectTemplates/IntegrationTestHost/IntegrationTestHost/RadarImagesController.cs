namespace BaseWebApi
{
    using DHI.Services.Rasters.Radar.DELIMITEDASCII;
    using DHI.Services.Rasters.WebApi;
    using DHI.Services.Rasters.Zones;

    public class RadarImagesController : RadarImagesController<AsciiImage>
    {
        public RadarImagesController(IZoneRepository zoneRepository)
            : base(zoneRepository)
        {
        }
    }
}
