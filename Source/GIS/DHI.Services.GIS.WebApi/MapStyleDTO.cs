namespace DHI.Services.GIS.WebApi
{
    using System.ComponentModel.DataAnnotations;
    using Maps;

    /// <summary>
    ///     Data transfer object for map style resource representation.
    /// </summary>
    public class MapStyleDTO
    {
        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the style code.
        /// </summary>
        [Required]
        public string StyleCode { get; set; }

        /// <summary>
        ///     Converts the DTO to a MapStyle object.
        /// </summary>
        public MapStyle ToMapStyle()
        {
            var mapStyle = new MapStyle(Id, Name) {StyleCode = StyleCode};
            return mapStyle;
        }
    }
}