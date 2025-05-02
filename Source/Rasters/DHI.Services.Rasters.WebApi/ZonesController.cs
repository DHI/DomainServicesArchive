namespace DHI.Services.Rasters.WebApi
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;
    using Zones;

    /// <summary>
    ///     Zones API
    /// </summary>
    [Produces("application/json")]
    [Route("api/zones")]
    [Authorize()]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing zones.")]
    public class ZonesController : ControllerBase
    {
        private readonly ZoneService _zoneService;

        public ZonesController(IZoneRepository zoneRepository)
        {
            _zoneService = new ZoneService(zoneRepository);
        }

        /// <summary>
        ///     Gets the zone with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Zone not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Zone> Get(string id)
        {
            var user = HttpContext.User;
            return Ok(_zoneService.Get(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets a list of all zones.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Zone>> GetAll()
        {
            var user = HttpContext.User;
            return Ok(_zoneService.GetAll(user));
        }

        /// <summary>
        ///     Gets the total number of zones.
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount()
        {
            var user = HttpContext.User;
            return Ok(_zoneService.Count(user));
        }

        /// <summary>
        ///     Gets a list of all zone IDs.
        /// </summary>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds()
        {
            var user = HttpContext.User;
            return Ok(_zoneService.GetIds(user));
        }

        /// <summary>
        ///     Creates a new zone.
        /// </summary>
        /// <param name="zoneDto">The zone dto.</param>
        /// <response code="201">Created</response>
        [Authorize(Policy = "AdministratorsOnly")]
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public ActionResult<Zone> Add([FromBody] ZoneDTO zoneDto)
        {
            var user = HttpContext.User;
            var zone = zoneDto.ToZone();
            _zoneService.Add(zone, user);
            return CreatedAtAction(nameof(Get), new { id = zone.Id }, zone);
        }

        /// <summary>
        ///     Removes the zone with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Zone not found</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdministratorsOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string id)
        {
            var user = HttpContext.User;
            _zoneService.Remove(id, user);
            return NoContent();
        }
    }
}