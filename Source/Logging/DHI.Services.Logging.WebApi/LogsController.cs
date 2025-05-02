namespace DHI.Services.Logging.WebApi
{
    using System.Collections.Generic;
	using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Logs API
    /// </summary>
    [Produces("application/json")]
    [Route("api/logs/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for adding and querying log entries")]
    public class LogsController : ControllerBase
    {
        /// <summary>
        ///     Gets a list of log entries by query.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="queryDTO">The query body</param>
        [HttpPost("query")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<IEnumerable<LogEntryDTO>> GetByQuery(string connectionId, [FromBody] QueryDTO<LogEntryDTO> queryDTO)
        {
            var logService = Services.Get<LogService>(connectionId);

			return Ok(logService.Get(queryDTO.ToQuery()));
		}

        /// <summary>
        ///     Gets a list of log entries by query string.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<LogEntryDTO>> GetByQueryString(string connectionId)
        {
            var query = new List<QueryCondition>();
            foreach (var condition in Request.Query)
            {
                var queryCondition = new QueryCondition(condition.Key, condition.Value.ToString().ToObject());
                query.Add(queryCondition);
            }

            var logService = Services.Get<LogService>(connectionId);
			return Ok(logService.Get(query));
		}

        /// <summary>
        ///     Gets the last log entry full-filling the given query.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="queryDTO">The query body</param>
        [HttpPost("last")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<LogEntryDTO> Last(string connectionId, [FromBody] QueryDTO<LogEntryDTO> queryDTO)
        {
            var logService = Services.Get<LogService>(connectionId);
            var last = logService.Last(queryDTO.ToQuery());
            if (last is null || last.Equals(default(LogEntryDTO)))
            {
                return NotFound("No log entry found.");
            }

            return Ok(last);
        }

        /// <summary>
        ///     Adds a new log entry
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="logEntryDTO">The LogEntry.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        [Authorize(Policy = "EditorsOnly")]
        public ActionResult<LogEntryDTO> AddLogEntry(string connectionId, [FromBody] LogEntryDTO logEntryDTO)
        {
            var logService = Services.Get<LogService>(connectionId);
            logService.Add(logEntryDTO);
            return CreatedAtAction(nameof(GetByQueryString), new { connectionId, id = logEntryDTO.Id }, logEntryDTO);
        }
    }
}