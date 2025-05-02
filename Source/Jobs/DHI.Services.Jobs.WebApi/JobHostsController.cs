namespace DHI.Services.Jobs.WebApi
{
    using System.Collections.Generic;
    using Jobs;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Job Hosts API
    /// </summary>
    [Produces("application/json")]
    [Route("api/jobhosts")]
    [Authorize(Policy = "AdministratorsOnly")]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing job hosts. Job hosts are the machines (physical or virtual) used for job execution.")]
    public class JobHostsController : ControllerBase
    {
        private readonly IHostService _hostService;

        public JobHostsController(IHostRepository hostRepository)
        {
            _hostService = hostRepository is IGroupedHostRepository repository ? (IHostService)new GroupedHostService(repository) : new HostService(hostRepository);
        }

        /// <summary>
        ///     Gets the job host with the specified identifier.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Job host not found</response>
        /// <param name="id">The job host ID.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Host> Get(string id)
        {
            var user = HttpContext.User;
            return Ok(_hostService.Get(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets a list of job hosts.
        /// </summary>
        /// <remarks>
        ///     If no group is given, a list of all job hosts is retrieved.
        ///     If a group is given, a list of job hosts within the given group is retrieved. This is only applicable if the connection type is grouped.
        /// </remarks>
        /// <param name="group">The group.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Host>> GetList(string group = null)
        {
            var user = HttpContext.User;
            return group == null ? Ok(_hostService.GetAll(user)) : Ok(((IGroupedHostService)_hostService).GetByGroup(group, user));
        }

        /// <summary>
        ///     Gets the total number of job hosts.
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount()
        {
            var user = HttpContext.User;
            return Ok(_hostService.Count(user));
        }

        /// <summary>
        ///     Gets a list of job host full-name identifiers.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is grouped (hierarchical).
        ///     If a group is given, a list of time series full-name identifiers within the given group (recursive) is retrieved.
        /// </remarks>
        /// <param name="group">The group.</param>
        [HttpGet("fullnames")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetFullNames(string group = null)
        {
            var user = HttpContext.User;
            return group == null ? Ok(((IGroupedHostService)_hostService).GetFullNames(user)) : Ok(((IGroupedHostService)_hostService).GetFullNames(group, user));
        }

        /// <summary>
        ///     Gets a list of all job host IDs.
        /// </summary>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds()
        {
            var user = HttpContext.User;
            return Ok(_hostService.GetIds(user));
        }

        /// <summary>
        ///     Adds a new job host.
        /// </summary>
        /// <response code="201">Created</response>
        /// <param name="hostDto">The job host body.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public ActionResult<Host> Add([FromBody] HostDTO hostDto)
        {
            var user = HttpContext.User;
            var host = hostDto.ToHost();
            _hostService.Add(host, user);
            var hostId = _hostService is IGroupedHostService ? FullNameString.ToUrl(host.FullName) : host.Id;
            return CreatedAtAction(nameof(Get), new { id = hostId }, host);
        }

        /// <summary>
        ///     Updates an existing job host.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Job host not found</response>
        /// <param name="hostDto">The job host body.</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<Host> Update([FromBody] HostDTO hostDto)
        {
            var user = HttpContext.User;
            var host = hostDto.ToHost();
            _hostService.Update(host, user);
            var hostId = _hostService is IGroupedHostService ? host.FullName : host.Id;
            return Ok(_hostService.Get(hostId, user));
        }

        /// <summary>
        ///     Deletes the job host with the specified identifier.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Job host not found</response>
        /// <param name="id">The job host ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string id)
        {
            var user = HttpContext.User;
            _hostService.Remove(FullNameString.FromUrl(id), user);
            return NoContent();
        }
    }
}