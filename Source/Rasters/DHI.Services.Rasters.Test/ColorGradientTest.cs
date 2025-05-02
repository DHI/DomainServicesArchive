namespace DHI.Services.Rasters.Test
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using AutoFixture;
    using Rasters;
    using Xunit;

    public class ColorGradientTest
    {
        private readonly Fixture _fixture;

        public ColorGradientTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void CreateWithoutPointColorsThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ColorGradient(null));
        }

        [Fact]
        public void CreateWithTooFewPointColorsThrows()
        {
            var pointColors = new SortedDictionary<double, Color> { { _fixture.Create<double>(), _fixture.Create<Color>() } };

            Assert.Throws<ArgumentException>(() => new ColorGradient(pointColors));
        }

        [Fact]
        public void CreateWithEnoughPointDoesNotThrow()
        {
            var pointColors = new SortedDictionary<double, Color>();
            _fixture.AddManyTo(pointColors);

            _ = new ColorGradient(pointColors);
        }

        [Fact]
        public void GetFirstAndLastColorIsOk()
        {
            var pointColors = new SortedDictionary<double, Color> { { 0, Color.Red }, { 10, Color.Green } };
            var colorGradient = new ColorGradient(pointColors);

            Assert.Equal(Color.Transparent, colorGradient.GetColor(-1));
            Assert.Equal(Color.Green, colorGradient.GetColor(100));
        }

        [Fact]
        public void GetColorIsOk()
        {
            var pointColors = new SortedDictionary<double, Color> { { 0, Color.Red }, { 10, Color.Yellow }, { 20, Color.Green } };
            var colorGradient = new ColorGradient(pointColors);

            // Red
            var color = colorGradient.GetColor(0);
            Assert.Equal(255, color.R);
            Assert.Equal(0, color.G);
            Assert.Equal(0, color.B);

            // Yellow
            color = colorGradient.GetColor(10);
            Assert.Equal(255, color.R);
            Assert.Equal(255, color.G);
            Assert.Equal(0, color.B);

            // YellowGreen
            color = colorGradient.GetColor(15);
            Assert.Equal(128, color.R);
            Assert.Equal(192, color.G);
            Assert.Equal(0, color.B);

            // Green
            color = colorGradient.GetColor(20);
            Assert.Equal(0, color.R);
            Assert.Equal(128, color.G);
            Assert.Equal(0, color.B);
        }

        [Fact]
        public void ToBitmapIsOk()
        {
            // TODO: implement   
        }
    }
}
