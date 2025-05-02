namespace DHI.Services.Tables.WebApi
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Tables API.
    /// </summary>
    [Produces("application/json")]
    [Route("api/tables/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for retrieving table data.")]
    public class TablesController : ControllerBase
    {
        /// <summary>
        ///     Gets the table with the specified identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Table not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string connectionId, string id)
        {
            var user = HttpContext.User;
            var tableService = Services.Get<TableService>(connectionId);
            return Ok(tableService.Get(id, user));
        }

        /// <summary>
        ///     Gets a list of all table IDs.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds(string connectionId)
        {
            var user = HttpContext.User;
            var tableService = Services.Get<TableService>(connectionId);
            return Ok(tableService.GetIds(user));
        }

        /// <summary>
        ///     Gets a list of all tables.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAll(string connectionId)
        {
            var user = HttpContext.User;
            var tableService = Services.Get<TableService>(connectionId);
            return Ok(tableService.GetAll(user));
        }

        /// <summary>
        ///     Gets the total number of tables.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount(string connectionId)
        {
            var user = HttpContext.User;
            var tableService = Services.Get<TableService>(connectionId);
            return Ok(tableService.Count(user));
        }

        /// <summary>
        ///     Gets all the data from the table with the given identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        [HttpGet("{id}/data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<object[,]> GetData(string connectionId, string id)
        {
            var filter = new List<QueryCondition>();
            foreach (var condition in Request.Query)
            {
                var queryCondition = new QueryCondition(condition.Key, condition.Value.ToString().ToObject());
                filter.Add(queryCondition);
            }

            var tableService = Services.Get<TableService>(connectionId);
            return filter.Any() ? Ok(tableService.GetData(id, filter)) : Ok(tableService.GetData(id));
        }
    }
}