namespace DHI.Services.Test
{
    using System;
    using Xunit;

    public class ConcurrentFileTest
    {
        [Fact]
        public void GetNullOrEmptyFilePathThrows()
        {
            Assert.Throws<ArgumentNullException>(() => ConcurrentFile.GetFilePath(null));
            Assert.Throws<ArgumentException>(() => ConcurrentFile.GetFilePath(""));
        }

        [Fact]
        public void GetFilePathWithoutRootThrows()
        {
            var e = Assert.Throws<ArgumentException>(() => ConcurrentFile.GetFilePath("NotARealFile.txt"));
            Assert.Contains("The given file path is not valid.", e.Message);
        }

        [Fact]
        public void GetRootThrows()
        {
            var e = Assert.Throws<ArgumentException>(() => ConcurrentFile.GetFilePath("C:\\"));
            Assert.Contains("The given file path is not valid.", e.Message);
        }

        [Fact]
        public void GetConcurrentFilePathIsOk()
        {
            Assert.Equal("..\\..\\..\\Data\\ConcurrentFile\\Concurrent 2020-02-14-12-01-01.txt", ConcurrentFile.GetFilePath("..\\..\\..\\Data\\ConcurrentFile\\Concurrent.txt"));
        }

        [Fact]
        public void GetSameFilePathIsOk()
        {
            Assert.Equal("..\\..\\..\\Data\\ConcurrentFile\\NotARealFile.txt", ConcurrentFile.GetFilePath("..\\..\\..\\Data\\ConcurrentFile\\NotARealFile.txt"));
        }

        [Fact]
        public void GetNewFilePathIsOk()
        {
            Assert.Contains("..\\..\\..\\Data\\ConcurrentFile\\NotARealFile", ConcurrentFile.GetFilePath("..\\..\\..\\Data\\ConcurrentFile\\NotARealFile.txt", true));
        }

        [Fact]
        public void GetFilePathWithoutExtensionIsOk()
        {
            Assert.Equal("C:\\NotARealFile", ConcurrentFile.GetFilePath("C:\\NotARealFile"));
        }

        [Fact]
        public void CopyDoesNotParseIsOk()
        {
            var concurrentFile = ConcurrentFile.GetFilePath("..\\..\\..\\Data\\ConcurrentFile\\Concurrent.txt");
            Assert.Equal("..\\..\\..\\Data\\ConcurrentFile\\Concurrent 2020-02-14-12-01-01.txt", concurrentFile);
        }
    }
}