namespace DHI.Services.Rasters.Radar.ESRIASCII
{
    /// <summary>
    ///     A repository for ESRI ASCII grids
    /// </summary>
    public class EsriAsciiRepository : BaseRadarImageRepository<AsciiImage>
    {
        /// <summary>
        /// Initializes a new instance of the EsriAsciiRepository class
        /// </summary>
        /// <param name="connectionString">The connection string in format  [folderpath];[filepattern];[datetimeformat] eg: C:\\data\\images;Radar33{{datetimeFormat}}.ascii;yyyyMMddHHmmss</param>
        public EsriAsciiRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the EsriAsciiRepository class
        /// </summary>
        /// <param name="folderPath">The folder containing the images</param>
        /// <param name="filePattern">the filename with any dae patterns marked</param>
        /// <param name="dateTimeFormat">The format of the datetime string in the filename</param>
        public EsriAsciiRepository(string folderPath, string filePattern, string dateTimeFormat) : base(folderPath, filePattern, dateTimeFormat)
        {
        }
    }
}
