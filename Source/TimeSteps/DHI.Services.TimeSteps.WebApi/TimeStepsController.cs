namespace DHI.Services.TimeSteps.WebApi
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    ///     Time Steps Data API
    /// </summary>
    [Produces("application/json")]
    [Route("api/timesteps/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for retrieving time steps data.")]
    public class TimeStepsController : ControllerBase
    {
        /// <summary>
        ///     Gets the data object at the given datetime for the given item time series.
        /// </summary>
        /// <remarks>
        ///     If the time series does not contain any data, null is returned.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="itemId">The item ID.</param>
        /// <param name="date">The datetime.</param>
        [HttpGet("{itemId}/data/{date:datetime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string connectionId, string itemId, DateTime date)
        {
            var user = HttpContext.User;
            var timeStepService = Services.Get<ITimeStepService<string, object>>(connectionId);
            return Ok(timeStepService.Get(itemId, date, user));
        }

        /// <summary>
        ///     Gets a list of data objects for the given items IDs at the given datetimes.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The item IDs and selected datetimes.</param>
        [HttpPost("list")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IDictionary<string, IDictionary<DateTime, object>>> GetListByIds(string connectionId, [FromBody] IDictionary<string, IEnumerable<DateTime>> ids)
        {
            var user = HttpContext.User;
            var timeStepService = Services.Get<ITimeStepService<string, object>>(connectionId);
            return Ok(timeStepService.Get(ids, user));
        }

        /// <summary>
        ///     Gets a list of all datetimes.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("datetimes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<DateTime[]> GetDateTimes(string connectionId)
        {
            var user = HttpContext.User;
            var timeStepService = Services.Get<ITimeStepService<string, object>>(connectionId);
            return Ok(timeStepService.GetDateTimes(user));
        }

        /// <summary>
        ///     Gets the first datetime.
        /// </summary>
        /// <remarks>
        ///     If the time series does not contain any data, null is returned.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("datetime/first")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<DateTime?> GetFirstDateTime(string connectionId)
        {
            var user = HttpContext.User;
            var timeStepService = Services.Get<ITimeStepService<string, object>>(connectionId);
            return Ok(timeStepService.GetFirstDateTime(user));
        }

        /// <summary>
        ///     Gets the first data object after the given datetime for the given item time series.
        /// </summary>
        /// <remarks>
        ///     If the time series does not contain any data, null is returned.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="itemId">The item ID.</param>
        /// <param name="date">The datetime.</param>
        [HttpGet("{itemId}/data/firstafter/{date:datetime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetFirstAfter(string connectionId, string itemId, DateTime date)
        {
            var user = HttpContext.User;
            var timeStepService = Services.Get<ITimeStepService<string, object>>(connectionId);
            return Ok(timeStepService.GetFirstAfter(itemId, date, user));
        }

        /// <summary>
        ///     Gets a list of all items.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Item<string>[]> GetItems(string connectionId)
        {
            var user = HttpContext.User;
            var timeStepService = Services.Get<ITimeStepService<string, object>>(connectionId);
            return Ok(timeStepService.GetItems(user));
        }

        /// <summary>
        ///     Gets the last datetime.
        /// </summary>
        /// <remarks>
        ///     If the time series does not contain any data, null is returned.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("datetime/last")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<DateTime?> GetLastDateTime(string connectionId)
        {
            var user = HttpContext.User;
            var timeStepService = Services.Get<ITimeStepService<string, object>>(connectionId);
            return Ok(timeStepService.GetLastDateTime(user));
        }

        /// <summary>
        ///     Gets the last data object (corresponding to the last time step) for the given item.
        /// </summary>
        /// <remarks>
        ///     If the time series does not contain any data, null is returned.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="itemId">The item ID.</param>
        [HttpGet("{itemId}/data/last")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetLast(string connectionId, string itemId)
        {
            var user = HttpContext.User;
            var timeStepService = Services.Get<ITimeStepService<string, object>>(connectionId);
            return Ok(timeStepService.GetLast(itemId, user));
        }

        /// <summary>
        ///     Gets the last data object before the given datetime for the given item time series.
        /// </summary>
        /// <remarks>
        ///     If the time series does not contain any data, null is returned.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="itemId">The item ID.</param>
        /// <param name="date">The datetime.</param>
        [HttpGet("{itemId}/data/lastbefore/{date:datetime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetLastBefore(string connectionId, string itemId, DateTime date)
        {
            var user = HttpContext.User;
            var timeStepService = Services.Get<ITimeStepService<string, object>>(connectionId);
            return Ok(timeStepService.GetLastBefore(itemId, date, user));
        }
    }
}