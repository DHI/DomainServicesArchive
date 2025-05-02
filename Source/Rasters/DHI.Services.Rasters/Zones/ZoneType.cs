namespace DHI.Services.Rasters.Zones
{
    /// <summary>
    ///     Zone type enumeration.
    /// </summary>
    public class ZoneType : Enumeration
    {
        /// <summary>
        ///     A Line.
        /// </summary>
        public static readonly ZoneType LineString = new ZoneType(0, "Line string");

        /// <summary>
        ///     A Point.
        /// </summary>
        public static readonly ZoneType Point = new ZoneType(1, "Point");

        /// <summary>
        ///     A Polygon.
        /// </summary>
        public static readonly ZoneType Polygon = new ZoneType(2, "Polygon");

        /// <summary>
        ///     Initializes a new instance of the <see cref="ZoneType" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="displayName">The display name.</param>
        protected ZoneType(int value, string displayName)
            : base(value, displayName)
        {
        }

        public ZoneType()
        {
        }
    }
}