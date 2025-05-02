namespace DHI.Services.Documents.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Documents;
    using Xunit;

    public class GroupedDocumentServiceTest
    {
        private const int _repeatCount = 10;

        [Fact]
        public void CreateWithNullRepositoryThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupedDocumentService(null));
        }

        [Theory, AutoDocumentData]
        public void GetNonExistingThrows(GroupedDocumentService documentService)
        {
            Assert.Throws<KeyNotFoundException>(() => documentService.Get("UnknownDocument"));
        }

        [Theory, AutoDocumentData]
        public void RemoveNonExistingThrows(GroupedDocumentService documentService)
        {
            Assert.Throws<KeyNotFoundException>(() => documentService.Remove("UnknownDocument"));
        }

        [Theory, AutoDocumentData]
        public void AddNonExistingThrows(GroupedDocumentService documentService)
        {
            Assert.Throws<FileNotFoundException>(() => documentService.Add("NonExistingFile", "myFile"));
        }

        [Theory, AutoDocumentData]
        public void GetByNonExistingGroupThrows(GroupedDocumentService documentService)
        {
            var e = Assert.Throws<KeyNotFoundException>(() => documentService.GetByGroup("nonExistingGroup"));
            Assert.Contains("does not exist", e.Message);
        }

        [Theory, AutoDocumentData]
        public void GetFullNamesForNonExistingGroupThrows(GroupedDocumentService documentService)
        {
            var e = Assert.Throws<KeyNotFoundException>(() => documentService.GetFullNames("nonExistingGroup"));
            Assert.Contains("does not exist", e.Message);
        }

        [Theory, AutoDocumentData]
        public void GetMetadataByFilterWithNullOrEmptyFilterThrows(GroupedDocumentService documentService)
        {
            Assert.Throws<ArgumentNullException>(() => documentService.GetMetadataByFilter(null));
            Assert.Throws<ArgumentException>(() => documentService.GetMetadataByFilter(""));
        }

        [Theory, AutoDocumentData]
        public void GetByGroupIsOk(GroupedDocumentService documentService, byte[] data)
        {
            var document = new MemoryStream(data);
            documentService.Add(document, "group1/document1");
            documentService.Add(document, "group1/document2");
            documentService.Add(document, "group2/document3");

            Assert.Equal(2, documentService.GetByGroup("group1").Count());
            Assert.Single(documentService.GetByGroup("group2"));
        }

        [Theory, AutoDocumentData(_repeatCount)]
        public void GetByMultipleGroupsIsOk(GroupedDocumentService documentService, byte[] data)
        {
            var document = new MemoryStream(data);
            documentService.Add(document, "group1/document1");
            documentService.Add(document, "group1/document2");
            documentService.Add(document, "group2/document3");

            Assert.Equal(3, documentService.GetByGroups(new List<string> { "group1", "group2" }).Count());
            Assert.Equal(2, documentService.GetByGroups(new List<string> { "group1" }).Count());
        }

        [Theory, AutoDocumentData(_repeatCount)]
        public void GetFullNamesIsOk(GroupedDocumentService documentService)
        {
            var fullNames = documentService.GetFullNames();
            Assert.Equal(_repeatCount, fullNames.Count());
        }

        [Theory, AutoDocumentData(_repeatCount)]
        public void GetFullNamesByGroupIsOk(GroupedDocumentService documentService, byte[] data)
        {
            var document = new MemoryStream(data);
            documentService.Add(document, "group1/document1");
            documentService.Add(document, "group1/document2");
            documentService.Add(document, "group2/document3");

            var fullNames = documentService.GetFullNames("group1");
            Assert.Equal(2, fullNames.Count());

            fullNames = documentService.GetFullNames("group2");
            Assert.Single(fullNames);
        }

        [Theory, AutoDocumentData(_repeatCount)]
        public void GetAllIsOk(GroupedDocumentService documentService)
        {
            Assert.Equal(_repeatCount, documentService.GetAll().Count());
        }

        [Theory, AutoDocumentData(_repeatCount)]
        public void GetIdsIsOk(GroupedDocumentService documentService)
        {
            Assert.Equal(_repeatCount, documentService.GetIds().Count());
        }

        [Theory, AutoDocumentData]
        public void AddAndExistsAndGetIsOk(GroupedDocumentService documentService, byte[] data)
        {
            var document = new MemoryStream(data);
            const string id = "myfolder//mydocument";
            documentService.Add(document, id);

            Assert.True(documentService.Exists(id));
            Assert.Equal(document, documentService.Get(id).stream);
        }

        [Theory, AutoDocumentData]
        public void AddAndGetMetadataIsOk(GroupedDocumentService documentService, byte[] data)
        {
            var document = new MemoryStream(data);
            const string id = "myfolder//mydocument";
            var metadata = new Parameters
            {
                { "Title", "This is a title."},
                {"Description", "This is a description."}
            };

            documentService.Add(document, id, metadata);

            Assert.Equal(metadata["Title"], documentService.GetMetadata(id)["Title"]);
            Assert.Equal(metadata["Description"], documentService.GetMetadata(id)["Description"]);
        }

        [Theory, AutoDocumentData]
        public void GetAllMetadataIsOk(GroupedDocumentService documentService, byte[] data)
        {
            var document1 = new MemoryStream(data);
            const string id1 = "myfolder//mydocument1";
            var metadata1 = new Parameters
            {
                {"Title", "Scenario 2020"},
                {"Description", "This is a description of scenario 2020"}
            };
            documentService.Add(document1, id1, metadata1);

            var document2 = new MemoryStream(data);
            const string id2 = "myfolder//mydocument2";
            var metadata2 = new Parameters
            {
                {"Title", "Basin Development 2020"},
                {"Description", "This is a description of basin development 2020"}
            };
            documentService.Add(document2, id2, metadata2);
            var result = documentService.GetAllMetadata();

            Assert.Equal(2, result.Count);
            Assert.Equal(metadata1["Title"], result[id1]["Title"]);
            Assert.Equal(metadata2["Title"], result[id2]["Title"]);
        }

        [Theory, AutoDocumentData]
        public void GetMetadataByFilterIsOk(GroupedDocumentService documentService, byte[] data)
        {
            var document1 = new MemoryStream(data);
            const string id1 = "myfolder//mydocument1";
            var metadata1 = new Parameters
            {
                {"Title", "Scenario 2020"},
                {"Description", "This is a description of scenario 2020"}
            };
            documentService.Add(document1, id1, metadata1);

            var document2 = new MemoryStream(data);
            const string id2 = "myfolder//mydocument2";
            var metadata2 = new Parameters
            {
                {"Title", "Basin Development 2020"},
                {"Description", "This is a description of basin development 2020"}
            };
            documentService.Add(document2, id2, metadata2);
            var result = documentService.GetMetadataByFilter("basin");

            Assert.Single(result);
            Assert.Equal(id2, result.Single().Key);
            Assert.Equal(2, result[id2].Count);
            Assert.Equal(metadata2["Title"], result[id2]["Title"]);
        }

        [Theory, AutoDocumentData]
        public void AddAndGetFileIsOk(GroupedDocumentService documentService)
        {
            const string id = "myFolder//HelloWorld.txt";
            documentService.Add("..\\..\\..\\Data\\HelloWorld.txt", id);

            Assert.True(documentService.Exists(id));
            var stream = (MemoryStream)documentService.Get(id).stream;
            Assert.Equal("Hello world!", Encoding.Default.GetString(stream.ToArray()));
        }

        [Theory, AutoDocumentData(_repeatCount)]
        public void CountIsOk(GroupedDocumentService documentService)
        {
            Assert.Equal(_repeatCount, documentService.Count());
        }

        [Theory, AutoDocumentData(_repeatCount)]
        public void DoesNotExistsIsOk(GroupedDocumentService documentService)
        {
            Assert.False(documentService.Exists("NonExistingDocument"));
        }

        [Theory, AutoDocumentData]
        public void EventsAreRaisedOnAdd(GroupedDocumentService documentService, byte[] data)
        {
            var raisedEvents = new List<string>();
            documentService.Putting += (s, e) => { raisedEvents.Add("Putting"); };
            documentService.WasPut += (s, e) => { raisedEvents.Add("WasPut"); };

            var document = new MemoryStream(data);
            const string id = "myfolder//mydocument";
            documentService.Add(document, id);

            Assert.Equal("Putting", raisedEvents[0]);
            Assert.Equal("WasPut", raisedEvents[1]);
        }

        [Theory, AutoDocumentData]
        public void RemoveIsOk(GroupedDocumentService documentService, byte[] data, string id)
        {
            var document = new MemoryStream(data);
            documentService.Add(document, id);

            documentService.Remove(id);

            Assert.False(documentService.Exists(id));
            Assert.Equal(0, documentService.Count());
        }

        [Theory, AutoDocumentData]
        public void EventsAreRaisedOnRemove(GroupedDocumentService documentService, byte[] data, string id)
        {
            var raisedEvents = new List<string>();
            documentService.Removing += (s, e) => { raisedEvents.Add("Removing"); };
            documentService.Removed += (s, e) => { raisedEvents.Add("Removed"); };
            var document = new MemoryStream(data);
            documentService.Add(document, id);

            documentService.Remove(id);

            Assert.Equal("Removing", raisedEvents[0]);
            Assert.Equal("Removed", raisedEvents[1]);
        }

        [Theory, AutoDocumentData]
        public void ValidationFailureThrows(GroupedDocumentService documentService)
        {
            documentService.Validators.Add(new TxtValidator());
            const string id = "HowdyWorld.txt";
            Assert.Throws<ArgumentException>(() => documentService.Add("..\\..\\..\\Data\\HowdyWorld.txt", id));
        }

        [Theory, AutoDocumentData]
        public void ValidationIsIgnoredIfPatternDoesNotMatch(GroupedDocumentService documentService)
        {
            documentService.Validators.Add(new TxtValidator("NonMatchingPattern"));
            const string id = "HowdyWorld.txt";
            documentService.Add("..\\..\\..\\Data\\HowdyWorld.txt", id);

            Assert.True(documentService.Exists(id));
        }

        [Theory, AutoDocumentData]
        public void ValidationIsOk(GroupedDocumentService documentService)
        {
            var raisedEvents = new List<string>();
            Type validatorType = null;
            documentService.Validating += (s, e) => { raisedEvents.Add("Validating"); validatorType = e.Item; };
            documentService.Validated += (s, e) => { raisedEvents.Add("Validated"); validatorType = e.Item; };
            documentService.Validators.Add(new TxtValidator());
            const string id = "myFolder//HelloWorld.txt";
            documentService.Add("..\\..\\..\\Data\\HelloWorld.txt", id);

            Assert.True(documentService.Exists(id));
            var stream = (MemoryStream)documentService.Get(id).stream;
            Assert.Contains("Hello", Encoding.Default.GetString(stream.ToArray()));
            Assert.Equal("Validating", raisedEvents[0]);
            Assert.Equal("Validated", raisedEvents[1]);
            Assert.Equal(typeof(TxtValidator), validatorType);
        }
    }
}