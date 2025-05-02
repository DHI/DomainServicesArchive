namespace DHI.Services.Rasters.Test
{
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using Rasters;
    using Xunit;
    using Zones;

    public class ZoneTest
    {
        private readonly IFixture _fixture;

        public ZoneTest()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
        }

        [Fact]
        public void PixelsAreOnlyRepresentedOnce()
        {
            // Setup fixture
            var pixel = _fixture.Create<Pixel>();
            var weight = new Weight(1);
            var zone = new Zone(_fixture.Create<string>(), _fixture.Create<string>());
            zone.PixelWeights.Add(new PixelWeight(pixel, weight));
            zone.PixelWeights.Add(new PixelWeight(pixel, weight));

            // Exercise system and verify outcome
            Assert.Single(zone.PixelWeights);
        }

        [Fact]
        public void PixelWeightTotalIsOk()
        {
            // Setup fixture
            var pixel1 = _fixture.Create<Pixel>();
            var pixel2 = _fixture.Create<Pixel>();
            var zone = new Zone(_fixture.Create<string>(), _fixture.Create<string>());
            zone.PixelWeights.Add(new PixelWeight(pixel1, new Weight(.2)));
            zone.PixelWeights.Add(new PixelWeight(pixel2, new Weight(.4)));

            // Exercise system and verify outcome
            Assert.InRange(zone.PixelWeightTotal, .5999, .6001);
            Assert.False(zone.PixelWeightsAreValid);
        }

        [Fact]
        public void PixelWeightsAreValidIsOk()
        {
            // Setup fixture
            var pixel1 = _fixture.Create<Pixel>();
            var pixel2 = _fixture.Create<Pixel>();
            var zone = new Zone(_fixture.Create<string>(), _fixture.Create<string>());
            zone.PixelWeights.Add(new PixelWeight(pixel1, new Weight(.5)));
            zone.PixelWeights.Add(new PixelWeight(pixel2, new Weight(.4999)));

            Assert.True(zone.PixelWeightsAreValid);
        }
    }
}