namespace DHI.Services.Spreadsheets.Test
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Xunit2;

    [AttributeUsage(AttributeTargets.Method)]
    internal class AutoSpreadsheetDataAttribute : AutoDataAttribute
    {
        public AutoSpreadsheetDataAttribute()
            : base(() =>
            {
                var fixture = new Fixture();
                var spreadsheetList = fixture.CreateMany<Spreadsheet<Guid>>().ToList();
                fixture.Register<ISpreadsheetRepository<Guid>>(() => new FakeSpreadsheetRepository<Guid>(spreadsheetList));
                return fixture;
            })
        {
        }
    }
}