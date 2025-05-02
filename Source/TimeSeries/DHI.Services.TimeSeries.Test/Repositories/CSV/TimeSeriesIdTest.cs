namespace DHI.Services.TimeSeries.Test.Repositories.CSV
{
    using System;
    using DHI.Services.TimeSeries.CSV;
    using Xunit;

    public class TimeSeriesIdTest
    {
        [Fact]
        public void ParseInvalidStringThrows()
        {
            Assert.Throws<ArgumentException>(() => TimeSeriesId.Parse(@"folder/subfolder/file.csv"));
        }

        [Fact]
        public void CreateWithNullOrEmptyItemThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new TimeSeriesId("folder/subfolder", null));
            Assert.Throws<ArgumentException>(() => new TimeSeriesId("folder/subfolder", ""));
        }

        [Fact]
        public void CreateWithNullOrEmptyPathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new TimeSeriesId(null, "item"));
            Assert.Throws<ArgumentException>(() => new TimeSeriesId("", "item"));
        }

        [Theory]
        [InlineData(@"folder/subfolder/file.csv;MyItem")]
        [InlineData(@"folder/file.csv;MyItem")]
        public void ParseIsOk(string s)
        {
            Assert.IsType<TimeSeriesId>(TimeSeriesId.Parse(s));
        }

        [Fact]
        public void ParsePropertiesAreOk()
        {
            var timeSeriesId = TimeSeriesId.Parse(@"folder/subfolder/file.csv;MyItem");
            Assert.Equal("file.csv", timeSeriesId.FileName);
            Assert.Equal("folder/subfolder/file.csv", timeSeriesId.RelativeFilePath);
            Assert.Equal(@"folder/subfolder/file.csv;MyItem", timeSeriesId.FullName);
            Assert.Equal(@"folder/subfolder", timeSeriesId.Group);
            Assert.Equal("MyItem", timeSeriesId.ObjId);
            Assert.Equal(@"file.csv;MyItem", timeSeriesId.Name);
        }

        [Theory]
        [InlineData(@"folder/subfolder/file.csv", "MyItem")]
        public void EqualsIsOk(string filePath, string item)
        {
            var ts1 = new TimeSeriesId(filePath, item);
            var ts2 = new TimeSeriesId(filePath, item);
            Assert.Equal(ts1, ts2);
        }

        [Theory]
        [InlineData("MyItem")]
        public void NotEqualsIsOk(string item)
        {
            var ts1 = new TimeSeriesId(@"folder/subfolder/file.csv", item);
            var ts2 = new TimeSeriesId(@"folder/file.csv", item);
            Assert.NotEqual(ts1, ts2);
        }
    }
}