namespace DHI.Services.Jobs.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Scenarios;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Scenarios API.
    /// </summary>
    [Produces("application/json")]
    [Route("api/scenarios/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing scenarios.")]
    public class ScenariosController : ControllerBase
    {
        /// <summary>
        ///     Gets all scenarios within the given time span
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="from">From datetime</param>
        /// <param name="to">To datetime</param>
        /// <param name="dataSelectors">The selectors for filtering the data. E.g: dataSelectors=[foo,bar]</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ScenarioInfo>> GetWithinTimeInterval(string connectionId, DateTime? from = null, DateTime? to = null, string dataSelectors = null)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            var cleanedDataSelectors = dataSelectors?.TrimStart('[').TrimEnd(']').Split(',');
            return Ok(scenarioService.Get(fromDateTime, toDateTime, cleanedDataSelectors, user).ToArray());
        }

        /// <summary>
        ///     Gets a list of scenarios by query.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="queryDTO">The query body</param>
        /// <param name="dataSelectors">The selectors for filtering the data. E.g: dataSelectors=[foo,bar]</param>
        [HttpPost("query")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<IEnumerable<ScenarioInfo>> GetByQuery(string connectionId, [FromBody] QueryDTO<Scenario> queryDTO, [FromQuery] string dataSelectors = null)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            var cleanedDataSelectors = dataSelectors?.TrimStart('[').TrimEnd(']').Split(',');
            return Ok(scenarioService.Get(queryDTO.ToQuery(), cleanedDataSelectors, user).ToArray());
        }

        /// <summary>
        ///     Gets a scenario with the specified identifier
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The scenario identifier.</param>
        /// <param name="dataSelectors">The selectors for filtering the data. E.g: dataSelectors=[foo,bar]</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ScenarioInfo> Get(string connectionId, string id, string dataSelectors = null)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            var cleanedDataSelectors = dataSelectors?.TrimStart('[').TrimEnd(']').Split(',');
            return Ok(scenarioService.Get(id, cleanedDataSelectors, user));
        }

        /// <summary>
        ///     Adds a scenario
        /// </summary>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="scenarioDTO">The scenario body</param>
        /// <returns></returns>
        [Authorize(Policy = "EditorsOnly")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public ActionResult<ScenarioInfo> Add(string connectionId, [FromBody] ScenarioDTO scenarioDTO)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            var scenario = scenarioDTO.ToScenario();
            scenarioService.Add(scenario, user);
            return CreatedAtAction(nameof(Get), new {connectionId, id = scenario.Id}, scenarioService.Get(scenario.Id));
        }

        /// <summary>
        ///     Updates an existing scenario.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="scenarioDTO">The scenario body</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "EditorsOnly")]
        [Consumes("application/json")]
        public ActionResult<ScenarioInfo> Update(string connectionId, [FromBody] ScenarioDTO scenarioDTO)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            var scenario = scenarioDTO.ToScenario();
            scenarioService.Update(scenario, user);
            return Ok(scenarioService.Get(scenario.Id));
        }

        /// <summary>
        ///     Deletes the scenario with the specified identifier.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted the scenario.</response>
        /// <response code="200">OK. Successfully soft-deleted the scenario (marked as deleted, but scenario is still available).</response>
        /// <response code="404">Scenario not found.</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The scenario identifier.</param>
        /// <param name="softDelete">Set to <c>true</c> if scenario should be soft deleted, <c>false</c> otherwise.</param>
        /// <returns><c>true</c> if the file exists, <c>false</c> otherwise.</returns>

        [HttpDelete("{id}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string connectionId, string id, bool softDelete=false)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            if (softDelete)
            {
                scenarioService.TrySoftRemove(id, user);
                return Ok(scenarioService.Get(id));
            }

            scenarioService.Remove(id, user);
            return NoContent();
        }
    }
}