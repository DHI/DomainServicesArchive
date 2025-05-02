namespace DHI.Services.Rasters.Test
{
    using System;
    using System.Drawing;
    using System.IO;
    using AutoFixture;
    using Rasters;
    using Xunit;

    public class MatrixTest
    {
        private readonly IFixture _fixture;

        public MatrixTest()
        {
            _fixture = new Fixture();
        }

        [Theory, AutoData]
        public void UpdateValueThrowIfNoValues(Matrix matrix, Pixel pixel, float value)
        {
            Assert.Throws<Exception>(() => matrix.UpdateValue(pixel, value));
        }

        [Theory, AutoData]
        public void UpdateValueThrowIfIllegalPixel(Matrix matrix, float value)
        {
            _fixture.AddManyTo(matrix.Values, matrix.Size.Width * matrix.Size.Height);
            var pixel = new Pixel(matrix.Size.Width + 1, matrix.Size.Height);
            Assert.Throws<ArgumentException>(() => matrix.UpdateValue(pixel, value));
        }

        [Theory, AutoData]
        public void HasValuesIsOk(Matrix matrix)
        {
            _fixture.AddManyTo(matrix.Values, matrix.Size.Width * matrix.Size.Height);
            Assert.True(matrix.HasValues);
        }

        [Theory, AutoData]
        public void DoesNotHaveValuesIsOk(Matrix matrix)
        {
            Assert.False(matrix.HasValues);

            _fixture.AddManyTo(matrix.Values, matrix.Size.Width +999 * matrix.Size.Height);
            Assert.False(matrix.HasValues);
        }

        [Theory, AutoData]
        public void GetValueIsOk(Matrix matrix)
        {
            _fixture.AddManyTo(matrix.Values, matrix.Size.Width * matrix.Size.Height);
            var pixel = new Pixel(matrix.Size.Width, matrix.Size.Height);

            var expected = matrix.Values[matrix.Size.Width * matrix.Size.Height - 1];
            Assert.Equal(expected, matrix.GetValue(pixel));
        }

        [Theory, AutoData]
        public void UpdateValueIsOk(Matrix matrix)
        {
            _fixture.AddManyTo(matrix.Values, matrix.Size.Width * matrix.Size.Height);
            var pixel = new Pixel(matrix.Size.Width, matrix.Size.Height);

            matrix.UpdateValue(pixel, 999.99f);
            Assert.Equal(999.99f, matrix.GetValue(pixel));
        }

        [Fact]
        public void ToFileAndCreateNewFromFileIsOk()
        {
            // Setup fixture
            var filename = $"{DateTime.Now:yyyyMMddHHmmss}.radarimage";
            var filePath = Path.Combine(Path.GetTempPath(), filename);
            var dateTime = DateTime.Now;
            var matrixSize = new Size(240, 240);
            var matrix = new Matrix(dateTime) { Size = matrixSize };
            for (var i = 0; i < matrixSize.Height * matrixSize.Width; i++)
            {
                matrix.Values.Add(101.11f);
            }

            matrix.UpdateValue(new Pixel(20, 20), 110);
            matrix.UpdateValue(new Pixel(220, 20), 120);
            matrix.UpdateValue(new Pixel(20, 220), 130);

            // Exercise system
            matrix.ToFile(filePath);
            var matrix2 = Matrix.CreateNew(filePath);

            // verify outcome
            Assert.Equal(matrixSize, matrix2.Size);
            Assert.Equal(dateTime, matrix2.DateTime);
            Assert.Equal(101.11f, matrix2.GetValue(new Pixel(240, 240)));
            Assert.Equal(110f, matrix2.GetValue(new Pixel(20, 20)));
            Assert.Equal(120f, matrix2.GetValue(new Pixel(220, 20)));
            Assert.Equal(130f, matrix2.GetValue(new Pixel(20, 220)));

            // tear down
            File.Delete(filePath);
        }
    }
}