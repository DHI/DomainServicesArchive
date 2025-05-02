namespace DHI.Services.TimeSeries.Test
{
    using System;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class DataPointTest
    {
        [Fact]
        public void DataPointEqualityIsOk()
        {
            Assert.Equal(new DataPoint(new DateTime(2015, 1, 2), 9999d), new DataPoint(new DateTime(2015, 1, 2), 9999d));
            Assert.Equal(new DataPoint(new DateTime(2015, 1, 2), 9999d), new DataPoint(new DateTime(2015, 1, 2), 8888d));
            Assert.NotEqual(new DataPoint(new DateTime(2015, 1, 2), 9999d), new DataPoint(new DateTime(2014, 1, 2), 9999d));
        }

        [Fact]
        public void DataPointWFlagEqualityIsOk()
        {
            Assert.Equal(new DataPointWFlag<int?>(new DateTime(2015, 1, 2), 9999d, 99), new DataPointWFlag<int?>(new DateTime(2015, 1, 2), 9999d, 99));
            Assert.Equal(new DataPointWFlag<int?>(new DateTime(2015, 1, 2), 9999d, 88), new DataPointWFlag<int?>(new DateTime(2015, 1, 2), 9999d, 99));
            Assert.Equal(new DataPointWFlag<int?>(new DateTime(2015, 1, 2), 9999d, 88), new DataPointWFlag<int?>(new DateTime(2015, 1, 2), 8888d, 99));
            Assert.NotEqual(new DataPointWFlag<int?>(new DateTime(2015, 1, 2), 9999d, 88), new DataPointWFlag<int?>(new DateTime(2014, 1, 2), 8888d, 99));
        }

        [Fact]
        public void DataPointWTimeOfForecastEqualityIsOk()
        {
            Assert.Equal(new DataPointForecasted(new DateTime(2015, 1, 2), 9999d, new DateTime(2015, 1, 2)), new DataPointForecasted(new DateTime(2015, 1, 2), 9999d, new DateTime(2015, 1, 2)));
            Assert.Equal(new DataPointForecasted(new DateTime(2015, 1, 2), 9999d, new DateTime(2015, 1, 2)), new DataPointForecasted(new DateTime(2015, 1, 2), 8888d, new DateTime(2015, 1, 2)));
            Assert.NotEqual(new DataPointForecasted(new DateTime(2015, 1, 2), 9999d, new DateTime(2015, 1, 2)), new DataPointForecasted(new DateTime(2015, 1, 2), 8888d, new DateTime(2014, 1, 2)));
            Assert.NotEqual(new DataPointForecasted(new DateTime(2015, 1, 2), 9999d, new DateTime(2015, 1, 2)), new DataPointForecasted(new DateTime(2015, 1, 2), 9999d, new DateTime(2014, 1, 2)));
        }

        [Fact]
        public void DataPointCompareToIsOk()
        {
            var dp1 = new DataPoint(new DateTime(2015, 1, 2), 9999d);
            var dp2 = new DataPoint(new DateTime(2014, 1, 2), 9999d);
            var dp3 = new DataPoint(new DateTime(2013, 1, 2), 9999d);
            Assert.Equal(0, dp1.CompareTo(dp1));
            Assert.Equal(0, dp2.CompareTo(dp2));
            Assert.Equal(1, dp1.CompareTo(dp2));
            Assert.Equal(1, dp1.CompareTo(dp3));
            Assert.Equal(1, dp2.CompareTo(dp3));
            Assert.Equal(-1, dp2.CompareTo(dp1));
            Assert.Equal(-1, dp3.CompareTo(dp1));
            Assert.Equal(-1, dp3.CompareTo(dp2));
        }

        [Fact]
        public void DataPointWFlagCompareToIsOk()
        {
            var dp1 = new DataPointWFlag<int?>(new DateTime(2015, 1, 2), 9999d, 0);
            var dp2 = new DataPointWFlag<int?>(new DateTime(2014, 1, 2), 9999d, null);
            var dp3 = new DataPointWFlag<int?>(new DateTime(2013, 1, 2), 9999d, 1);
            Assert.Equal(0, dp1.CompareTo(dp1));
            Assert.Equal(0, dp2.CompareTo(dp2));
            Assert.Equal(1, dp1.CompareTo(dp2));
            Assert.Equal(1, dp1.CompareTo(dp3));
            Assert.Equal(1, dp2.CompareTo(dp3));
            Assert.Equal(-1, dp2.CompareTo(dp1));
            Assert.Equal(-1, dp3.CompareTo(dp1));
            Assert.Equal(-1, dp3.CompareTo(dp2));
        }

        [Fact]
        public void DataPointForecastedCompareToIsOk()
        {
            var dp1 = new DataPointForecasted(new DateTime(2015, 1, 2), 9999d, new DateTime(2015, 1, 1));
            var dp2 = new DataPointForecasted(new DateTime(2014, 1, 2), 9999d, new DateTime(2014, 1, 1, 6, 0, 0));
            var dp3 = new DataPointForecasted(new DateTime(2014, 1, 2), 9999d, new DateTime(2014, 1, 1, 5, 0, 0));
            Assert.Equal(0, dp1.CompareTo(dp1));
            Assert.Equal(0, dp2.CompareTo(dp2));
            Assert.Equal(1, dp1.CompareTo(dp2));
            Assert.Equal(1, dp1.CompareTo(dp3));
            Assert.Equal(1, dp2.CompareTo(dp3));
            Assert.Equal(-1, dp2.CompareTo(dp1));
            Assert.Equal(-1, dp3.CompareTo(dp1));
            Assert.Equal(-1, dp3.CompareTo(dp2));
        }

        [Fact]
        public void ValueIsNullable()
        {
            Assert.IsType<DataPoint>(new DataPoint(new DateTime(2015, 1, 2), null));
        }

        [Fact]
        public void FlagIsNullable()
        {
            Assert.IsType<DataPointWFlag<int?>>(new DataPointWFlag<int?>(new DateTime(2015, 1, 2), 9999d, null));
        }

        [Fact]
        public void EqualOperatorIsOk()
        {
            Assert.True(new DataPoint(new DateTime(2015, 1, 2), 9999d) == new DataPoint(new DateTime(2015, 1, 2), 8888d));
            Assert.False(new DataPoint(new DateTime(2015, 1, 2), 9999d) == new DataPoint(new DateTime(2014, 1, 2), 9999d));
        }

        [Fact]
        public void NotEqualOperatorIsOk()
        {
            Assert.False(new DataPoint(new DateTime(2015, 1, 2), 9999d) != new DataPoint(new DateTime(2015, 1, 2), 8888d));
            Assert.True(new DataPoint(new DateTime(2015, 1, 2), 9999d) != new DataPoint(new DateTime(2014, 1, 2), 9999d));
        }

        [Fact]
        public void LessThanOperatorIsOk()
        {
            Assert.False(new DataPoint(new DateTime(2015, 1, 2), 9999d) < new DataPoint(new DateTime(2015, 1, 2), 8888d));
            Assert.True(new DataPoint(new DateTime(2013, 1, 2), 9999d) < new DataPoint(new DateTime(2014, 1, 2), 9999d));
        }

        [Fact]
        public void LessThanOrEqualOperatorIsOk()
        {
            Assert.True(new DataPoint(new DateTime(2015, 1, 2), 9999d) <= new DataPoint(new DateTime(2015, 1, 2), 8888d));
            Assert.True(new DataPoint(new DateTime(2013, 1, 2), 9999d) <= new DataPoint(new DateTime(2014, 1, 2), 9999d));
        }

        [Fact]
        public void GreaterThanOperatorIsOk()
        {
            Assert.False(new DataPoint(new DateTime(2015, 1, 2), 9999d) > new DataPoint(new DateTime(2015, 1, 2), 8888d));
            Assert.True(new DataPoint(new DateTime(2015, 1, 2), 9999d) > new DataPoint(new DateTime(2014, 1, 2), 9999d));
        }

        [Fact]
        public void GreaterThanOrEqualOperatorIsOk()
        {
            Assert.True(new DataPoint(new DateTime(2015, 1, 2), 9999d) >= new DataPoint(new DateTime(2015, 1, 2), 8888d));
            Assert.True(new DataPoint(new DateTime(2015, 1, 2), 9999d) >= new DataPoint(new DateTime(2014, 1, 2), 9999d));
        }
    }
}
