namespace DHI.Services.Tables.Test
{
    using System.Linq;
    using AutoFixture;
    using Tables;
    using Xunit;

    public class BaseTableRepositoryTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Theory, AutoTableData]
        public void CountIsOk(ITableRepository repository)
        {
            Assert.Equal(_fixture.RepeatCount, repository.Count());
        }

        [Theory, AutoTableData]
        public void GetIsOk(ITableRepository repository)
        {
            var table = repository.GetAll().First();
            Assert.Equal(table.Id, repository.Get(table.Id).Value.Id);
        }

        [Theory, AutoTableData]
        public void GetIdsIsOk(ITableRepository repository)
        {
            Assert.True(repository.GetIds().Any());
            Assert.IsType<string>(repository.GetIds().First());
        }

        [Theory, AutoTableData]
        public void ContainsIsOk(ITableRepository repository)
        {
            var table = repository.GetAll().First();
            Assert.True(repository.Contains(table.Id));
        }

        [Theory, AutoTableData]
        public void DoesNotContainIsOk(ITableRepository repository)
        {
            Assert.False(repository.Contains("NonExistingID"));
        }

        [Theory, AutoTableData]
        public void GetColumnsIsOk(ITableRepository repository)
        {
            var table = repository.GetAll().First();
            Assert.Equal(_fixture.RepeatCount, repository.GetColumns(table.Id).Count());
        }

        [Theory, AutoTableData]
        public void GetColumnsForNonExistingTableReturnsEmptyColllection(ITableRepository repository, string tableId)
        {
            Assert.Empty(repository.GetColumns(tableId));
        }

        [Theory, AutoTableData]
        public void GetKeyColumnsIsOk(ITableRepository repository)
        {
            var table = repository.GetAll().First();
            table.Columns.Add(new Column("ID", typeof (int), true));
            repository.Update(table);
            Assert.Equal("ID", repository.GetKeyColumns(table.Id).Single().Name);
        }

        [Theory, AutoTableData]
        public void GetKeyColumnsForNonExistingTableReturnsEmptyColllection(ITableRepository repository, string tableId)
        {
            Assert.Empty(repository.GetKeyColumns(tableId));
        }

        [Theory, AutoTableData]
        public void ContainsColumnIsOk(ITableRepository repository)
        {
            var table = repository.GetAll().First();
            Assert.True(repository.ContainsColumn(table.Id, table.Columns.First().Name));
        }

        [Theory, AutoTableData]
        public void DoesNotContainColumnIsOk(ITableRepository repository, string columnName)
        {
            var table = repository.GetAll().First();
            Assert.False(repository.ContainsColumn(table.Id, columnName));
        }

        [Theory, AutoTableData]
        public void ContainsColumnForNonExistingTableReturnsFalse(ITableRepository repository, string tableId)
        {
            var table = repository.GetAll().First();
            Assert.False(repository.ContainsColumn(tableId, table.Columns.First().Name));
        }
    }
}