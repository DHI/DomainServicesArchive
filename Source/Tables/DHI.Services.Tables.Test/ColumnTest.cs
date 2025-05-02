namespace DHI.Services.Tables.Test
{
    using System;
    using Tables;
    using Xunit;

    public class ColumnTest
    {
        [Fact]
        public void CreateWithNullOrEmptyNameThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Column(null, typeof (string)));
            Assert.Throws<ArgumentException>(() => new Column("", typeof(string)));
        }

        [Fact]
        public void CreateWithNullDataTypeThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Column("Name", null));
        }

        [Fact]
        public void EqualityIsOk()
        {
            Assert.Equal(new Column("Name", typeof(string)), new Column("Name", typeof(double)));
            Assert.NotEqual(new Column("Name", typeof(string)), new Column("Description", typeof(string)));
        }
    }
}