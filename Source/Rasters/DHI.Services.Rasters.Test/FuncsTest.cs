namespace DHI.Services.Rasters.Test
{
    using System;
    using AutoFixture;
    using Radar;
    using Xunit;

    public class FuncsTest
    {
        private readonly Fixture _fixture;

        public FuncsTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void ReflectivityToIntensityWithIllegalReflectivityThrows()
        {
            // Setup fixture
            var coefficients = _fixture.Create<ConversionCoefficients>();

            // Exercise system and verify outcome
            Assert.Throws<ArgumentException>(() => Funcs.ReflectivityToIntensity(1000, coefficients));
            Assert.Throws<ArgumentException>(() => Funcs.ReflectivityToIntensity(-100, coefficients));
        }

        [Fact]
        public void ReflectivityToIntensityIsOk()
        {
            // Exercise system and verify outcome
            Assert.InRange(Funcs.ReflectivityToIntensity(10), 0.14, 0.16);
            Assert.InRange(Funcs.ReflectivityToIntensity(20), 0.5, 0.7);
            Assert.InRange(Funcs.ReflectivityToIntensity(30), 2.6, 2.8);
            Assert.InRange(Funcs.ReflectivityToIntensity(40), 11.5, 11.6);
            Assert.InRange(Funcs.ReflectivityToIntensity(50), 48.5, 48.7);
            Assert.InRange(Funcs.ReflectivityToIntensity(55), 99, 100);
            Assert.InRange(Funcs.ReflectivityToIntensity(60), 204, 206);
        }

        [Fact]
        public void ReflectivityToIntensityUnitAdjustmentIsOk()
        {
            // Setup fixture
            var coefficients = ConversionCoefficients.Default;
            coefficients.RainIntensityUnit = RainIntensityUnit.MicroMetersPerSecond;

            // Exercise system and verify outcome
            Assert.InRange(Funcs.ReflectivityToIntensity(10, coefficients), 0.14 / 3.6, 0.16 / 3.6);
            Assert.InRange(Funcs.ReflectivityToIntensity(20, coefficients), 0.5 / 3.6, 0.7 / 3.6);
            Assert.InRange(Funcs.ReflectivityToIntensity(30, coefficients), 2.6 / 3.6, 2.8 / 3.6);
            Assert.InRange(Funcs.ReflectivityToIntensity(40, coefficients), 11.5 / 3.6, 11.6 / 3.6);
            Assert.InRange(Funcs.ReflectivityToIntensity(50, coefficients), 48.5 / 3.6, 48.7 / 3.6);
            Assert.InRange(Funcs.ReflectivityToIntensity(55, coefficients), 99 / 3.6, 100 / 3.6);
            Assert.InRange(Funcs.ReflectivityToIntensity(60, coefficients), 204 / 3.6, 206 / 3.6);
        }

        [Fact]
        public void ReflectivityToIntensityAdjustmentIsOk()
        {
            // Setup fixture
            var coefficients = ConversionCoefficients.Default;
            const double slope = 1.1;
            const double offset = 0.1;
            coefficients.IntensitySlope = slope;
            coefficients.IntensityOffset = offset;

            // Exercise system and verify outcome
            Assert.InRange(Funcs.ReflectivityToIntensity(10, coefficients), 0.14 * slope + offset, 0.16 * slope + offset);
            Assert.InRange(Funcs.ReflectivityToIntensity(20, coefficients), 0.5 * slope + offset, 0.7 * slope + offset);
            Assert.InRange(Funcs.ReflectivityToIntensity(30, coefficients), 2.6 * slope + offset, 2.8 * slope + offset);
            Assert.InRange(Funcs.ReflectivityToIntensity(40, coefficients), 11.5 * slope + offset, 11.6 * slope + offset);
            Assert.InRange(Funcs.ReflectivityToIntensity(50, coefficients), 48.5 * slope + offset, 48.7 * slope + offset);
            Assert.InRange(Funcs.ReflectivityToIntensity(55, coefficients), 99 * slope + offset, 100 * slope + offset);
            Assert.InRange(Funcs.ReflectivityToIntensity(60, coefficients), 204 * slope + offset, 206 * slope + offset);
        }
    }
}