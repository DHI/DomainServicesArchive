namespace DHI.Services.Documents.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Xunit;

    public class FileDocumentRepositoryTest : IDisposable
    {
        private readonly string _testRootDir;
        private readonly FileDocumentRepository _repository;

        public FileDocumentRepositoryTest()
        {
            _testRootDir = Path.Combine(Path.GetTempPath(), "FileDocRepoTests_" + Guid.NewGuid());
            _repository = new FileDocumentRepository(_testRootDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testRootDir))
                Directory.Delete(_testRootDir, true);
        }

        [Fact]
        public void AddAndGetFileIsOk()
        {
            var id = "testFolder/HelloWorld.txt";
            var stream = File.OpenRead("../../../Data/HelloWorld.txt");

            _repository.Add(stream, id, new Parameters());

            Assert.True(_repository.Contains(id));
            var (retrievedStream, fileType, fileName) = _repository.Get(id);

            using var reader = new StreamReader(retrievedStream);
            var content = reader.ReadToEnd();
            Assert.Equal("Hello world!", content.Trim());
            Assert.Equal("txt", fileType);
            Assert.Equal("HelloWorld.txt", fileName);
        }

        [Fact]
        public void AddAndGetMetadataIsOk()
        {
            var id = "folder/Doc1.txt";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Some content"));
            var metadata = new Parameters
            {
                { "Title", "A Document" },
                { "Description", "Test Description" }
            };

            _repository.Add(stream, id, metadata);

            var actual = _repository.GetMetadata(id);
            Assert.Equal("A Document", actual["Title"]);
            Assert.Equal("Test Description", actual["Description"]);
        }

        [Fact]
        public void GetMetadataByFilterIsOk()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Some content"));

            _repository.Add(stream, "group/doc1.txt", new Parameters { { "Title", "Basin Plan" } });
            _repository.Add(stream, "group/doc2.txt", new Parameters { { "Title", "Not Relevant" } });

            var result = _repository.GetMetadataByFilter("basin");

            Assert.Single(result);
            Assert.Contains("group/doc1.txt", result.Keys);
        }

        [Fact]
        public void RemoveIsOk()
        {
            var id = "folder/Removable.txt";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Delete me"));
            _repository.Add(stream, id, new Parameters());

            Assert.True(_repository.Contains(id));
            _repository.Remove(id);
            Assert.False(_repository.Contains(id));
        }

        [Fact]
        public void GetAllAndGetIdsAndCountIsOk()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Content"));

            _repository.Add(stream, "folder/one.txt", new Parameters());
            _repository.Add(stream, "folder/two.txt", new Parameters());

            Assert.Equal(2, _repository.Count());
            Assert.Equal(2, _repository.GetIds().Count());
            Assert.Equal(2, _repository.GetAll().Count());
        }

        [Fact]
        public void GetByGroupAndContainsGroupIsOk()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Content"));

            _repository.Add(stream, "groupA/doc1.txt", new Parameters());
            _repository.Add(stream, "groupA/doc2.txt", new Parameters());
            _repository.Add(stream, "groupB/doc3.txt", new Parameters());

            Assert.True(_repository.ContainsGroup("groupA"));
            Assert.Equal(2, _repository.GetByGroup("groupA").Count());
            Assert.Single(_repository.GetByGroup("groupB"));
        }
    }
}
