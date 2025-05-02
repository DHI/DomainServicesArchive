namespace DHI.Services.TimeSeries.Test
{
    using Xunit;

    public class VectorTest
    {
        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 45)]
        [InlineData(-1, -1, -135)]
        [InlineData(2.3, 0, 0)]
        [InlineData(0, 4.55, 90)]
        [InlineData(0, -4.55, -90)]
        [InlineData(-123, 0, 180)]
        public void DirectionIsOk(float x, float y, double direction)
        {
            Assert.Equal(direction, new Vector<float>(x, y).Direction);
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 1.4142)]
        [InlineData(-1, -1, 1.4142)]
        [InlineData(2.3, 0, 2.3)]
        [InlineData(0, 4.55, 4.55)]
        [InlineData(0, -4.55, 4.55)]
        [InlineData(-123, 0, 123)]
        public void SizeIsOk(float x, float y, double size)
        {
            Assert.Equal(size, new Vector<float>(x, y).Size, 4);
        }

        [Fact]
        public void EqualityIsOk()
        {
            var vector1 = new Vector<double>(12.3, 45.6);
            var vector2 = new Vector<double>(12.3, 45.6);
            var vector3 = new Vector<double>(12.39, 45.69);

            Assert.True(vector1 == vector2);
            Assert.False(vector1 == vector3);
            Assert.False(vector1 != vector2);
            Assert.True(vector1 != vector3);
        }

        [Fact]
        public void ToStringIsOk()
        {
            Assert.Equal("(12.3, 45.6)", new Vector<float>(12.3f, 45.6f).ToString());
        }

    }
}