namespace DHI.Services.WebApi
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;

    [Produces("application/json")]
    [Route("api/services")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing service connections.")]
    public class ServicesController : ControllerBase
    {
        /// <summary>
        ///     Gets a list of all connection IDs.
        /// </summary>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds()
        {
            return Ok(ServiceLocator.Ids);
        }

        /// <summary>
        ///     Gets a list of all service types.
        /// </summary>
        [HttpGet("types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IDictionary<string, string>> GetTypes()
        {
            return Ok(ServiceLocator.GetTypes().ToDictionary(t => t.Key, t => t.Value.ToString()));
        }
    }
}
