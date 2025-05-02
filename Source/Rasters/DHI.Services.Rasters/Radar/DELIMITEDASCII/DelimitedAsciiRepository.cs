namespace DHI.Services.Rasters.Radar.DELIMITEDASCII
{
    /// <summary>
    ///     A repository for Delimited ASCII grids
    /// </summary>
    public class DelimitedAsciiRepository : BaseRadarImageRepository<AsciiImage>
    {
        /// <summary>
        /// Initializes a new instance of the DelimitedAsciiRepository class
        /// </summary>
        /// <param name="connectionString">The connection string in format  [folderpath];[filepattern];[datetimeformat] eg: C:\\data\\images;Radar33{{datetimeFormat}}.ascii;yyyyMMddHHmmss</param>
        public DelimitedAsciiRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DelimitedAsciiRepository class
        /// </summary>
        /// <param name="folderPath">The folder containing the images</param>
        /// <param name="filePattern">the filename with any dae patterns marked</param>
        /// <param name="dateTimeFormat">The format of the datetime string in the filename</param>
        public DelimitedAsciiRepository(string folderPath, string filePattern, string dateTimeFormat) : base(folderPath, filePattern, dateTimeFormat)
        {
        }
    }
}