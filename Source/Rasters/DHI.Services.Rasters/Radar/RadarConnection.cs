namespace DHI.Services.Rasters.Radar
{
    using System;

    internal readonly struct RadarConnection
    {
        public string FolderPath { get; }

        public string FilePattern { get; }

        public string DateTimeFormat { get; }

        public static RadarConnection Parse(string connectionString)
        {
            var radarConnection = new RadarConnection(connectionString);
            return radarConnection;
        }

        public override string ToString()
        {
            return $"token={FolderPath};timeSeriesName={FilePattern}";
        }

        private RadarConnection(string connectionString) : this()
        {
            try
            {
                var items = connectionString.Split(new char[] { ';' });
                FolderPath = items[0];
                FilePattern = items[1];
                DateTimeFormat = items[2];
            }
            catch (Exception)
            {
                throw new ArgumentException($"Bad connection string format '{connectionString}'. Connection string must have syntax:  <folderpath>;<filepattern>;<datetimeformat> eg: C:\\data\\images;Radar33{{datetimeFormat}}.ascii;yyyyMMddHHmmss", nameof(connectionString));
            }
        }
    }
}