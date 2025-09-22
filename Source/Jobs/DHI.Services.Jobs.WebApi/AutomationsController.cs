namespace DHI.Services.Jobs.WebApi
{
    using Automations;
    using DHI.Services.Jobs.WebApi.DTOs;
    using DHI.Services.Scalars;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Web;
    using WebApiCore;

    /// <summary>
    ///     Automations API
    /// </summary>
    [Produces("application/json")]
    [Route("api/automations")]
    [Authorize(Policy = "AdministratorsOnly")]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing job automations. Automations are recipes for automated job execution.")]
    public class AutomationsController : ControllerBase
    {
        private readonly AutomationService _automationService;

        public AutomationsController(IAutomationRepository repository, IScalarRepository<string, int> scalarRepository, IJobRepository<Guid, string> jobRepository)
        {
            _automationService = new AutomationService(repository, scalarRepository, jobRepository);
        }

        /// <summary>
        ///     Gets the job automation with the specified identifier.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Automation not found</response>
        /// <param name="id">The automation ID.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Automation> Get(string id)
        {
            var user = HttpContext.User;
            var decodedId = HttpUtility.UrlDecode(id);
            return Ok(_automationService.Get(FullNameString.FromUrl(decodedId), user));
        }

        /// <summary>
        ///     Gets a list of job automations.
        /// </summary>
        /// <remarks>
        ///     If no group is given, a list of all automations is retrieved.
        ///     If a group is given, a list of automations within the given group is retrieved
        /// </remarks>
        /// <param name="group">The group.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Automation>> GetList(string group = null)
        {
            var user = HttpContext.User;
            return group == null ? Ok(_automationService.GetAll(user)) : Ok(_automationService.GetByGroup(group, user));
        }

        /// <summary>
        ///     Gets the total number of job automations.
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount()
        {
            var user = HttpContext.User;
            return Ok(_automationService.Count(user));
        }

        /// <summary>
        ///     Gets a list of job automation full-name identifiers.
        /// </summary>
        /// <remarks>
        ///     If a group is given, a list of time series full-name identifiers within the given group (recursive) is retrieved.
        /// </remarks>
        /// <param name="group">The group.</param>
        [HttpGet("fullnames")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetFullNames(string group = null)
        {
            var user = HttpContext.User;
            return group == null ? Ok(_automationService.GetFullNames(user)) : Ok(_automationService.GetFullNames(group, user));
        }

        /// <summary>
        ///     Gets a list of all job automation IDs.
        /// </summary>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds()
        {
            var user = HttpContext.User;
            return Ok(_automationService.GetIds(user));
        }

        /// <summary>
        ///     Adds a new job automation.
        /// </summary>
        /// <remarks>
        ///     The automation body must define the automation type and the individual predicate types using the $type property.
        /// </remarks>
        /// <response code="201">Created</response>
        /// <param name="jsonElement">The job automation body.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public ActionResult<Automation> Add([FromBody] JsonElement jsonElement)
        {
            var user = HttpContext.User;
            var automation = JsonSerializer.Deserialize<Automation>(jsonElement.GetRawText(), SerializerOptionsDefault.Options);
            _automationService.Add(automation, user);
            return CreatedAtAction(nameof(Get), new { id = FullNameString.ToUrl(automation.FullName) }, automation);
        }

        /// <summary>
        ///     Updates an existing job automation.
        /// </summary>
        /// <remarks>
        ///     The automation body must define the automation type and the individual predicate types using the $type property.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="404">Job automation not found</response>
        /// <param name="jsonElement">The job automation body.</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<Automation> Update([FromBody] JsonElement jsonElement)
        {
            var user = HttpContext.User;
            var automation = JsonSerializer.Deserialize<Automation>(jsonElement.GetRawText(), SerializerOptionsDefault.Options);
            _automationService.Update((Automation)automation, user);
            return Ok(_automationService.Get(((Automation)automation).FullName, user));
        }

        /// <summary>
        ///     Deletes the job automation with the specified identifier.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Automation not found</response>
        /// <param name="id">The Automation ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string id)
        {
            var user = HttpContext.User;
            var decodedId = HttpUtility.UrlDecode(id);
            _automationService.Remove(FullNameString.FromUrl(decodedId), user);
            return NoContent();
        }

        [HttpPut("{id}/enable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<Automation> Enable(string id, [FromBody] EnableAutomationRequest request)
        {
            var user = HttpContext.User;
            var decodedId = HttpUtility.UrlDecode(id);
            var automation = _automationService.Get(FullNameString.FromUrl(decodedId), user);

            if (request.Flag)
            {
                automation.Enable();
            }
            else
            {
                automation.Disable();
            }

            _automationService.Update(automation);
            return Ok(automation);
        }

        [HttpGet("version")]
        public ActionResult<string> GetVersion()
        {
            var timestamp = _automationService.GetVersionTimestamp();
            return timestamp.HasValue
                ? Ok(timestamp.Value.ToString("O"))
                : StatusCode(StatusCodes.Status501NotImplemented, "Versioning is not supported by the current repository.");
        }
    }
}