namespace DHI.Services.Spreadsheets.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using AutoFixture;
    using Moq;
    using Spreadsheets;
    using Xunit;
    using Range = Spreadsheets.Range;

    public class SpreadsheetServiceTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new SpreadsheetService<Guid>(null));
        }

        [Theory, AutoSpreadsheetData]
        public void GetNonExistingThrows(SpreadsheetService<Guid> spreadsheetService)
        {
            Assert.False(spreadsheetService.TryGet(Guid.NewGuid(), out _));
        }

        [Theory, AutoSpreadsheetData]
        public void GetStreamNonExistingThrows(SpreadsheetService<Guid> spreadsheetService)
        {
            Assert.Throws<KeyNotFoundException>(() => spreadsheetService.GetStream(Guid.NewGuid()));
        }

        [Theory, AutoSpreadsheetData]
        public void UpdateNonExistingThrows(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            Assert.Throws<KeyNotFoundException>(() => spreadsheetService.Update(spreadsheet));
        }

        [Theory, AutoSpreadsheetData]
        public void RemoveNonExistingThrows(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            Assert.Throws<KeyNotFoundException>(() => spreadsheetService.Remove(spreadsheet.Id));
        }

        [Theory, AutoSpreadsheetData]
        public void RemoveByNonExistingGroupThrows(SpreadsheetService<Guid> spreadsheetService)
        {
            Assert.Throws<KeyNotFoundException>(() => spreadsheetService.RemoveByGroup("NonExistingGroup"));
        }

        [Theory, AutoSpreadsheetData]
        public void GetByGroupForNonExistingThrows(SpreadsheetService<Guid> spreadsheetService)
        {
            Assert.Throws<KeyNotFoundException>(() => spreadsheetService.GetByGroup("NonExistingGroup"));
        }

        [Theory, AutoSpreadsheetData]
        public void GetByGroupForNullGroupThrows(SpreadsheetService<Guid> spreadsheetService)
        {
            Assert.Throws<ArgumentNullException>(() => spreadsheetService.GetByGroup(null));
        }

        [Theory, AutoSpreadsheetData]
        public void GetFullNamesForNonExistingGroupThrows(SpreadsheetService<Guid> spreadsheetService)
        {
            Assert.Throws<KeyNotFoundException>(() => spreadsheetService.GetFullNames("NonExistingGroup"));
        }

        [Theory, AutoSpreadsheetData]
        public void GetFullNamesForNullOrEmptyGroupThrows(SpreadsheetService<Guid> spreadsheetService)
        {
            Assert.Throws<ArgumentNullException>(() => spreadsheetService.GetFullNames(null, ClaimsPrincipal.Current));
            Assert.Throws<ArgumentException>(() => spreadsheetService.GetFullNames(""));
        }

        [Theory, AutoMoqData]
        public void GetCellValueReturnsNullIfEmptyMaybe(Mock<ISpreadsheetRepository<Guid>> spreadsheetRepositoryMock, Spreadsheet<Guid> spreadsheet, string sheetName, Cell cell)
        {
            spreadsheetRepositoryMock.Setup(r => r.GetCellValue(spreadsheet.Id, sheetName, cell, null)).Returns(Maybe.Empty<object>());
            var spreadsheetRepository = spreadsheetRepositoryMock.Object;
            var spreadSheetService = new SpreadsheetService<Guid>(spreadsheetRepository);

            Assert.Null(spreadSheetService.GetCellValue(spreadsheet.Id, sheetName, cell));
        }

        [Theory, AutoMoqData]
        public void GetNamedRangeReturnsNullIfEmptyMaybe(Mock<ISpreadsheetRepository<Guid>> spreadsheetRepositoryMock, Spreadsheet<Guid> spreadsheet, string sheetName, string namedRange)
        {
            spreadsheetRepositoryMock.Setup(r => r.GetNamedRange(spreadsheet.Id, sheetName, namedRange, null)).Returns(Maybe.Empty<object[,]>());
            var spreadsheetRepository = spreadsheetRepositoryMock.Object;
            var spreadSheetService = new SpreadsheetService<Guid>(spreadsheetRepository);

            Assert.Null(spreadSheetService.GetNamedRange(spreadsheet.Id, sheetName, namedRange));
        }

        [Theory, AutoMoqData]
        public void GetRangeReturnsNullIfEmptyMaybe(Mock<ISpreadsheetRepository<Guid>> spreadsheetRepositoryMock, Spreadsheet<Guid> spreadsheet, string sheetName, Range range)
        {
            spreadsheetRepositoryMock.Setup(r => r.GetRange(spreadsheet.Id, sheetName, range, null)).Returns(Maybe.Empty<object[,]>());
            var spreadsheetRepository = spreadsheetRepositoryMock.Object;
            var spreadSheetService = new SpreadsheetService<Guid>(spreadsheetRepository);

            Assert.Null(spreadSheetService.GetRange(spreadsheet.Id, sheetName, range));
        }

        [Theory, AutoMoqData]
        public void GetUsedRangeReturnsNullIfEmptyMaybe(Mock<ISpreadsheetRepository<Guid>> spreadsheetRepositoryMock, Spreadsheet<Guid> spreadsheet, string sheetName)
        {
            spreadsheetRepositoryMock.Setup(r => r.GetUsedRange(spreadsheet.Id, sheetName, null)).Returns(Maybe.Empty<object[,]>());
            var spreadsheetRepository = spreadsheetRepositoryMock.Object;
            var spreadSheetService = new SpreadsheetService<Guid>(spreadsheetRepository);

            Assert.Null(spreadSheetService.GetUsedRange(spreadsheet.Id, sheetName));
        }

        [Theory, AutoMoqData]
        public void GetUsedRangeFormatsReturnsNullIfEmptyMaybe(Mock<ISpreadsheetRepository<Guid>> spreadsheetRepositoryMock, Spreadsheet<Guid> spreadsheet, string sheetName)
        {
            spreadsheetRepositoryMock.Setup(r => r.GetUsedRangeFormats(spreadsheet.Id, sheetName, null)).Returns(Maybe.Empty<CellFormat[,]>());
            var spreadsheetRepository = spreadsheetRepositoryMock.Object;
            var spreadSheetService = new SpreadsheetService<Guid>(spreadsheetRepository);

            Assert.Null(spreadSheetService.GetUsedRangeFormats(spreadsheet.Id, sheetName));
        }

        [Theory, AutoMoqData]
        public void SheetExistsThrowsIfInvalidId(Mock<ISpreadsheetRepository<Guid>> spreadsheetRepositoryMock, Spreadsheet<Guid> spreadsheet, string sheetName)
        {
            var spreadsheetRepository = spreadsheetRepositoryMock.Object;
            spreadsheetRepositoryMock.Setup(repository => repository.Contains(spreadsheet.Id, null)).Returns(false);
            var spreadSheetService = new SpreadsheetService<Guid>(spreadsheetRepository);

            Assert.Throws<KeyNotFoundException>(() => spreadSheetService.SheetExists(spreadsheet.Id, sheetName));
            spreadsheetRepositoryMock.Verify(repository => repository.Contains(spreadsheet.Id, null), Times.Exactly(1));
        }

        [Theory, AutoSpreadsheetData]
        public void AddExistingThrows(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            spreadsheetService.Add(spreadsheet);
            Assert.Throws<ArgumentException>(() => spreadsheetService.Add(spreadsheet));
        }

        [Theory, AutoSpreadsheetData]
        public void AddStreamExistingThrows(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            spreadsheetService.Add(spreadsheet);
            Assert.Throws<ArgumentException>(() => spreadsheetService.AddStream(spreadsheet.Id, "aName", "aGroup", new MemoryStream()));
        }

        [Theory, AutoSpreadsheetData]
        public void GetIsOk(SpreadsheetService<Guid> spreadsheetService)
        {
            var spreadsheet = spreadsheetService.GetAll().ToArray()[0];
            spreadsheetService.TryGet(spreadsheet.Id, out var sc);
            Assert.Equal(spreadsheet.Id, sc.Id);
        }

        [Theory, AutoSpreadsheetData]
        public void GetByGroupIsOk(SpreadsheetService<Guid> spreadsheetService)
        {
            var group = spreadsheetService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(spreadsheetService.GetByGroup(group).Any());
        }

        [Theory, AutoSpreadsheetData]
        public void GetByGroupsIsOk(SpreadsheetService<Guid> spreadsheetService)
        {
            var group = spreadsheetService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            Assert.True(spreadsheetService.GetByGroups(new List<string> { group, group }).Any());
        }

        [Theory, AutoSpreadsheetData]
        public void GetFullNamesByGroupIsOk(SpreadsheetService<Guid> spreadsheetService)
        {
            var group = spreadsheetService.GetAll().ToArray()[0].Group;
            Assert.NotNull(group);
            Assert.NotEmpty(group);
            var fullNames = spreadsheetService.GetFullNames(group).ToList();
            Assert.True(fullNames.Any());

            var fullName = FullName.Parse(fullNames[0]);
            Assert.NotEmpty(fullName.Group);
            Assert.NotEmpty(fullName.Name);
        }

        [Theory, AutoSpreadsheetData]
        public void GetFullNamesIsOk(SpreadsheetService<Guid> spreadsheetService)
        {
            Assert.Equal(_fixture.RepeatCount, spreadsheetService.GetFullNames().Count());
        }

        [Theory, AutoSpreadsheetData]
        public void GetAllIsOk(SpreadsheetService<Guid> spreadsheetService)
        {
            Assert.Equal(_fixture.RepeatCount, spreadsheetService.GetAll().Count());
        }

        [Theory, AutoSpreadsheetData]
        public void GetIdsIsOk(SpreadsheetService<Guid> spreadsheetService)
        {
            Assert.Equal(_fixture.RepeatCount, spreadsheetService.GetIds().Count());
        }

        [Theory, AutoSpreadsheetData]
        public void AddAndGetIsOk(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            spreadsheetService.Add(spreadsheet);
            spreadsheetService.TryGet(spreadsheet.Id, out var sc);
            Assert.Equal(spreadsheet.Id, sc.Id);
        }

        [Theory, AutoSpreadsheetData]
        public void AddStreamAndGetStreamIsOk(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            spreadsheetService.AddStream(spreadsheet.Id, spreadsheet.Name, spreadsheet.Group, new MemoryStream());
            var (stream, _, _) = spreadsheetService.GetStream(spreadsheet.Id);
            Assert.IsAssignableFrom<Stream>(stream);
        }

        [Theory, AutoSpreadsheetData]
        public void CountIsOk(SpreadsheetService<Guid> spreadsheetService)
        {
            Assert.Equal(_fixture.RepeatCount, spreadsheetService.Count());
        }

        [Theory, AutoSpreadsheetData]
        public void ExistsIsOk(SpreadsheetService<Guid> spreadsheetService)
        {
            var spreadsheet = spreadsheetService.GetAll().ToArray()[0];
            Assert.True(spreadsheetService.Exists(spreadsheet.Id));
        }

        [Theory, AutoMoqData]
        public void SheetExistsIsOk(Mock<ISpreadsheetRepository<Guid>> spreadsheetRepositoryMock, Spreadsheet<Guid> spreadsheet, string sheetName)
        {
            var spreadsheetRepository = spreadsheetRepositoryMock.Object;
            spreadsheetRepositoryMock.Setup(repository => repository.Contains(spreadsheet.Id, null)).Returns(true);
            var spreadSheetService = new SpreadsheetService<Guid>(spreadsheetRepository);

            Assert.True(spreadSheetService.SheetExists(spreadsheet.Id, sheetName));
            spreadsheetRepositoryMock.Verify(repository => repository.Contains(spreadsheet.Id, null), Times.Exactly(1));
            spreadsheetRepositoryMock.Verify(repository => repository.ContainsSheet(spreadsheet.Id, sheetName, null), Times.Exactly(1));
        }

        [Theory, AutoSpreadsheetData]
        public void DoesNotExistsIsOk(SpreadsheetService<Guid> spreadsheetService)
        {
            Assert.False(spreadsheetService.Exists(Guid.NewGuid()));
        }

        [Theory, AutoSpreadsheetData]
        public void EventsAreRaisedOnAdd(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            var raisedEvents = new List<string>();
            spreadsheetService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            spreadsheetService.Added += (s, e) => { raisedEvents.Add("Added"); };

            spreadsheetService.Add(spreadsheet);

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoSpreadsheetData]
        public void EventsAreRaisedOnAddStream(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            var raisedEvents = new List<string>();
            spreadsheetService.Adding += (s, e) => { raisedEvents.Add("Adding"); };
            spreadsheetService.Added += (s, e) => { raisedEvents.Add("Added"); };

            spreadsheetService.AddStream(spreadsheet.Id, spreadsheet.Name, spreadsheet.Group, new MemoryStream());

            Assert.Equal("Adding", raisedEvents[0]);
            Assert.Equal("Added", raisedEvents[1]);
        }

        [Theory, AutoSpreadsheetData]
        public void RemoveIsOk(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            spreadsheetService.Add(spreadsheet);
            spreadsheetService.Remove(spreadsheet.Id);

            Assert.False(spreadsheetService.Exists(spreadsheet.Id));
            Assert.Equal(3, spreadsheetService.Count());
        }

        [Theory, AutoSpreadsheetData]
        public void RemoveByGroupIsOk(SpreadsheetService<Guid> spreadsheetService)
        {
            const string myGroup = "MyGroup";
            const string anotherGroup = "anotherGroup";
            var foo = new Spreadsheet<Guid>(Guid.NewGuid(), "foo", myGroup);
            var bar = new Spreadsheet<Guid>(Guid.NewGuid(), "bar", myGroup);
            var baz = new Spreadsheet<Guid>(Guid.NewGuid(), "baz", anotherGroup);
            spreadsheetService.Add(foo);
            spreadsheetService.Add(bar);
            spreadsheetService.Add(baz);
            spreadsheetService.RemoveByGroup(myGroup);

            Assert.False(spreadsheetService.Exists(foo.Id));
            Assert.False(spreadsheetService.Exists(bar.Id));
            Assert.Throws<KeyNotFoundException>(() => spreadsheetService.GetByGroup(myGroup));
            Assert.Single(spreadsheetService.GetByGroup(anotherGroup));
        }

        [Theory, AutoSpreadsheetData]
        public void EventsAreRaisedOnRemove(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            var raisedEvents = new List<string>();
            spreadsheetService.Deleting += (s, e) => { raisedEvents.Add("Deleting"); };
            spreadsheetService.Deleted += (s, e) => { raisedEvents.Add("Deleted"); };
            spreadsheetService.Add(spreadsheet);

            spreadsheetService.Remove(spreadsheet.Id);

            Assert.Equal("Deleting", raisedEvents[0]);
            Assert.Equal("Deleted", raisedEvents[1]);
        }

        [Theory, AutoSpreadsheetData]
        public void EventsAreRaisedOnRemoveByGroup(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            var raisedEvents = new List<string>();
            spreadsheetService.DeletingGroup += (s, e) => { raisedEvents.Add("DeletingGroup"); };
            spreadsheetService.DeletedGroup += (s, e) => { raisedEvents.Add("DeletedGroup"); };
            spreadsheetService.Add(spreadsheet);

            spreadsheetService.RemoveByGroup(spreadsheet.Group);

            Assert.Equal("DeletingGroup", raisedEvents[0]);
            Assert.Equal("DeletedGroup", raisedEvents[1]);
        }

        [Theory, AutoSpreadsheetData]
        public void UpdateIsOk(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            spreadsheetService.Add(spreadsheet);

            var updated = new Spreadsheet<Guid>(spreadsheet.Id, "Updated name", spreadsheet.Group);
            spreadsheetService.Update(updated);

            spreadsheetService.TryGet(spreadsheet.Id, out var sc);
            Assert.Equal("Updated name", sc.Name);
        }

        [Theory, AutoSpreadsheetData]
        public void AddOrUpdateIsOk(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            var raisedEvents = new List<string>();
            spreadsheetService.Added += (s, e) => { raisedEvents.Add("Added"); };
            spreadsheetService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            spreadsheetService.AddOrUpdate(spreadsheet);
            var updated = new Spreadsheet<Guid>(spreadsheet.Id, "Updated name", spreadsheet.Group);
            spreadsheetService.AddOrUpdate(updated);

            Assert.Equal("Added", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
            spreadsheetService.TryGet(spreadsheet.Id, out var sc);
            Assert.Equal(updated.Name, sc.Name);
        }

        [Theory, AutoSpreadsheetData]
        public void EventsAreRaisedOnUpdate(SpreadsheetService<Guid> spreadsheetService, Spreadsheet<Guid> spreadsheet)
        {
            var raisedEvents = new List<string>();
            spreadsheetService.Updating += (s, e) => { raisedEvents.Add("Updating"); };
            spreadsheetService.Updated += (s, e) => { raisedEvents.Add("Updated"); };
            spreadsheetService.Add(spreadsheet);

            var updated = new Spreadsheet<Guid>(spreadsheet.Id, "Updated name", spreadsheet.Group);
            spreadsheetService.Update(updated);

            Assert.Equal("Updating", raisedEvents[0]);
            Assert.Equal("Updated", raisedEvents[1]);
        }
    }
}