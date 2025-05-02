namespace DHI.Services.Models.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using TimeSeries;
    using WebApiCore;

    /// <summary>
    ///     Scenarios API
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
        ///     Adds a new scenario.
        /// </summary>
        /// <response code="201">Created</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="scenarioDTO">The scenario body</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public ActionResult<Scenario> Add(string connectionId, [FromBody] ScenarioDto scenarioDTO)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);

            var scenario = scenarioDTO.ToScenario();
            scenarioService.Add(scenario, user);
            var id = scenario.Id;
            return CreatedAtAction(nameof(Get), new {connectionId, id}, scenario);
        }

        /// <summary>
        ///     Creates and adds a new scenario from the scenario with the specified identifier.
        /// </summary>
        /// <remarks>
        ///     The individual providers might support other query string parameters than the below default parameters.
        ///     For details about such provider-specific parameters, see the provider documentation.
        /// </remarks>
        /// <response code="201">Created</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="derivedName">The name of the derived scenario.</param>
        /// <param name="id">The identifier of the base scenario.</param>
        /// <param name="simulationId">The identifier of the simulation.</param>
        [HttpPost("{id}/derived")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Scenario> CreateDerived(string connectionId, string id, [Required] string derivedName, [Required] Guid simulationId)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            var parameters = Request.Query.GetAdditionalParameters(new[] { "derivedName", "simulationId" });
            var scenario = scenarioService.CreateAndAdd(derivedName, id, simulationId, parameters, user);
            return CreatedAtAction(nameof(Get), new { connectionId, scenario.Id }, scenario);
        }

        /// <summary>
        ///     Updates an existing scenario.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Scenario not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="scenarioDTO">The scenario body.</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<Scenario> Update(string connectionId, [FromBody] ScenarioDto scenarioDTO)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            var scenario = scenarioDTO.ToScenario();
            scenarioService.Update(scenario, user);
            return Ok(scenarioService.Get(scenario.Id, user));
        }

        /// <summary>
        ///     Deletes the scenario with the specified identifier.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Scenario not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="id">The scenario identifier.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string connectionId, string id)
        {
            var user = HttpContext.User;
            var placeService = Services.Get<ScenarioService>(connectionId);
            placeService.Remove(id, user);
            return NoContent();
        }

        /// <summary>
        ///     Gets the specified scenario.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Scenario not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The scenario identifier.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Scenario> Get(string connectionId, string id)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            return Ok(scenarioService.Get(id, user));
        }

        /// <summary>
        ///     Gets all scenarios.
        /// </summary>
        /// <response code="200">OK</response>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<Scenario>> GetAll(string connectionId)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            return Ok(scenarioService.GetAll(user));
        }

        /// <summary>
        ///     Gets all simulations of a specific scenario.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The scenario identifier.</param>
        /// <response code="200">OK</response>
        [HttpGet("{id}/simulations")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<Scenario>> GetSimulations(string connectionId, string id)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            return Ok(scenarioService.GetSimulations(id, user));
        }

        /// <summary>
        ///     Gets the data from the specified time series of the specified simulation of the specified scenario.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The scenario identifier.</param>
        /// <param name="simulationId">The simulation identifier.</param>
        /// <param name="timeSeriesId">The time series identifier.</param>
        [HttpGet("{id}/simulations/{simulationId}/data/{timeSeriesId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ITimeSeriesData<double>>> GetSimulationData(string connectionId, string id, Guid simulationId, string timeSeriesId)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            var data = await scenarioService.GetSimulationData(id, simulationId, timeSeriesId, user);
            return Ok(data);
        }

        /// <summary>
        ///     Executes the scenario with the specified identifier
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The scenario identifier.</param>
        [HttpPost("execute/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Guid> Execute(string connectionId, string id)
        {
            var user = HttpContext.User;
            var scenarioService = Services.Get<ScenarioService>(connectionId);
            var simulationId = scenarioService.Execute(id, user);
            return Ok(simulationId);
        }
    }
}