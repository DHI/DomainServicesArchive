namespace DHI.Services.GIS.WebApi
{
    using System.Collections.Generic;
    using Maps;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    ///     Map Styles API
    /// </summary>
    [Produces("application/json")]
    [Route("api/mapstyles")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing predefined map styles. These map styles are used in the Maps API")]
    public class MapStylesController : ControllerBase
    {
        private readonly MapStyleService _mapStyleService;

        public MapStylesController(IMapStyleRepository mapStyleRepository)
        {
            _mapStyleService = new MapStyleService(mapStyleRepository);
        }

        /// <summary>
        ///     Deletes the map style with the specified identifier.
        /// </summary>
        /// <param name="id">The map style identifier.</param>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Map style not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "EditorsOnly")]
        public IActionResult Delete(string id)
        {
            var user = HttpContext.User;
            _mapStyleService.Remove(id, user);
            return NoContent();
        }

        /// <summary>
        ///     Gets the map style with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Map style not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<MapStyle> Get(string id)
        {
            var user = HttpContext.User;
            if (_mapStyleService.TryGet(id, out var style, user))
            {
                return Ok(style);
            }

            return NotFound($"Could not find map style with id \"{id}\"");
        }

        /// <summary>
        ///     Gets the palette for the map style with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Map style not found</response>
        [HttpGet("{id}/palette")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Dictionary<double, MapStyleBand>> GetPalette(string id)
        {
            var user = HttpContext.User;

            if (_mapStyleService.TryGet(id, out var style, user))
            {
                return Ok(style.GetPalette());
            }

            return NotFound($"Could not find palette with id \"{id}\"");
        }

        /// <summary>
        ///     Gets a list of all map styles.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<MapStyle>> GetAll()
        {
            var user = HttpContext.User;
            return Ok(_mapStyleService.GetAll(user));
        }

        /// <summary>
        ///     Gets the total number of map styles.
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount()
        {
            var user = HttpContext.User;
            return Ok(_mapStyleService.Count(user));
        }

        /// <summary>
        ///     Adds a new map style.
        /// </summary>
        /// <param name="mapStyleDto">The map style dto.</param>
        /// <response code="201">Created</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        [Authorize(Policy = "EditorsOnly")]
        public ActionResult<MapStyle> Add([FromBody] MapStyleDTO mapStyleDto)
        {
            var user = HttpContext.User;
            var mapStyle = mapStyleDto.ToMapStyle();
            _mapStyleService.Add(mapStyle, user);

            if (_mapStyleService.TryGet(mapStyle.Id, out _, user))
            {
                return CreatedAtAction(nameof(Get), new { id = mapStyle.Id }, mapStyle);
            }

            return BadRequest($"Could not successfully add map style \"{mapStyle.Id}\"");
        }
    }
}