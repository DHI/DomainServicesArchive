using DHI.Services;
using DHI.Services.WebApiCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DHI.Services.Notifications.WebApi
{
    /// <summary>
    ///     Notifications API
    /// </summary>
    [Produces("application/json")]
    [Route("api/notifications/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for adding and querying notification entries")]
    public class NotificationsController : ControllerBase
    {
        /// <summary>
        ///     Gets a list of notification entries by query.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="queryDTO">The query body</param>
        [HttpPost("query")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<IEnumerable<NotificationEntry>> GetByQuery(string connectionId, [FromBody] QueryDTO<NotificationEntryDTO> queryDTO)
        {
            var notificationService = Services.Get<NotificationService>(connectionId);
            return Ok(notificationService.Get(queryDTO.ToQuery()));
        }

        /// <summary>
        ///     Gets a list of notification entries by query string.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<NotificationEntry>> GetByQueryString(string connectionId)
        {
            var query = new List<QueryCondition>();
            foreach (var condition in Request.Query)
            {
                var queryCondition = new QueryCondition(condition.Key, condition.Value.ToString().ToObject());
                query.Add(queryCondition);
            }

            var notificationService = Services.Get<NotificationService>(connectionId);
            return Ok(notificationService.Get(query));
        }

        /// <summary>
        ///     Gets the last notification entry full-filling the given query.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="queryDTO">The query body</param>
        [HttpPost("last")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<NotificationEntry> Last(string connectionId, [FromBody] QueryDTO<NotificationEntryDTO> queryDTO)
        {
            var notificationService = Services.Get<NotificationService>(connectionId);
            var last = notificationService.Last(queryDTO.ToQuery());
            if (last.Equals(default(NotificationEntry)))
            {
                return NotFound("No notification entry found.");
            }

            return Ok(last);
        }

        /// <summary>
        ///     Adds a new notification entry
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="dto"></param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        [Authorize(Policy = "EditorsOnly")]
        public ActionResult<NotificationEntry> AddNotificationEntry(string connectionId, [FromBody] NotificationEntryDTO dto)
        {
            var newEntry = new NotificationEntry(
                dto.NotificationLevel,
                dto.Text,
                dto.Source,
                dto.Tag,
                dto.MachineName,
                DateTime.UtcNow,
                dto.Metadata
            );

            var notificationService = Services.Get<NotificationService>(connectionId);
            notificationService.Add(newEntry);

            return CreatedAtAction(nameof(GetByQueryString), new { connectionId, id = newEntry.Id }, newEntry);
        }
    }
}
