namespace DHI.Services.TimeSeries.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using Xunit;

    public class EquidistantTimeSeriesSerializerTest
    {
        [Fact]
        public void DeserializeNonExistingThrows()
        {
            var serializer = new EquidistantTimeSeriesSerializer();
            Assert.Throws<FileNotFoundException>(() => serializer.Deserialize("NonExistingFile").ToList());
        }

        [Fact]
        public void DeserializeNullThrows()
        {
            var serializer = new EquidistantTimeSeriesSerializer();
            Assert.Throws<ArgumentNullException>(() => serializer.Deserialize(null).ToList());
        }

        [Fact]
        public void SerializeNonEquidistantThrows()
        {
            var serializer = new EquidistantTimeSeriesSerializer();

            // Create non-equidistant test time series
            var startTime = DateTime.Now;
            var time = new[] {startTime, startTime.AddDays(1), startTime.AddHours(1)};
            var values = Enumerable.Repeat(1.23, time.Length).ToList();
            var data = new TimeSeriesData<double>(time, values);
            var name = FullName.Parse("group/name");
            var ts = new TimeSeries<string, double>(name.ToString(), name.Name, name.Group, "", "WaterLevel", "m", data);

            // Serialize
            Assert.Throws<ArgumentException>(() => serializer.Serialize(new[] {ts, ts}));
        }

        [Fact]
        public void SerializeAndDeserializeIsOk()
        {
            var testFilePath = EquidistantTimeSeriesSerializer.GetTempFilePath("test_file", true);
            var serializer = new EquidistantTimeSeriesSerializer(testFilePath);

            // Create test time series
            var startTime = DateTime.Now;
            var timeStep = TimeSpan.FromHours(1);

            var time = TimeSeriesData<double>.CreateEquidistantDateTimes(startTime, startTime.AddDays(1), timeStep).ToList();
            var values = Enumerable.Repeat(1.23, time.Count).ToList();
            var data = new TimeSeriesData<double>(time, values);
            var name = FullName.Parse("group/name");
            var ts = new TimeSeries<string, double>(name.ToString(), name.Name, name.Group, "", "WaterLevel", "m", data);

            // Serialize
            serializer.Serialize(new[] {ts, ts});

            // Deserialize
            var timeSeries = serializer.Deserialize(serializer.FilePath).ToList();

            Assert.True(timeSeries[0].Data.DateTimes.Count == time.Count);
            Assert.Equal(timeSeries[0].Data.Values[0], values[0]);
            Assert.Equal("WaterLevel", timeSeries[0].Quantity);
            Assert.Equal("m", timeSeries[0].Unit);
            Assert.Equal("group/name", timeSeries[0].Id);
        }
    }
}