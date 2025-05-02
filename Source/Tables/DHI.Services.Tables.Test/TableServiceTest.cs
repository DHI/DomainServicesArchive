namespace DHI.Services.Tables.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Tables;
    using Xunit;

    public class TableServiceTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new TableService(null));
        }

        [Theory, AutoTableData]
        public void GetNonExistingThrows(TableService tableService, string tableId)
        {
            Assert.False(tableService.TryGet(tableId, out _));
        }

        [Theory, AutoTableData]
        public void UpdateNonExistingThrows(TableService tableService, Table table)
        {
            Assert.Throws<KeyNotFoundException>(() => tableService.Update(table));
        }

        [Theory, AutoTableData]
        public void RemoveNonExistingThrows(TableService tableService, Table table)
        {
            Assert.Throws<KeyNotFoundException>(() => tableService.Remove(table.Id));
        }

        [Theory, AutoTableData]
        public void GetIsOk(TableService tableService)
        {
            var table = tableService.GetAll().ToArray()[0];
            tableService.TryGet(table.Id, out var tb);
            Assert.Equal(table.Id, tb.Id);
        }

        [Theory, AutoTableData]
        public void GetAllIsOk(TableService tableService)
        {
            Assert.Equal(_fixture.RepeatCount, tableService.GetAll().Count());
        }

        [Theory, AutoTableData]
        public void GetIdsIsOk(TableService tableService)
        {
            Assert.Equal(_fixture.RepeatCount, tableService.GetIds().Count());
        }

        [Theory, AutoTableData]
        public void AddAndGetIsOk(TableService tableService, Table table)
        {
            tableService.Add(table);
            tableService.TryGet(table.Id, out var tb);
            Assert.Equal(table.Id, tb.Id);
        }

        [Theory, AutoTableData]
        public void CountIsOk(TableService tableService)
        {
            Assert.Equal(_fixture.RepeatCount, tableService.Count());
        }

        [Theory, AutoTableData]
        public void ExistsIsOk(TableService tableService)
        {
            var table = tableService.GetAll().ToArray()[0];
            Assert.True(tableService.Exists(table.Id));
        }

        [Theory, AutoTableData]
        public void DoesNotExistsIsOk(TableService tableService, string tableId)
        {
            Assert.False(tableService.Exists(tableId));
        }

        [Theory, AutoTableData]
        public void GetColumnsIsOk(TableService tableService)
        {
            var table = tableService.GetAll().First();
            Assert.Equal(_fixture.RepeatCount, tableService.GetColumns(table.Id).Count());
        }

        [Theory, AutoTableData]
        public void GetColumnsForNonExistingTableReturnsEmptyColllection(TableService tableService, string tableId)
        {
            Assert.Empty(tableService.GetColumns(tableId));
        }

        [Theory, AutoTableData]
        public void GetKeyColumnsIsOk(TableService tableService)
        {
            var table = tableService.GetAll().First();
            table.Columns.Add(new Column("ID", typeof(int), true));
            tableService.Update(table);
            Assert.Equal("ID", tableService.GetKeyColumns(table.Id).Single().Name);
        }

        [Theory, AutoTableData]
        public void GetKeyColumnsForNonExistingTableReturnsEmptyColllection(TableService tableService, string tableId)
        {
            Assert.Empty(tableService.GetKeyColumns(tableId));
        }

        [Theory, AutoTableData]
        public void ContainsColumnIsOk(TableService tableService)
        {
            var table = tableService.GetAll().First();
            Assert.True(tableService.ContainsColumn(table.Id, table.Columns.First().Name));
        }

        [Theory, AutoTableData]
        public void DoesNotContainColumnIsOk(TableService tableService, string columnName)
        {
            var table = tableService.GetAll().First();
            Assert.False(tableService.ContainsColumn(table.Id, columnName));
        }

        [Theory, AutoTableData]
        public void ContainsColumnForNonExistingTableReturnsFalse(TableService tableService, string tableId)
        {
            var table = tableService.GetAll().First();
            Assert.False(tableService.ContainsColumn(tableId, table.Columns.First().Name));
        }

        [Theory, AutoTableData]
        public void EventsAreRaisedOnAdd(TableService tableService, Table table)
        {
            var raisedEvents = new List<string>();
            tableService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            tableService.Added += (s, e) => { raisedEvents.Add("Added"); };

            tableService.Add(table);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoTableData]
        public void RemoveIsOk(TableService tableService, Table table)
        {
            tableService.Add(table);
            tableService.Remove(table.Id);

            Assert.False(tableService.Exists(table.Id));
            Assert.Equal(3, tableService.Count());
        }

        [Theory, AutoTableData]
        public void EventsAreRaisedOnRemove(TableService tableService, Table table)
        {
            var raisedEvents = new List<string>();
            tableService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            tableService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            tableService.Add(table);

            tableService.Remove(table.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoTableData]
        public void UpdateIsOk(TableService tableService, Table table)
        {
            tableService.Add(table);

            var updated = new Table(table.Id);
            updated.Columns.Add(new Column("NewColumn", typeof(string)));
            tableService.Update(updated);

            tableService.TryGet(table.Id, out var tb);
            Assert.True(tb.ContainsColumn("NewColumn"));
        }

        [Theory, AutoTableData]
        public void AddOrUpdateIsOk(TableService tableService, Table table)
        {
            var raisedEvents = new List<string>();
            tableService.Added += (s, e) => { raisedEvents.Add("Added"); };
            tableService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            tableService.AddOrUpdate(table);
            var updated = new Table(table.Id);
            updated.Columns.Add(new Column("NewColumn", typeof(string)));
            tableService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            tableService.TryGet(table.Id, out var tb);
            Assert.True(tb.ContainsColumn("NewColumn"));
        }

        [Theory, AutoTableData]
        public void EventsAreRaisedOnUpdate(TableService tableService, Table table)
        {
            var raisedEvents = new List<string>();
            tableService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            tableService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            tableService.Add(table);

            var updated = new Table(table.Id);
            updated.Columns.Add(new Column("NewColumn", typeof(string)));
            tableService.Update(updated);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }
    }
}
