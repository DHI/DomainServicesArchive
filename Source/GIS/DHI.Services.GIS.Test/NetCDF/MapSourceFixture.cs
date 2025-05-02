namespace DHI.Services.GIS.Test.NetCDF
{
    using System;
    using System.IO;
    using GIS.NetCDF;

    public class MapSourceFixture : IDisposable
    {
        private readonly string _folderPath;

        public MapSourceFixture()
        {
            var tempPath = Path.GetTempPath();
            _folderPath = Path.Combine(tempPath, "ds-test-repository" + Guid.NewGuid());
            Directory.CreateDirectory(_folderPath);
            var filePath = Path.Combine(_folderPath, FileName);
            File.Copy(@"..\..\..\Data\" + FileName, filePath, true);
            new FileInfo(filePath).IsReadOnly = false;
            MapSource = new MapSource(filePath, new Parameters());

            const string imageFileName = "TRMM_2000.png";
            ImageFilePath = Path.Combine(Path.GetTempPath(), imageFileName);
            File.Copy(@"..\..\..\Data\" + imageFileName, ImageFilePath, true);
            new FileInfo(ImageFilePath).IsReadOnly = false;
        }

        public string FileName => "TRMM_2000.nc";

        public MapSource MapSource { get; }

        public string ImageFilePath { get; }

        public string StyleCode => "0~5:#800080,#5500AB,#2A00D5,#0000FF,#0038E1,#006FC3,#00A6A6,#00C46E,#00E237,#00FF00,#55FF00,#AAFF00,#FFFF00,#FFAA00,#FF5500,#FF0000";

        public void Dispose()
        {
            if (Directory.Exists(_folderPath))
            {
                Directory.Delete(_folderPath, true);
            }
        }
    }
}