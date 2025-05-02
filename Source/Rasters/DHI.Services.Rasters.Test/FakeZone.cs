namespace DHI.Services.Rasters.Test
{
    using Rasters;
    using Zones;

    internal class FakeZone : Zone
    {
        public FakeZone() : this("MyZone", "My Zone")
        {
        }

        public FakeZone(string id, string name) : base(id, name, ZoneType.Point)
        {
            PixelWeights.Add(new PixelWeight(new Pixel(10, 10), new Weight(1)));
        }
    }
}