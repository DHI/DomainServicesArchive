namespace DHI.Services.Spreadsheets.Test.Converters
{
    using System.Collections.Generic;
    using System.Text.Json;
    using DHI.Services.Spreadsheets.Converters;
    using Xunit;

    public class JsonConverterTest
    {

        [Fact]
        public void CanSerializeMultiDimensionalArray()
        {
            var array = new List<int[,]> {
                {  new int[,] { { 1, 2 }, { 3, 4 } } },
                {  new int[,] { { 5, 6 }, { 7, 8 } } },
                {  new int[,] { { 9, 10 }, { 11, 12 } } }
            };
            var options = new JsonSerializerOptions();
            options.Converters.Add(new TwoDimensionalArrayConverter<int>());

            var json = JsonSerializer.Serialize(array, options);

            var expected = "[[[1,2],[3,4]],[[5,6],[7,8]],[[9,10],[11,12]]]";
            Assert.Equal(expected, json);
        }

        [Fact]
        public void CanDeserializeMultiDimensionalArray()
        {
            var json = "[[[1,2],[3,4]],[[5,6],[7,8]],[[9,10],[11,12]]]";
            var options = new JsonSerializerOptions();
            options.Converters.Add(new TwoDimensionalArrayConverter<int>());

            var actual = JsonSerializer.Deserialize<List<int[,]>>(json, options);

            var expected = new List<int[,]> {
                {  new int[,] { { 1, 2 }, { 3, 4 } } },
                {  new int[,] { { 5, 6 }, { 7, 8 } } },
                {  new int[,] { { 9, 10 }, { 11, 12 } } }
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanSerializeSpreadSheet()
        {
            var sheet = new Spreadsheet<int>(1001, "name-1001", "group-00");
            sheet.Data.Add(new object[,] { { 1, 2 }, { 3, 4 } });
            sheet.Data.Add(new object[,] { { 0.1, 0.2 }, { 0.3, 0.4 } });
            sheet.Data.Add(new object[,] { { "one", "two" }, { "three", "four" } });

            var options = new JsonSerializerOptions
            {
                Converters = { new SpreadsheetConverter<int>() }
            };

            var actual = JsonSerializer.Serialize(sheet, options);
            var expected = "{\"Id\":1001,\"Name\":\"name-1001\",\"Group\":\"group-00\",\"Data\":[[[1,2],[3,4]],[[0.10000000000000001,0.20000000000000001],[0.29999999999999999,0.40000000000000002]],[[\"one\",\"two\"],[\"three\",\"four\"]]],\"Metadata\":{},\"Permissions\":[]}";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanDeserializeSpreadSheet()
        {
            var expected = new Spreadsheet<int>(1001, "name-1001", "group-00");
            expected.Data.Add(new object[,] { { 1, 2 }, { 3, 4 } });
            expected.Data.Add(new object[,] { { 0.1, 0.2 }, { 0.3, 0.4 } });
            expected.Data.Add(new object[,] { { "one", "two" }, { "three", "four" } });

            var options = new JsonSerializerOptions
            {
                Converters = { new SpreadsheetConverter<int>() }
            };

            var json = "{\"Id\":1001,\"Name\":\"name-1001\",\"Group\":\"group-00\",\"Data\":[[[1,2],[3,4]],[[0.1,0.2],[0.3,0.4]],[[\"one\",\"two\"],[\"three\",\"four\"]]]}";
            var actual = JsonSerializer.Deserialize<Spreadsheet<int>>(json, options);

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Group, actual.Group);
        }
    }
}
