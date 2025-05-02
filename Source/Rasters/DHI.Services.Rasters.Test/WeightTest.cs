namespace DHI.Services.Rasters.Test
{
    using System;
    using Xunit;
    using Zones;

    public class WeightTest
    {
        [Fact]
        public void CreateIllegalWeightThrows()
        {
            // Exercise system and verify outcome
            Assert.Throws<ArgumentOutOfRangeException>(() => new Weight(-999));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Weight(-0.1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Weight(1.1));
        }

        [Fact]
        public void CreateWeightIsOk()
        {
            // Create fixture
            var weight = new Weight(0.12345);

            // Exercise system and verify outcome
            Assert.Equal(0.12345, weight.Value);
        }
    }
}