namespace DHI.Services.JsonDocuments.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Authorization;
    using Filters;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     JSON Documents API
    /// </summary>
    [Produces("application/json")]
    [Route("api/jsondocuments/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing JSON documents.")]
    public class JsonDocumentsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly FilterService _filterService;

        public JsonDocumentsController(IHubContext<NotificationHub> hubContext, IFilterRepository filterRepository, ILogger logger)
        {
            _logger = logger;
            _hubContext = hubContext;
            _filterService = new FilterService(filterRepository);
        }

        /// <summary>
        ///     Gets all json documents within the given time interval.
        ///     If no time interval is given, all json documents are returned.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="from">Interval start.</param>
        /// <param name="to">Interval end.</param>
        /// <param name="dataSelectors">The selectors for filtering the data. E.g: dataSelectors=[foo,bar].</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<JsonDocumentDTO>> GetWithinTimeInterval(string connectionId, DateTime? from = null, DateTime? to = null, string dataSelectors = null)
        {
            var user = HttpContext.User;
            var jsonDocumentService = Services.Get<JsonDocumentService<string>>(connectionId);
            if (from is null && to is null)
            {
                return Ok(jsonDocumentService.GetAll(ParseDataSelectors(dataSelectors), user).ToDTOs());
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(jsonDocumentService.Get(fromDateTime, toDateTime, ParseDataSelectors(dataSelectors), user).ToDTOs());
        }

        /// <summary>
        ///     Gets a list of json documents by query.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="queryDTO">The query body.</param>
        /// <param name="dataSelectors">The selectors for filtering the data. E.g: dataSelectors=[foo,bar].</param>
        [HttpPost("query")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<IEnumerable<JsonDocumentDTO>> GetByQuery(string connectionId, [FromBody] QueryDTO<JsonDocument<string>> queryDTO, [FromQuery] string dataSelectors = null)
        {
            var user = HttpContext.User;
            var jsonDocumentService = Services.Get<JsonDocumentService<string>>(connectionId);
            return Ok(jsonDocumentService.Get(queryDTO.ToQuery(), ParseDataSelectors(dataSelectors), user).ToDTOs());
        }

        /// <summary>
        ///     Gets the JSON document with the specified fullname identifier.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">JSON document not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="fullName">The JSON document fullname identifier.</param>
        /// <param name="dataSelectors">The selectors for filtering the data. E.g: dataSelectors=[foo,bar].</param>
        [HttpGet("{fullName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<JsonDocumentDTO> Get(string connectionId, string fullName, [FromQuery] string dataSelectors = null)
        {
            var user = HttpContext.User;
            var jsonDocumentService = Services.Get<JsonDocumentService<string>>(connectionId);
            if (dataSelectors is null)
            {
                if (jsonDocumentService.TryGet(FullNameString.FromUrl(fullName), out var entity, user))
                {
                    return Ok(entity.ToDTO());
                }

                return NotFound();
            }

            return Ok(jsonDocumentService.Get(FullNameString.FromUrl(fullName), ParseDataSelectors(dataSelectors), user).ToDTO());
        }

        /// <summary>
        ///     Gets a list of JSON documents within the given group.
        /// </summary>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="group">The group.</param>
        /// <param name="dataSelectors">The selectors for filtering the data. E.g: dataSelectors=[foo,bar].</param>
        [HttpGet("group/{group}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<JsonDocumentDTO>> GetByGroup(string connectionId, string group, string dataSelectors = null)
        {
            var user = HttpContext.User;
            var jsonDocumentService = Services.Get<JsonDocumentService<string>>(connectionId);
            if (dataSelectors is null)
            {
                return Ok(jsonDocumentService.GetByGroup(FullNameString.FromUrl(group), user).ToDTOs());
            }

            return Ok(jsonDocumentService.GetByGroup(FullNameString.FromUrl(group), ParseDataSelectors(dataSelectors), user).ToDTOs());
        }

        /// <summary>
        ///     Gets the total number of JSON documents.
        /// </summary>
        /// <param name="connectionId">The connection identifier</param>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount(string connectionId)
        {
            var user = HttpContext.User;
            var jsonDocumentService = Services.Get<JsonDocumentService<string>>(connectionId);
            return Ok(jsonDocumentService.Count(user));
        }

        /// <summary>
        ///     Gets a list of JSON document fullname identifiers.
        /// </summary>
        /// <remarks>
        ///     If a group is given, a list of time series fullname identifiers within the given group (recursive) is retrieved.
        /// </remarks>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="group">The group.</param>
        [HttpGet("fullnames")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetFullNames(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var jsonDocumentService = Services.Get<JsonDocumentService<string>>(connectionId);
            return group == null ? Ok(jsonDocumentService.GetFullNames(user)) : Ok(jsonDocumentService.GetFullNames(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Adds a new JSON document.
        /// </summary>
        /// <response code="201">Created</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="jsonDocumentDTO">The JSON document body</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public async Task<ActionResult<JsonDocumentDTO>> Add(string connectionId, [FromBody] JsonDocumentDTO jsonDocumentDTO)
        {
            var user = HttpContext.User;
            var jsonDocumentService = Services.Get<JsonDocumentService<string>>(connectionId);
            var jsonDocument = jsonDocumentDTO.ToJsonDocument();
            jsonDocumentService.Add(jsonDocument, user);
            var groups = await GetGroups(user, jsonDocument.Id, connectionId, jsonDocumentService);
            var jsonDocumentId = FullNameString.ToUrl(jsonDocument.FullName);
            var parameters = new Parameters
            {
                { "id", jsonDocument.Id },
                { "data", jsonDocument.Data },
                { "userName", user.GetUserId() }
            };

            await SendMessages(groups, "JsonDocumentAdded", parameters);
            if (jsonDocumentService.TryGet(jsonDocument.FullName, out var entity, user))
            {
                return CreatedAtAction(nameof(Get), new { connectionId, fullName = jsonDocumentId }, entity.ToDTO());
            }

            return NotFound();
        }

        /// <summary>
        ///     Updates an existing JSON document.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">JSON document not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="jsonDocumentDTO">The JSON document body.</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public async Task<ActionResult<JsonDocumentDTO>> Update(string connectionId, [FromBody] JsonDocumentDTO jsonDocumentDTO)
        {
            var user = HttpContext.User;
            var jsonDocumentService = Services.Get<JsonDocumentService<string>>(connectionId);
            var jsonDocument = jsonDocumentDTO.ToJsonDocument();
            var groups = await GetGroups(user, jsonDocument.Id, connectionId, jsonDocumentService);
            jsonDocumentService.Update(jsonDocument, user);
            var parameters = new Parameters
            {
                { "id", jsonDocument.Id },
                { "data", jsonDocument.Data },
                { "userName", user.GetUserId() }
            };

            await SendMessages(groups, "JsonDocumentUpdated", parameters);
            if (jsonDocumentService.TryGet(jsonDocument.Id, out var entity, user))
            {
                return Ok(entity.ToDTO());
            }

            return NotFound();
        }

        /// <summary>
        ///     Deletes the JSON document with the specified fullname identifier.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted the document.</response>
        /// <response code="200">OK. Successfully soft-deleted the document (marked as deleted, but document is still available).</response>
        /// <response code="404">Document not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="fullName">The JSON document fullname identifier.</param>
        /// <param name="softDelete">Set to true if scenario should be soft-deleted, false otherwise.</param>
        [HttpDelete("{fullName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string connectionId, string fullName, bool softDelete = false)
        {
            var user = HttpContext.User;
            var jsonDocumentService = Services.Get<JsonDocumentService<string>>(connectionId);
            var jsonDocumentId = FullNameString.FromUrl(fullName);
            var groups = await GetGroups(user, jsonDocumentId, connectionId, jsonDocumentService);
            var parameters = new Parameters
            {
                { "id", jsonDocumentId },
                { "userName", user.GetUserId() }
            };

            if (softDelete)
            {
                jsonDocumentService.TrySoftRemove(jsonDocumentId, user);
                await SendMessages(groups, "JsonDocumentDeleted", parameters);
                if (jsonDocumentService.TryGet(jsonDocumentId, out var entity, user))
                {
                    return Ok(entity.ToDTO());
                }

                return NotFound();
            }

            jsonDocumentService.Remove(jsonDocumentId, user);
            await SendMessages(groups, "JsonDocumentDeleted", parameters);
            return NoContent();
        }

        private static string[] ParseDataSelectors(string dataSelectors)
        {
            return dataSelectors?.TrimStart('[').TrimEnd(']').Split(',').Select(d => d.Trim()).ToArray();
        }

        private async Task<IEnumerable<string>> GetGroups(ClaimsPrincipal user, string id, string connectionId, JsonDocumentService<string> jsonDocumentService)
        {
            var groups = new List<string>();
            foreach (var filter in await _filterService.GetListAsync("JsonDocument", connectionId))
            {
                if (filter.QueryConditions is null || !filter.QueryConditions.Any())
                {
                    groups.Add(filter.Id);
                }
                else
                {
                    try
                    {
                        var query = new Query<JsonDocument<string>>(filter.QueryConditions) { new QueryCondition("Id", id) };
                        if (!jsonDocumentService.Get(query, null, user).Any())
                        {
                            continue;
                        }

                        groups.Add(filter.Id);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error while getting groups for JsonDocument");
                    }
                }
            }

            return groups;
        }

        private async Task SendMessages(IEnumerable<string> groups, string action, Parameters parameters)
        {
            foreach (var group in groups)
            {
                try
                {
                    await _hubContext.Clients.Groups(group).SendAsync(action, parameters);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while sending message to group {Group}", group);
                }
            }
        }
    }
}