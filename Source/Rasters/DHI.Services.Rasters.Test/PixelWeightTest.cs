namespace DHI.Services.Rasters.Test
{
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using Rasters;
    using Xunit;
    using Zones;

    public class PixelWeightTest
    {
        private readonly IFixture _fixture;

        public PixelWeightTest()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
        }

        [Fact]
        public void EqualityIsDoneOnPixel()
        {
            var pixel = _fixture.Create<Pixel>();
            var pixel2 = _fixture.Create<Pixel>();

            Assert.Equal(new PixelWeight(pixel, new Weight(.5)), new PixelWeight(pixel, new Weight(.9)));
            Assert.Equal(new PixelWeight(pixel, new Weight(.5)), new PixelWeight(pixel, new Weight(.5)));
            Assert.NotEqual(new PixelWeight(pixel, new Weight(.5)), new PixelWeight(pixel2, new Weight(.5)));
        }
    }
}