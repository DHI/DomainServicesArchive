namespace DHI.Services.GIS.Maps
{
    using Spatial;
    using System;
    using System.Collections.Generic;
    using SkiaSharp;

    public interface IMapService
    {
        /// <summary>
        ///     Gets a bitmap image.
        /// </summary>
        /// <param name="style">
        ///     The style (color scheme) to be used when rendering the map.
        ///     The style is either an identifier of a style in a map style repository OR a <a href="https://developerdoc-mike-platform-prod.eu.mike-cloud.com/domain_services/faq/#what-is-a-stylecode">StyleCode</a>
        /// </param>
        /// <param name="crs">The coordinate reference system.</param>
        /// <param name="boundingBox">The bounding box defining the map area.</param>
        /// <param name="width">The image width in pixels.</param>
        /// <param name="height">The image height in pixels.</param>
        /// <param name="sourceId">The data source identifier.</param>
        /// <param name="dateTime">A datetime representing a time step in a time varying data source.</param>
        /// <param name="item">The identifier for a data item in the data source.</param>
        /// <param name="parameters">A collection of additional parameters.</param>
        /// <returns>A bitmap.</returns>
        SKBitmap GetMap(string style, string crs, BoundingBox boundingBox, int width, int height, string sourceId, DateTime? dateTime, string item, Parameters parameters);

        /// <summary>
        ///     Gets a collection of bitmap images.
        /// </summary>
        /// <param name="style">
        ///     The style (color scheme) to be used when rendering the map.
        ///     The style is either an identifier of a style in a map style repository OR a <a href="https://developerdoc-mike-platform-prod.eu.mike-cloud.com/domain_services/faq/#what-is-a-stylecode">StyleCode</a>
        /// </param>
        /// <param name="boundingBox">The bounding box defining the map area.</param>
        /// <param name="size">The image size in pixels.</param>
        /// <param name="timeSteps">A dictionary of date times and corresponding data source identifiers.</param>
        /// <param name="item">The identifier for a data item in the data source.</param>
        /// <param name="parameters">A collection of additional parameters.</param>
        /// <returns>A collection of bitmap images.</returns>
        SortedDictionary<DateTime, SKBitmap> GetMaps(string style, BoundingBox boundingBox, SKSizeI size, Dictionary<DateTime, string> timeSteps, string item, Parameters parameters);

        /// <summary>
        ///     Gets the available date times for the data source with the given identifier.
        /// </summary>
        /// <param name="id">The data source identifier.</param>
        /// <returns>A time axis.</returns>
        SortedSet<DateTime> GetDateTimes(string id);

        /// <summary>
        ///     Gets the available date times, within the given date range, for the data source with the given identifier.
        /// </summary>
        /// <param name="id">The data source identifier.</param>
        /// <param name="dateRange">The date range.</param>
        /// <returns>A time axis.</returns>
        SortedSet<DateTime> GetDateTimes(string id, DateRange dateRange);
    }
}