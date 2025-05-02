namespace DHI.Services.GIS.WebApi
{
    using System.Text.Json;

    /// <summary>
    ///     JSON file-based MapStyleRepository that is located in the App_Data folder.
    /// </summary>
    /// <seealso cref="Maps.MapStyleRepository" />
    public class MapStyleRepository : Maps.MapStyleRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MapStyleRepository" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public MapStyleRepository(string fileName)
            : base(fileName)
        {
        }


        public MapStyleRepository(string fileName, JsonSerializerOptions serializerOptions, JsonSerializerOptions deserializerOptions = null)
            : base(fileName, serializerOptions, deserializerOptions)
        {
        }
    }
}