namespace DHI.Services.Samples.Radar.ViewModels.Types
{
    using System;
    using DHI.Services.Rasters.Zones;

    public sealed class ZoneItem
    {
        public ZoneItem(Zone zone) { Zone = zone; }
        public Zone Zone { get; }
        public string DisplayName => $"{Zone.Name}  (pixels: {Zone.PixelWeights.Count})";
        public override string ToString() => DisplayName;
    }
}
