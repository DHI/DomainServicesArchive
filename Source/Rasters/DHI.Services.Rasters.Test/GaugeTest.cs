namespace DHI.Services.Rasters.Test
{
    using Radar;
    using Rasters;
    using Xunit;

    public class GaugeTest
    {
        [Theory]
        [InlineData(10, 10)]
        [InlineData(30, 10)]
        [InlineData(20, 20)]
        [InlineData(26, 1)]
        [InlineData(1, 1)]
        [InlineData(10000, 100000)]
        public void GetWeightOutsideDistanceOfInfluenceIsOk(int col, int row)
        {
            var gauge = new Gauge(new Pixel(20, 10), 10) { WeightFactor = 5 };
            Assert.Equal(1, gauge.GetWeight(new Pixel(col, row)));
        }

        [Fact]
        public void GetWeightInGaugeLocationIsOk()
        {
            var gaugeLocation = new Pixel(20, 10);
            const int factor = 2;
            var gauge = new Gauge(gaugeLocation) { WeightFactor = factor };
            Assert.Equal(factor, gauge.GetWeight(gaugeLocation));
        }

        [Theory]
        [InlineData(21, 1)]
        [InlineData(24, 5)]
        [InlineData(25, 13)]
        [InlineData(18, 12)]
        [InlineData(12, 8)]
        public void GetWeightWithinDistanceOfInfluenceIsOk(int col, int row)
        {
            const int factor = 5;
            var gauge = new Gauge(new Pixel(20, 10), 10) { WeightFactor = factor };
            Assert.InRange(gauge.GetWeight(new Pixel(col, row)), 1, factor);
        }
    }
}