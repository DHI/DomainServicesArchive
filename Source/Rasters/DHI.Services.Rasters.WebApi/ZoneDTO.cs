namespace DHI.Services.Rasters.WebApi
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Drawing;
    using Zones;

    /// <summary>
    ///     Data transfer object for a zone resource representation.
    /// </summary>
    public class ZoneDTO
    {
        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        [Required]
        [Key]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the type.
        /// </summary>
        [Required]
        public string Type { get; set; }

        /// <summary>
        ///     Gets or sets the size of the image.
        /// </summary>
        [Required]
        public Size ImageSize { get; set; }

        /// <summary>
        ///     Gets or sets the pixel weights.
        /// </summary>
        [Required]
        public HashSet<PixelWeight> PixelWeights { get; set; }

        /// <summary>
        ///     Converts the DTO to a Zone object.
        /// </summary>
        public Zone ToZone()
        {
            var zoneType = Enumeration.FromDisplayName<ZoneType>(Type);
            var zone = new Zone(Id, Name, zoneType) {ImageSize = ImageSize};
            foreach (var pixelWeight in PixelWeights)
            {
                zone.PixelWeights.Add(pixelWeight);
            }

            return zone;
        }
    }
}