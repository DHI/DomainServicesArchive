namespace DHI.Services.TimeSeries.Test.Repositories.CSV
{
    using System;
    using System.IO;
    using DHI.Services.TimeSeries.CSV;

    public class UpdatableTimeSeriesRepositoryFixture : IDisposable
    {
        private const string fileName = "Timeseries1.csv";

        public UpdatableTimeSeriesRepositoryFixture()
        {
            RootPath = Path.Combine(Path.GetTempPath(), "__CsvUpdatableRepository");
            Directory.CreateDirectory(RootPath);
            var filePath = Path.Combine(RootPath, fileName);
            File.Copy(@"..\..\..\Data\" + fileName, filePath, true);
            new FileInfo(filePath).IsReadOnly = false;
            Repository = new UpdatableTimeSeriesRepository(RootPath);
        }

        public UpdatableTimeSeriesRepository Repository { get; }

        public string RootPath { get; }

        public string FileName => fileName;

        public void Dispose()
        {
            Directory.Delete(RootPath, true);
        }
    }
}