namespace DHI.Services.Places.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using GIS.Maps;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Places API
    /// </summary>
    [Produces("application/json")]
    [Route("api/places/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing places.")]
    public class PlacesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PlacesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        ///     Adds a new place.
        /// </summary>
        /// <response code="201">Created</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="placeDTO">The place body</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public ActionResult<PlaceDTO> Add(string connectionId, [FromBody] PlaceDTO placeDTO)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);

            var place = placeDTO.ToPlace();
            placeService.Add(place, user);
            var id = FullNameString.ToUrl(place.FullName);
            return CreatedAtAction(nameof(Get), new { connectionId, id }, place.ToDTO());
        }

        /// <summary>
        ///     Deletes the place with the specified identifier.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Place not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="id">The place ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string connectionId, string id)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            placeService.Remove(FullNameString.FromUrl(id), user);
            return NoContent();
        }

        /// <summary>
        ///     Adds an indicator to the specified place.
        /// </summary>
        /// <response code="201">Indicator created.</response>
        /// <response code="404">Place not found.</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The place identifier.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="indicatorDTO">The indicator body.</param>
        [HttpPost("{id}/indicator/{type}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public ActionResult<IndicatorDTO> AddIndicator(string connectionId, string id, string type, [FromBody] IndicatorDTO indicatorDTO)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            placeService.AddIndicator(FullNameString.FromUrl(id), type, indicatorDTO.ToIndicator(), user);
            var urlId = FullNameString.ToUrl(id);
            return CreatedAtAction(nameof(GetIndicator), new { connectionId, id = urlId, type }, indicatorDTO);
        }

        /// <summary>
        ///     Updates an indicator at the specified place.
        /// </summary>
        /// <response code="200">OK.</response>
        /// <response code="404">Place not found.</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The place identifier.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="indicatorDTO">The indicator body.</param>
        [HttpPut("{id}/indicator/{type}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<IndicatorDTO> UpdateIndicator(string connectionId, string id, string type, [FromBody] IndicatorDTO indicatorDTO)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            placeService.UpdateIndicator(FullNameString.FromUrl(id), type, indicatorDTO.ToIndicator(), user);
            return Ok(placeService.GetIndicator(FullNameString.FromUrl(id), type, user).ToDTO());
        }

        /// <summary>
        ///     Deletes an indicator at the specified place.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Place not found</response>
        /// <response code="404">Indicator not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="id">The place ID.</param>
        /// <param name="type">The indicator type.</param>
        [HttpDelete("{id}/indicator/{type}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteIndicator(string connectionId, string id, string type)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            placeService.RemoveIndicator(FullNameString.FromUrl(id), type, user);
            return NoContent();
        }

        /// <summary>
        ///     Gets places.
        /// </summary>
        /// <remarks>
        ///     If no group is given, a list of all places is retrieved.
        ///     If a group is given, a list of places within the specified group (recursive) is retrieved.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<PlaceDTO>> GetAll(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            var places = group is null ? placeService.GetAll(user) : placeService.GetByGroup(group, user);
            return Ok(places.ToDTOs());
        }

        /// <summary>
        ///     Gets GIS features of places.
        /// </summary>
        /// <remarks>
        ///     If no group is given, features for all places is retrieved.
        ///     If a group is given, only features for places within the specified group (recursive) is retrieved.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        /// <param name="from">From date for indicator status period (overwrites time interval settings on indicators).</param>
        /// <param name="to">To date for indicator status period (overwrites time interval on indicators).</param>
        /// <param name="dateTime">The dateTime if time interval type is RelativeToDateTime</param>
        /// <param name="path">A path to inject into the entity ID</param>
        /// <param name="includeIndicatorStatus">Whether to return indicator status together with features.</param>
        [HttpGet("features")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IList<IFactory>> GetFeatures(
            string connectionId,
            string group = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] DateTime? dateTime = null,
            [FromQuery] string path = null,
            [FromQuery] bool includeIndicatorStatus = false)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            if (includeIndicatorStatus && (from.HasValue || to.HasValue))
            {
                return Ok(placeService.GetFeaturesWithIndicatorStatus(from ?? DateTime.MinValue, to ?? DateTime.MaxValue, group, path, user).Features);
            }

            if (includeIndicatorStatus)
            {
                return Ok(placeService.GetFeaturesWithIndicatorStatus(group, dateTime, path, user).Features);
            }

            return Ok(placeService.GetFeatures(group, user).Features);
        }

        /// <summary>
        ///     Gets a list of place fullname identifiers.
        /// </summary>
        /// <remarks>
        ///     If a group is given, a list of fullname identifiers within the given group (recursive) is retrieved.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        [HttpGet("fullnames")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetFullNames(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            return group == null ? Ok(placeService.GetFullNames(user)) : Ok(placeService.GetFullNames(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Gets the specified place.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Place not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The place identifier.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<PlaceDTO> Get(string connectionId, string id)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);

            var fullName = FullNameString.FromUrl(id);
            if (placeService.Exists(fullName, user))
            {
                var placeDTO = placeService.Get(fullName, user).ToDTO();
                return Ok(placeDTO);
            }

            return NotFound();
        }

        /// <summary>
        ///     Gets all indicators from the specified place.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Place not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The place identifier.</param>
        [HttpGet("{id}/indicators")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IDictionary<string, IndicatorDTO>> GetIndicatorsByPlace(string connectionId, string id)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            return Ok(placeService.GetIndicatorsByPlace(FullNameString.FromUrl(id), user).ToDTOs());
        }

        /// <summary>
        ///     Gets all indicators of the specified type.
        /// </summary>
        /// <remarks>
        ///     If a group is specified, only the indicators of places within this group is returned.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="group">Group of places.</param>
        [HttpGet("indicators/{type}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IDictionary<string, IndicatorDTO>> GetIndicatorsByType(string connectionId, string type, [FromQuery] string group = null)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            var indicators = group is null ? placeService.GetIndicatorsByType(type, user) : placeService.GetIndicatorsByGroupAndType(group, type);
            return Ok(indicators.ToDTOs());
        }

        /// <summary>
        ///     Gets the specified type of indicator from the specified place.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Place not found.</response>
        /// <response code="404">Indicator of this type was not found.</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The place identifier.</param>
        /// <param name="type">The indicator type.</param>
        [HttpGet("{id}/indicators/{type}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IndicatorDTO> GetIndicator(string connectionId, string id, string type)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            return Ok(placeService.GetIndicator(FullNameString.FromUrl(id), type, user).ToDTO());
        }

        /// <summary>
        ///     Gets the threshold values for all indicators from the specified place.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Place not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The place identifier.</param>
        [HttpGet("{id}/thresholds")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IDictionary<string, IEnumerator<double>>> GetThresholdValuesByPlace(string connectionId, string id)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            return Ok(placeService.GetThresholdValues(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets the threshold values for the specified indicator type from the specified place.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Place not found.</response>
        /// <response code="404">Indicator not found.</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The place identifier.</param>
        /// <param name="indicatorType">The indicator type.</param>
        [HttpGet("{id}/thresholds/{indicatorType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerator<double>> GetThresholdValues(string connectionId, string id, string indicatorType)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            return Ok(placeService.GetThresholdValues(FullNameString.FromUrl(id), indicatorType, user));
        }

        /// <summary>
        ///     Gets the current status (color) of the specified indicator at the specified place.
        /// </summary>
        /// <remarks>
        ///    If no calculation can be performed because of missing or corrupt values, a default color is returned. 
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="404">Place not found.</response>
        /// <response code="404">Indicator of this type not found.</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The place identifier.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="dateTime">The dateTime if time interval type is RelativeToDateTime</param>
        /// <param name="path">A path to inject into the entity ID</param>
        [HttpGet("{id}/indicators/{type}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<IndicatorStatusDTO> GetIndicatorStatus(string connectionId, string id, string type,
            [FromQuery] DateTime? dateTime = null,
            [FromQuery] string path = null)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            var indicator = placeService.GetIndicator(FullNameString.FromUrl(id), type, user);
            var maybe = placeService.GetIndicatorStatus(indicator, dateTime, path);
            if (maybe.HasValue)
            {
                return Ok(new IndicatorStatusDTO(indicator, maybe.Value));
            }

            return NoContent();
        }

        /// <summary>
        ///     Gets the current status (color) of all indicators of the specified type.
        /// </summary>
        /// <remarks>
        ///     If a group is specified, only the indicators of places within this group is processed.
        ///     If no calculation can be performed because of missing or corrupt values, a default color is returned.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="group">Group of places.</param>
        /// <param name="dateTime">The dateTime if time interval type is RelativeToDateTime</param>
        /// <param name="path">A path to inject into the entity ID</param>
        [HttpGet("indicators/{type}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IDictionary<string, IndicatorStatusDTO>> GetIndicatorStatusByType(string connectionId, string type,
            [FromQuery] string group = null,
            [FromQuery] DateTime? dateTime = null,
            [FromQuery] string path = null)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            var dictionary = new Dictionary<string, IndicatorStatusDTO>();
            var indicators = group is null ? placeService.GetIndicatorsByType(type, user) : placeService.GetIndicatorsByGroupAndType(group, type);
            foreach (var indicator in indicators)
            {
                var maybe = placeService.GetIndicatorStatus(indicator.Value, dateTime, path);
                if (maybe.HasValue)
                {
                    dictionary.Add(indicator.Key, new IndicatorStatusDTO(indicator.Value, maybe.Value));
                }
            }

            return Ok(dictionary);
        }

        /// <summary>
        ///     Gets a palette image (png) of the specified indicator at the specified place.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The place identifier.</param>
        /// <param name="type">The indicator type.</param>
        /// <param name="width">The image width.</param>
        /// <param name="height">The image height.</param>
        /// <param name="numberOfDecimals">The number of decimals on threshold values.</param>
        /// <param name="horizontal">true if horizontal palette image</param>
        /// <returns>IActionResult.</returns>
        [HttpGet("{id}/indicators/{type}/palette")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPalette(string connectionId, string id, string type,
            [FromQuery] int width, [FromQuery] int height, [FromQuery] int numberOfDecimals = 1, [FromQuery] bool horizontal = false)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<PlaceService>(connectionId);
            var indicator = placeService.GetIndicator(FullNameString.FromUrl(id), type, user);
            var palette = new Palette(indicator.StyleCode, numberOfDecimals);
            var bitmap = horizontal ? palette.ToBitmapHorizontal(width, height) : palette.ToBitmapVertical(width, height);
            var png = bitmap.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            return File(png.AsStream(true), "image/png");
        }
    }
}
