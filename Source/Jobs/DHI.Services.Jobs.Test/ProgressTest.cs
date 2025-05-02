namespace DHI.Services.Jobs.Test
{
    using System;
    using Jobs;
    using Xunit;

    public class ProgressTest
    {
        [Theory]
        [InlineData(-10)]
        [InlineData(110)]
        public void IllegalValueThrows(int progress)
        {
            Assert.Throws<ArgumentException>(() => new Progress(progress));
        }

        [Fact]
        public void ComparisonIsOk()
        {
            Assert.True(new Progress(10) < new Progress(20));
            Assert.True(new Progress(20) > new Progress(10));
        }

        [Fact]
        public void EqualityIsOk()
        {
            Assert.Equal(new Progress(10, "running"), new Progress(10));
            Assert.NotEqual(new Progress(20), new Progress(10));
            Assert.True(new Progress(10).Equals(new Progress(10)));
        }
    }
}