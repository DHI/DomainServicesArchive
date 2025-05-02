namespace DHI.Services.Tables.Test
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Xunit2;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoTableDataAttribute : AutoDataAttribute
    {
        public AutoTableDataAttribute()
            : base(() =>
            {
                var fixture = new Fixture();
                var tableList = fixture.CreateMany<Table>().ToList();
                foreach (var table in tableList)
                {
                    fixture.AddManyTo(table.Columns);
                    fixture.AddManyTo(table.Data);
                }

                fixture.Register<ITableRepository>(() => new FakeTableRepository(tableList));
                return fixture;
            })
        {
        }
    }
}