namespace DHI.Services.TimeSeries.Test.Repositories.CSV
{
    using System;
    using System.IO;
    using DHI.Services.TimeSeries.CSV;

    public class TimeSeriesRepositoryFixture : IDisposable
    {
        private const string fileName = "Test.csv";
        private readonly string _rootPath;

        public TimeSeriesRepositoryFixture()
        {
            _rootPath = Path.Combine(Path.GetTempPath(), "__CsvRepository");
            var rootDir = Directory.CreateDirectory(_rootPath);
            var dir1 = rootDir.CreateSubdirectory("dir1");
            var dir11 = dir1.CreateSubdirectory("dir1.1");
            rootDir.CreateSubdirectory("dir2");
            var filepath = Path.Combine(dir11.FullName, fileName);
            File.Copy(@"..\..\..\Data\" + fileName, filepath, true);
            new FileInfo(filepath).IsReadOnly = false;

            Repository = new TimeSeriesRepository(_rootPath);
        }

        public TimeSeriesRepository Repository { get; }

        public string FileName => fileName;
        public string TempDirectory => _rootPath;

        public void Dispose()
        {
            Directory.Delete(_rootPath, true);
        }
    }
}