namespace DHI.Services.Spreadsheets.Test
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class SheetTest
    {
        private readonly object[,] _data;
        
        public SheetTest()
        {
            _data = new object[,]
            {
                {"City", "Country", "Population", "Livable", null, ""},
                {"Copenhagen", "Denmark", 1000000, true, 0, "blabla"},
                {"Brisbane", "Australia", 2000000, true, 0, ""},
                {"Duckberg", "", null, true, null, ""},
                {"Delhi", "India", 50000000, false, null, ""}
            };
        }

        [Fact]
        public void ConstructorThrowsIfDuplicateHeaders()
        {
            var data = new object[,]
            {
                {"City", "Country", "Population", "Livable", "Livable"},
                {"Copenhagen", "Denmark", 1000000, true, "Yes"},
                {"Brisbane", "Australia", 2000000, true, "Yes"}
            };

            var e = Assert.Throws<ArgumentException>(() => data.ToSheet());
            Assert.Contains("The column with name 'Livable' has already been added to the column list.", e.Message);
        }

        [Fact]
        public void GetValueThrowsIfNonExistingKey()
        {
            var sheet = new Sheet(_data);
            Assert.Throws<KeyNotFoundException>(() => sheet.GetValue(2, "Country"));
        }

        [Fact]
        public void GetValueThrowsIfNonExistingRow()
        {
            var sheet = new Sheet(_data);
            Assert.Throws<ArgumentOutOfRangeException>(() => sheet.GetValue(99, "Country"));
        }

        [Fact]
        public void SheetCreationIsOk()
        {
            var sheet = new Sheet(_data);
            Assert.IsType<Sheet>(sheet);
            Assert.Equal(4, sheet.Count);

            var row = sheet[1];
            Assert.True(row.ContainsKey("Country"));
            Assert.Equal("Australia", row["Country"]);
            Assert.Equal(4, row.Count);

            row = sheet[2];
            Assert.False(row.ContainsKey("Country"));
            Assert.Equal(2, row.Count);
        }

        [Fact]
        public void GetValueIsOk()
        {
            var sheet = new Sheet(_data);

            Assert.Equal(1000000, (int)sheet.GetValue(0, "Population"));
            Assert.Equal("Australia", (string)sheet.GetValue(1, "Country"));
            Assert.False((bool)sheet.GetValue(3, "Livable"));
        }

        [Fact]
        public void ToSheetIsOk()
        {
            var sheet = _data.ToSheet();
            Assert.IsType<Sheet>(sheet);
            Assert.Equal(4, sheet.Count);

            var row = sheet[1];
            Assert.True(row.ContainsKey("Country"));
            Assert.Equal("Australia", row["Country"]);
            Assert.Equal(4, row.Count);

            row = sheet[2];
            Assert.False(row.ContainsKey("Country"));
            Assert.Equal(2, row.Count);
        }
    }
}