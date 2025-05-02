namespace DHI.Services.Rasters.WebApi
{
    using System;
    using System.IO;

    /// <summary>
    ///     JSON file-based ZoneRepository that is located in the App_Data folder.
    /// </summary>
    /// <seealso cref="Zones.ZoneRepository" />
    public class ZoneRepository : Zones.ZoneRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZoneRepository"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public ZoneRepository(string fileName)
            : base(
                Path.Combine(
                  AppDomain.CurrentDomain.GetData("DataDirectory").ToString(),
                  fileName
                ),
                SerializerOptionsDefault
                  .Options
                  .Converters
            )
        {
        }
    }
}