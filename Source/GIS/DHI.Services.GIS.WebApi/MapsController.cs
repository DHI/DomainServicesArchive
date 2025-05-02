namespace DHI.Services.GIS.WebApi
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Runtime.Versioning;
    using System.Threading.Tasks;
    using Maps;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using SkiaSharp;
    using Spatial;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Maps API
    /// </summary>
    [Route("api/maps")]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for retrieving bitmap images.")]
    [ApiVersion("1")]
    [SupportedOSPlatform("windows")]
    public class MapsController : ControllerBase
    {
        private readonly MapStyleService _mapStyleService;

        public MapsController(IMapStyleRepository mapStyleRepository)
        {
            _mapStyleService = new MapStyleService(mapStyleRepository);
        }

        /// <summary>
        ///     Returns a bitmap image (png) using the OGC WMS standard protocol.
        /// </summary>
        /// <remarks>
        ///     Only the WMS request types "GetMap" and "GetLegendGraphic" are supported.
        ///     The individual map source providers might support other query string parameters than the below default parameters.
        ///     For details about such provider-specific parameters, see for example <a href="https://developerdoc-mike-platform-prod.eu.mike-cloud.com/domain_services/providers-reference/#dfsumapsource">DfsuMapSource</a>.
        /// </remarks>
        /// <param name="request">The request type. Only "GetMap" and "GetLegendGraphic" is supported.</param>
        /// <param name="service">The service type. Only "wms" is supported</param>
        /// <param name="version">The WMS protocol version. Only "1.3.0" is supported.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="styles">
        ///     The style (color scheme) to be used when rendering the map (or the legend).
        ///     Even if the WMS protocol allows for multiple styles, only a single style is supported.
        ///     The style is either an identifier of a style in a map style repository OR a <a href="https://developerdoc-mike-platform-prod.eu.mike-cloud.com/domain_services/faq/#what-is-a-stylecode">StyleCode</a>.
        /// </param>
        /// <param name="item">
        ///     An identifier for a data set (item) in the underlying map source
        ///     NOTE: Only applicable for a "GetMap" request.
        /// </param>
        /// <param name="layers">
        ///     An identifier (connectionId) of a configured map service.
        ///     Even if the WMS protocol allows for multiple layers, only a single layer is supported.
        ///     NOTE: Only applicable for a "GetMap" request.
        /// </param>
        /// <param name="crs">
        ///     The coordinate reference system.
        ///     NOTE: Only applicable for a "GetMap" request.
        /// </param>
        /// <param name="bbox">
        ///     The bounding box defining the map area.
        ///     NOTE: Only applicable for a "GetMap" request.
        /// </param>
        /// <param name="timestamp">
        ///     A datetime representing a time step in a time varying data source.
        ///     NOTE: Only applicable for a "GetMap" request.
        /// </param>
        /// <param name="filepath">
        ///     The full- or relative path to a data source.
        ///     NOTE: Only applicable for a "GetMap" request.
        /// </param>
        /// <param name="isVertical">
        ///     If set to true, the legend is drawn vertically, otherwise horizontally.
        ///     NOTE: Only applicable for a "GetLegendGraphic" request.
        /// </param>
        /// <response code="501">Not implemented. When request is not valid or not supported.</response>
        [HttpGet]
        public ActionResult<FileStream> WmsRequest([Required] int width, [Required] int height, [Required] string styles,
            string item, string layers, string crs, string bbox, DateTime? timestamp, string filepath,
            [Required] string request = "GetMap", string service = "wms", string version = "1.3.0",
            bool isVertical = true)
        {
            if (service.ToLower() != "wms")
            {
                throw new NotSupportedException($"WMS service '{service}' is not valid or not supported");
            }

            if (version != "1.3.0")
            {
                throw new NotSupportedException($"WMS GetMap request version '{version}' is not supported");
            }

            Guard.Against.NegativeOrZero(width, nameof(width));
            Guard.Against.NegativeOrZero(height, nameof(height));
            if (request == "GetMap")
            {
                Guard.Against.NullOrEmpty(item, nameof(item));
                Guard.Against.NullOrEmpty(layers, nameof(layers));
                Guard.Against.NullOrEmpty(crs, nameof(crs));
                Guard.Against.NullOrEmpty(bbox, nameof(bbox));
                var layer = layers.Dissemble(',').First();
                var style = styles.Dissemble(',').First();
                var boundingBox = BoundingBox.Parse(bbox);

                // Detect additional provider-specific parameters 
                var reservedParameters = new[] { "request", "service", "version", "width", "height", "styles", "item", "layers", "crs", "bbox", "isVertical", "filepath", "timestamp" };
                var parameters = Request.Query.GetAdditionalParameters(reservedParameters);

                var mapService = Services.Get<IMapService>(layer);
                using var bitmap = mapService.GetMap(style, crs, boundingBox, width, height, filepath, timestamp, item, parameters);
                var encodedData = bitmap.Encode(SKEncodedImageFormat.Png, 100);
                return File(encodedData.AsStream(true), "image/png");
            }
            else if (request == "GetLegendGraphic")
            {
                var style = styles.Dissemble(',').First();
                var user = HttpContext.User;

                if (!_mapStyleService.TryGet(style, out var mapStyle, user))
                {
                    return NotFound($"Could not find map style \"{style}\"");
                }

                using var bitmap = isVertical ? mapStyle.ToBitmapVertical(width, height) : mapStyle.ToBitmapHorizontal(width, height);
                var encodedData = bitmap.Encode(SKEncodedImageFormat.Png, 100);
                return File(encodedData.AsStream(true), "image/png");
            }
            else
            {
                throw new NotSupportedException($"WMS request type '{request}' is not valid or not supported");
            }
        }

        /// <summary>
        ///     Gets the date times.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The data source identifier.</param>
        /// <param name="from">From date time.</param>
        /// <param name="to">To date time.</param>
        /// <returns>ActionResult&lt;SortedSet&lt;DateTime&gt;&gt;.</returns>
        [Authorize]
        [HttpGet("{connectionId}/datetimes/{id}")]
        [Produces("application/json")]
        public ActionResult<SortedSet<DateTime>> GetDateTimes(string connectionId, string id, DateTime? from = null, DateTime? to = null)
        {
            var mapService = Services.Get<IMapService>(connectionId);
            var fullName = FullNameString.FromUrl(id);

            if (from is null && to is null)
            {
                return Ok(mapService.GetDateTimes(fullName));
            }

            return Ok(mapService.GetDateTimes(fullName, new DateRange(from, to)));
        }

        /// <summary>
        ///     Returns a list of base64 encoded bitmap images.
        /// </summary>
        /// <remarks>
        ///     The individual map source providers might support other query string parameters than the below default parameters.
        ///     For details about such provider-specific parameters, see for example <a href="https://developerdoc-mike-platform-prod.eu.mike-cloud.com/domain_services/providers-reference/#dfsumapsource">DfsuMapSource</a>.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="timeSteps">The dictionary of date times and corresponding file paths</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="style">
        ///     The style (color scheme) to be used when rendering the map (or the legend).
        ///     The style is either an identifier of a style in a map style repository OR a <a href="https://developerdoc-mike-platform-prod.eu.mike-cloud.com/domain_services/faq/#what-is-a-stylecode">StyleCode</a>.
        /// </param>
        /// <param name="item">An identifier for a data set (item) in the underlying map source</param>
        /// <param name="bbox">The bounding box defining the map area.</param>
        [HttpPost("{connectionId}")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<SortedDictionary<DateTime, string>> GetMaps(string connectionId, [FromBody, Required] Dictionary<DateTime, string> timeSteps,
            [Required] int width, [Required] int height, [Required] string style, [Required] string item, [Required] string bbox)
        {
            Guard.Against.NegativeOrZero(width, nameof(width));
            Guard.Against.NegativeOrZero(height, nameof(height));

            // Detect additional provider-specific parameters
            var reservedParameters = new[] { "timeSteps", "width", "height", "style", "item", "bbox" };
            var parameters = Request.Query.GetAdditionalParameters(reservedParameters);

            var boundingBox = BoundingBox.Parse(bbox);
            var mapService = Services.Get<IMapService>(connectionId);
            var images = mapService.GetMaps(style, boundingBox, new SKSizeI(width, height), timeSteps, item, parameters);

            var encodedImages = new SortedDictionary<DateTime, string>();

            foreach (var dateTime in timeSteps.Keys)
            {
                using var image = images[dateTime];
                using var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
                encodedImages.Add(dateTime, Convert.ToBase64String(encodedData.ToArray()));
            }

            return Ok(encodedImages);
        }

        /// <summary>
        ///     Gets a list of all layers within a given group.
        ///     If no group is given, all layers are returned.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is grouped (hierarchical).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        [Produces("application/json")]
        [HttpGet("{connectionId}/layers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetLayers(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var groupedMapService = Services.Get<IGroupedMapService>(connectionId);
            return group == null ? Ok(groupedMapService.GetAll()) : Ok(groupedMapService.GetByGroup(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Gets the layer with the specified fullname identifier.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is grouped (hierarchical).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The layer fullname identifier</param>
        /// <response code="200">OK</response>
        /// <response code="404">layer not found</response>
        [Produces("application/json")]
        [HttpGet("{connectionId}/layers/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Layer> GetLayer(string connectionId, string id)
        {
            var user = HttpContext.User;
            var groupedMapService = Services.Get<IGroupedMapService>(connectionId);
            var layer = groupedMapService.Get(FullNameString.FromUrl(id), user);
            return Ok(layer);
        }

        /// <summary>
        ///     Gets all layer fullname identifiers within a given group.
        ///     If no group is given, all layer fullname identifiers are returned.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is grouped (hierarchical).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        [Produces("application/json")]
        [HttpGet("{connectionId}/layers/fullnames")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetLayerFullnames(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var groupedMapService = Services.Get<IGroupedMapService>(connectionId);
            return group == null ? Ok(groupedMapService.GetFullNames(user)) : Ok(groupedMapService.GetFullNames(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Downloads the layer with the given ID as an ascii file.
        /// </summary>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="id">The layer fullname identifier</param>
        [HttpGet("{connectionId}/layers/{id}/stream/ascii")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public IActionResult GetStream(string connectionId, string id)
        {
            var user = HttpContext.User;
            var groupMapService = Services.Get<IGroupedMapService>(connectionId);
            (Stream stream, string fileType, string fileName) = groupMapService.GetStream(FullNameString.FromUrl(id), user);
            return File(stream, fileType, fileName);
        }
    }
}