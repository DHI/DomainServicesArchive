namespace DHI.Services.GIS.Maps
{
    using System.Text.Json.Serialization;
    using Spatial;

    /// <summary>
    ///     Class representing a map layer.
    /// </summary>
    public class Layer : BaseGroupedEntity<string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Layer" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="group">The group.</param>
        [JsonConstructor]
        public Layer(string id, string name, string group)
            : base(id, name, group)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Layer" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public Layer(string id, string name)
            : base(id, name, null)
        {
        }

        /// <summary>
        ///     Gets the bounding box.
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        ///     Gets the coordinate system.
        /// </summary>
        public string CoordinateSystem { get; set; }
    }
}