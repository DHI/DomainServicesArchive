namespace DHI.Services.Scalars.WebApi
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using System.Collections.Generic;
    using WebApiCore;

    /// <summary>
    ///     Scalars API
    /// </summary>
    [Produces("application/json")]
    [Route("api/scalars/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing scalars.")]
    public class ScalarsController : ControllerBase
    {
        /// <summary>
        ///     Gets the scalar with the specified identifier.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Scalar not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="id">The scalar ID.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ScalarDTO> Get(string connectionId, string id)
        {
            var user = HttpContext.User;
            var scalarService = Services.Get<IScalarService<string, int>>(connectionId);
            if (!scalarService.TryGet(FullNameString.FromUrl(id), out var sc, user))
            {
                return NotFound();
            }


            return Ok(sc.ToDTO());
        }

        /// <summary>
        ///     Gets a list of scalars.
        /// </summary>
        /// <remarks>
        ///     If no group is given, a list of all scalars is retrieved.
        ///     If a group is given, a list of scalars within the given group is retrieved. This is only applicable if the connection type is grouped.
        /// </remarks>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="group">The group.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ScalarDTO> GetList(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var scalarService = Services.Get<IScalarService<string, int>>(connectionId);
            return group == null ? Ok(scalarService.GetAll(user).ToDTOs()) : Ok(((IGroupedScalarService<string, int>)scalarService).GetByGroup(FullNameString.FromUrl(group), user).ToDTOs());
        }

        /// <summary>
        ///     Gets the total number of scalars.
        /// </summary>
        /// <param name="connectionId">The connection identifier</param>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount(string connectionId)
        {
            var user = HttpContext.User;
            var scalarService = Services.Get<IScalarService<string, int>>(connectionId);
            return Ok(scalarService.Count(user));
        }

        /// <summary>
        ///     Gets a list of scalar full-name identifiers.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is grouped (hierarchical).
        ///     If a group is given, a list of time series full-name identifiers within the given group (recursive) is retrieved.
        /// </remarks>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="group">The group.</param>
        [HttpGet("fullnames")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetFullNames(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var scalarService = Services.Get<IGroupedScalarService<string, int>>(connectionId);
            return group == null ? Ok(scalarService.GetFullNames(user)) : Ok(scalarService.GetFullNames(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Gets a list of all scalar IDs.
        /// </summary>
        /// <param name="connectionId">The connection identifier</param>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds(string connectionId)
        {
            var user = HttpContext.User;
            var scalarService = Services.Get<IScalarService<string, int>>(connectionId);
            return Ok(scalarService.GetIds(user));
        }

        /// <summary>
        ///     Adds a new scalar.
        /// </summary>
        /// <response code="201">Created</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="scalarDTO">The scalar body.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public ActionResult<ScalarDTO> Add(string connectionId, [FromBody] ScalarDTO scalarDTO)
        {
            var user = HttpContext.User;
            var scalarService = Services.Get<IScalarService<string, int>>(connectionId);
            var scalar = scalarDTO.ToScalar();
            scalarService.Add(scalar, user);
            var scalarId = scalarService is IGroupedScalarService<string, int> ? FullNameString.ToUrl(scalar.FullName) : scalar.Id;
            return CreatedAtAction(nameof(Get), new { connectionId, id = scalarId }, scalarDTO);
        }

        /// <summary>
        ///     Updates an existing scalar.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Scalar not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="scalarDTO">The scalar body.</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<ScalarDTO> Update(string connectionId, [FromBody] ScalarDTO scalarDTO)
        {
            var user = HttpContext.User;
            var scalarService = Services.Get<IScalarService<string, int>>(connectionId);
            var scalar = scalarDTO.ToScalar();
            scalarService.Update(scalar, user);
            if (!scalarService.TryGet(scalar.Id, out var sc, user))
            {
                return NotFound();
            }

            return Ok(sc.ToDTO());
        }

        /// <summary>
        ///     Sets the data of an existing scalar.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Scalar not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="id">The scalar identifier.</param>
        /// <param name="scalarDataDTO">The scalar data body.</param>
        /// <param name="logging">Set to false to disable logging</param>
        [HttpPut("{id}/data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<ScalarDTO> SetData(string connectionId, string id, [FromBody] ScalarDataDTO scalarDataDTO, bool logging = true)
        {
            var user = HttpContext.User;
            var scalarService = Services.Get<IScalarService<string, int>>(connectionId);
            var scalarId = FullNameString.FromUrl(id);
            scalarService.SetData(scalarId, scalarDataDTO.ToScalarData(), logging, user);
            if (!scalarService.TryGet(scalarId, out var sc, user))
            {
                return NotFound();
            }

            return Ok(sc.ToDTO());
        }

        /// <summary>
        ///     Sets the locked property of an existing scalar.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Scalar not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="id">The scalar identifier.</param>
        /// <param name="lockedDTO">The locked body.</param>
        [HttpPut("{id}/locked")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<ScalarDTO> SetLocked(string connectionId, string id, [FromBody] LockedDTO lockedDTO)
        {
            var user = HttpContext.User;
            var scalarService = Services.Get<IScalarService<string, int>>(connectionId);
            var scalarId = FullNameString.FromUrl(id);
            scalarService.SetLocked(scalarId, lockedDTO.Locked, user);
            if (!scalarService.TryGet(scalarId, out var sc, user))
            {
                return NotFound();
            }

            return Ok(sc.ToDTO());
        }

        /// <summary>
        ///     Deletes the scalar with the specified identifier.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Scalar not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="id">The scalar ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string connectionId, string id)
        {
            var user = HttpContext.User;
            var scalarService = Services.Get<IScalarService<string, int>>(connectionId);
            scalarService.Remove(FullNameString.FromUrl(id), user);
            return NoContent();
        }
    }
}