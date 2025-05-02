namespace DHI.Services.Scalars.Test
{
    using System;
    using Xunit;

    public class ScalarDataTest
    {
        [Fact]
        public void AddingNullValueThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ScalarData<int>(null, DateTime.Now));
        }

        [Fact]
        public void ToStringIsOk()
        {
            Assert.Contains("99.99", new ScalarData<int>(99.99, DateTime.Now).ToString());
            Assert.Contains("(-1)", new ScalarData<int>(99.99, DateTime.Now, -1).ToString());
        }

        [Theory, AutoScalarData]
        public void NotEqualIsOk(ScalarData<int> scalarData1, ScalarData<int> scalarData2)
        {
            Assert.NotEqual(scalarData1, scalarData2);
        }

        [Theory, AutoScalarData]
        public void NotEqualOperatorIsOk(ScalarData<int> scalarData1, ScalarData<int> scalarData2)
        {
            Assert.True(scalarData1 != scalarData2);
        }

        [Theory, AutoScalarData]
        public void NotEqualIfDifferentValues(ScalarData<int> scalarData)
        {
            var scalarData2 = new ScalarData<int>((int)scalarData.Value + 1, scalarData.DateTime, scalarData.Flag);
            Assert.NotEqual(scalarData, scalarData2);
        }

        [Theory, AutoScalarData]
        public void NotEqualIfDifferentFlags(ScalarData<int> scalarData)
        {
            var scalarData2 = new ScalarData<int>(scalarData.Value, scalarData.DateTime, scalarData.Flag + 1);
            Assert.NotEqual(scalarData, scalarData2);
        }

        [Theory, AutoScalarData]
        public void EqualEvenIfDifferentDateTimes(ScalarData<int> scalarData)
        {
            var scalarData2 = new ScalarData<int>(scalarData.Value, DateTime.Now, scalarData.Flag);
            Assert.Equal(scalarData, scalarData2);
        }

        [Theory, AutoScalarData]
        public void EqualIsOk(ScalarData<int> scalarData)
        {
            var scalarData2 = scalarData;
            Assert.Equal(scalarData, scalarData2);
        }

        [Theory, AutoScalarData]
        public void EqualOperatorIsOk(ScalarData<int> scalarData)
        {
            var scalarData2 = scalarData;
            Assert.True(scalarData == scalarData2);
        }
    }
}