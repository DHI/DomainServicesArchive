namespace DHI.Services.Test
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Xunit;

    public sealed class FileSourceTest : IDisposable
    {
        public FileSourceTest()
        {
            _rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootPath);
            _fileSource = new FileSource(_rootPath);
        }

        private readonly string _rootPath;
        private readonly FileSource _fileSource;

        [Fact]
        public void ThrowsIfRootDirectoryIsDefinedButDoesNotExist()
        {
            var actual = Assert.Throws<ArgumentException>(() => new FileSource("NonExistingFilePath"));
            Assert.Equal("Specified root directory 'NonExistingFilePath' does not exist", actual.Message);
        }

        [Fact]
        public void CanInitializeWithoutRootDirectorySpecified()
        {
            Assert.IsType<FileSource>(new FileSource());
            Assert.IsType<FileSource>(new FileSource(string.Empty));
        }

        [Fact]
        public void CanInitializeWithRelativePath()
        {
            var path = Path.Combine(_rootPath, $"..\\{new DirectoryInfo(_rootPath).Name}");
            var fileSource = new FileSource(path);
            var filePaths = fileSource.GetFilePaths("", "txt");
            Assert.IsType<FileSource>(new FileSource(path));
        }

        [Fact]
        public void SaveIsOk()
        {
            _fileSource.Save("foo.txt", new MemoryStream(Encoding.ASCII.GetBytes("Hello World.")));
            Assert.True(File.Exists(Path.Combine(_rootPath, "foo.txt")));
        }

        [Fact]
        public void ExistsIsOk()
        {
            _fileSource.Save("foo.txt", new MemoryStream(Encoding.ASCII.GetBytes("Hello World.")));
            Assert.True(_fileSource.Exists("foo.txt"));
        }

        [Fact]
        public void GetLastWriteTimeIsOk()
        {
            _fileSource.Save("foo.txt", new MemoryStream(Encoding.ASCII.GetBytes("Hello World.")));
            Assert.True(_fileSource.GetLastWriteTime("foo.txt") < DateTime.Now);
        }

        [Fact]
        public void DeleteIsOk()
        {
            _fileSource.Save("foo.txt", new MemoryStream(Encoding.ASCII.GetBytes("Hello World.")));
            Assert.True(_fileSource.Exists("foo.txt"));
            _fileSource.Delete("foo.txt");
            Assert.False(_fileSource.Exists("foo.txt"));
        }

        [Fact]
        public void GetFilePathsIsOk()
        {
            _fileSource.Save("foo.txt", new MemoryStream(Encoding.ASCII.GetBytes("Hello World.")));
            Assert.True(_fileSource.Exists("foo.txt"));
            var filePaths = _fileSource.GetFilePaths("txt");
            Assert.Single(filePaths);
            Directory.CreateDirectory(Path.Combine(_rootPath, "sub"));
            _fileSource.Save("sub\\bar.txt", new MemoryStream(Encoding.ASCII.GetBytes("Hi World.")));
            filePaths = _fileSource.GetFilePaths("txt");
            Assert.Equal(2, filePaths.Count());
            filePaths = _fileSource.GetFilePaths("sub", "txt");
            Assert.Single(filePaths);
        }

        [Fact]
        public void GetFilePathsFromRelativePathIsOk()
        {
            var relativePath = Path.Combine(_rootPath, $"..\\{new DirectoryInfo(_rootPath).Name}");
            var fileSource = new FileSource(relativePath);
            fileSource.Save("foo.txt", new MemoryStream(Encoding.ASCII.GetBytes("Hello World.")));
            Assert.True(fileSource.Exists("foo.txt"));
            var filePaths = fileSource.GetFilePaths("txt");
            Assert.Single(filePaths);
            Directory.CreateDirectory(Path.Combine(_rootPath, "sub"));
            fileSource.Save("sub\\bar.txt", new MemoryStream(Encoding.ASCII.GetBytes("Hi World.")));
            filePaths = fileSource.GetFilePaths("txt");
            Assert.Equal(2, filePaths.Count());
            filePaths = fileSource.GetFilePaths("sub", "txt");
            Assert.Single(filePaths);
        }

        public void Dispose()
        {
            Directory.Delete(_rootPath, true);
        }
    }
}