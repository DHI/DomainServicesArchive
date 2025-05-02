namespace DHI.Services.Tables.Test
{
    using System;
    using System.Linq;
    using AutoFixture.Xunit2;
    using Tables;
    using Xunit;

    public class TableTest
    {
        [Theory, AutoData]
        public void AddingColumnWithExistingNameThrows(Table table)
        {
            table.Columns.Add(new Column("Name", typeof (string)));
            Assert.Throws<ArgumentException>(() => table.Columns.Add(new Column("Name", typeof(string))));
        }

        [Theory, AutoData]
        public void ContainsColumnIsOk(Table table)
        {
            table.Columns.Add(new Column("Name", typeof (string)));
            Assert.True(table.ContainsColumn("Name"));   
        }

        [Theory, AutoData]
        public void DoesNotContainColumnIsOk(Table table)
        {
            Assert.False(table.ContainsColumn("Name"));
        }

        [Theory, AutoData]
        public void KeyColumnsIsOk(Table table)
        {
            table.Columns.Add(new Column("ID", typeof(int), true));
            Assert.Equal("ID", table.KeyColumns.Single().Name);
        }
    }
}
