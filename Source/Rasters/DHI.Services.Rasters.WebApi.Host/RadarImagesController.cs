namespace DHI.Services.Rasters.WebApi.Host
{
    using Radar.DELIMITEDASCII;
    using Zones;

    public class RadarImagesController : RadarImagesController<AsciiImage>
    {
        public RadarImagesController(IZoneRepository zoneRepository)
            : base(zoneRepository)
        {
        }
    }
}