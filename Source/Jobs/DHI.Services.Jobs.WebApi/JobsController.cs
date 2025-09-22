namespace DHI.Services.Jobs.WebApi
{
    using Authorization;
    using Filters;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Json;
    using System.Threading.Tasks;
    using WebApiCore;

    /// <summary>
    ///     Jobs API
    /// </summary>
    [Produces("application/json")]
    [Route("api/jobs/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing jobs.")]
    public class JobsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly FilterService _filterService;
        private readonly JsonSerializerOptions? _jsonSerializerOptions;

        public JobsController(IHubContext<NotificationHub> hubContext, IFilterRepository filterRepository, ILogger logger, IOptions<MvcOptions> mvcOptions = null)
        {
            _hubContext = hubContext;
            _logger = logger;
            _filterService = new FilterService(filterRepository);

            var jsonFormatter = mvcOptions?.Value.OutputFormatters
                .OfType<SystemTextJsonOutputFormatter>()
                .FirstOrDefault();
            _jsonSerializerOptions = jsonFormatter?.SerializerOptions;
        }

        /// <summary>
        ///     Deletes the job with the specified identifier.
        /// </summary>
        /// <remarks>
        ///     NOTE: This endpoint if for system purposes only. You should never manually delete a job.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="status">Job status</param>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Job not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "AdministratorsOnly")]
        public async Task<IActionResult> Delete(string connectionId, Guid id, string status = null)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);

            if (string.IsNullOrEmpty(status))
            {
                jobService.Remove(id, user);
            }
            else
            {
                var job = jobService.Get(id);
                if (job.Status == (JobStatus)Enum.Parse(typeof(JobStatus), status))
                {
                    jobService.Remove(id, user);
                }
            }

            var groups = await GetGroups(user, id, connectionId, jobService);
            var parameters = new Parameters
            {
                {"id", id.ToString()},
                {"userName", user.GetUserId()},
                {"connectionId", connectionId},
                {"status",  status}
            };

            await SendMessages(groups, "JobDeleted", parameters);
            return NoContent();
        }

        /// <summary>
        ///     Deletes all the jobs meeting the criteria specified by the given parameters.
        /// </summary>
        /// <remarks>
        ///     NOTE: This endpoint if for system purposes only. You should never manually delete a job.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="account">The account.</param>
        /// <param name="before">The before datetime.</param>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Policy = "AdministratorsOnly")]
        public IActionResult DeleteByCriteria(string connectionId, string account = null, DateTime? before = null)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);
            jobService.Remove(account, before, user: user);
            return Ok();
        }

        /// <summary>
        ///     Gets the job with the specified identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The job identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Job not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult<Job<Guid, string>> Get(string connectionId, Guid id)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);
            return Ok(jobService.Get(id, user));
        }

        /// <summary>
        ///     Gets the last job meeting the criteria specified by the given parameters.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="account">The account.</param>
        /// <param name="status">The status.</param>
        /// <param name="task">The task.</param>
        /// <param name="tag">The tag.</param>
        [HttpGet("last")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public ActionResult<Job<Guid, string>> GetLast(string connectionId, string account = null, string status = null, string task = null, string tag = null)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);

            JobStatus? jobStatus = null;
            if (status != null)
            {
                jobStatus = (JobStatus)Enum.Parse(typeof(JobStatus), status);
            }

            return Ok(jobService.GetLast(account, jobStatus, task, tag, user));
        }

        /// <summary>
        ///     Gets all the jobs meeting the criteria specified by the given query string parameters.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="account">The account.</param>
        /// <param name="since">The since datetime.</param>
        /// <param name="status">The status.</param>
        /// <param name="task">The task.</param>
        /// <param name="tag">The tag.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public ActionResult<IEnumerable<Job<Guid, string>>> GetByQueryString(string connectionId, string account = null, DateTime? since = null, string status = null, string task = null, string tag = null)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);

            JobStatus? jobStatus = null;
            if (status != null)
            {
                jobStatus = (JobStatus)Enum.Parse(typeof(JobStatus), status);
            }

            return Ok(jobService.Get(account, since, jobStatus, task, tag, user));
        }

        /// <summary>
        ///      Gets all the jobs meeting the criteria specified by the given query.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="queryDTO">The query body</param>
        [HttpPost("query")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Authorize]
        public ActionResult<IEnumerable<Job<Guid, string>>> GetByQuery(string connectionId, [FromBody] QueryDTO queryDTO)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);
            var query = new Query<Job<Guid, string>>();
            foreach (var condition in queryDTO.ToQueryConditions())
            {
                switch (condition.Item)
                {
                    case "Status":
                        query.Add(Enum.TryParse(condition.Value.ToString(), out JobStatus jobStatus) ? new QueryCondition(condition.Item, condition.QueryOperator, jobStatus) : condition);
                        break;
                    case "Id":
                        query.Add(Guid.TryParse(condition.Value.ToString(), out var id) ? new QueryCondition(condition.Item, condition.QueryOperator, id) : condition);
                        break;
                    default:
                        query.Add(condition);
                        break;
                }
            }

            return Ok(jobService.Get(query, user));
        }

        /// <summary>
        ///     Gets the total number of jobs.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public ActionResult<int> GetCount(string connectionId)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);
            return Ok(jobService.Count(user));
        }

        /// <summary>
        ///     Requests a job execution.
        /// </summary>
        /// <remarks>
        ///     A job is a task "instance" – i.e. an execution of a task or a task that has been executed.
        /// </remarks>
        /// <remarks>
        ///     If the specified task requires input parameters, these must be specified. The response includes a Location header
        ///     with a URL to the created job resource. This link can be used to retrieve the status of the job. A job can be
        ///     marked with a given tag by including a Tag property in the request. However, this is optional.
        /// </remarks>
        /// <remarks>
        ///     The unique identifier of a job is a GUID (e.g. 7299875c-ee1f-4f20-bb30-066cc267f1bd).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="jobDTO">The job dto.</param>
        /// <response code="201">Pending job Created</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        [Authorize(Policy = "EditorsOnly")]
        public async Task<ActionResult<Job<Guid, string>>> Execute(string connectionId, [FromBody] JobDTO jobDTO)
        {
            var user = HttpContext.User;

            var jobId = (Guid)((jobDTO.Id == null) ? Guid.NewGuid() : jobDTO.Id);

            var job = new Job<Guid, string>(jobId, jobDTO.TaskId, user.GetUserId())
            {
                Status = JobStatus.Pending,
                Tag = jobDTO.Tag,
                HostGroup = jobDTO.HostGroup,
                Priority = jobDTO.Priority
            };

            if (jobDTO.Parameters != null && jobDTO.Parameters.Count > 0)
            {
                foreach (var parameter in jobDTO.Parameters)
                {
                    job.Parameters.Add(parameter.Key, parameter.Value);
                }
            }

            var jobService = Services.Get<IJobService<string>>(connectionId);
            jobService.Add(job, user);
            var groups = await GetGroups(user, job.Id, connectionId, jobService);
            var parameters = new Parameters
            {
                { "id", job.Id.ToString() },
                { "data", JsonSerializer.Serialize(job, _jsonSerializerOptions) },
                { "userName", user.GetUserId() }
            };

            await SendMessages(groups, "JobAdded", parameters);
            return CreatedAtAction(nameof(Get), new { connectionId, id = job.Id }, job);
        }

        /// <summary>
        ///     Updates the job.
        /// </summary>
        /// <remarks>
        ///     NOTE: This endpoint if for system purposes only. You should never manually update a job.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="jobUpdateDTO">The job update dto.</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        [Authorize(Policy = "AdministratorsOnly")]
        public async Task<ActionResult<Job>> Update(string connectionId, [FromBody] JobUpdateDTO jobUpdateDTO)
        {
            var user = HttpContext.User;
            var job = jobUpdateDTO.ToJob();
            var jobService = Services.Get<IJobService<string>>(connectionId);
            jobService.Update(job, user);
            var groups = await GetGroups(user, job.Id, connectionId, jobService);
            var parameters = new Parameters
            {
                { "id", job.Id.ToString() },
                { "data", JsonSerializer.Serialize(job, _jsonSerializerOptions) },
                { "userName", user.GetUserId() },
                { "connectionId", connectionId }
            };

            await SendMessages(groups, "JobUpdated", parameters);
            return Ok(job);
        }

        /// <summary>
        ///     Updates the job.
        /// </summary>
        /// <remarks>
        ///     This end point is intended for stateless clients, where posting JobUpdateDTO is not possible.
        ///     NOTE: This endpoint if for system purposes only. You should never manually update a job.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>     
        /// <param name="id">The job identifier.</param>
        /// <param name="statusUpdateDTO">Status update DTO.</param>
        [HttpPut("status/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        [Authorize(Policy = "EditorsOnly")]
        public async Task<ActionResult> UpdateStatus(string connectionId, Guid id, [FromBody] JobStatusUpdateDTO statusUpdateDTO)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);

            Job<Guid, string> current;
            try
            {
                current = jobService.Get(id, user);
            }
            catch
            {
                return NotFound();
            }

            if (current == null)
            {
                return NotFound();
            }

            var incoming = statusUpdateDTO.JobStatus;

            if (IsFinal(current.Status))
            {
                if (IsFinal(incoming) && incoming == current.Status)
                {
                    return Ok();
                }

                _logger.LogWarning(
                    "Rejecting status update for job {JobId}. Current state {Current} is terminal; incoming {Incoming}.",
                    id, current.Status, incoming
                );

                return StatusCode(StatusCodes.Status409Conflict, new
                {
                    message = string.Format("Job {0} is already in terminal state '{1}'. Further updates are not allowed.", id, current.Status)
                });
            }

            jobService.UpdateStatus(id, incoming, statusUpdateDTO.StatusMessage, statusUpdateDTO.Progress, user);

            var updated = jobService.Get(id, user);

            var groups = await GetGroups(user, id, connectionId, jobService);

            var parameters = new Parameters
            {
                { "id", updated.Id.ToString() },
                { "data", JsonSerializer.Serialize(updated, _jsonSerializerOptions) },
                { "userName", user.GetUserId() },
                { "connectionId", connectionId }
            };

            await SendMessages(groups, "JobUpdated", parameters);
            return Ok();
        }

        /// <summary>
        ///     Updates the heartbeat field on a job.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>     
        /// <param name="id">The job identifier.</param>
        [HttpPut("heartbeat/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "EditorsOnly")]
        public ActionResult UpdateHeartbeat(string connectionId, Guid id)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);
            jobService.UpdateHeartbeat(id, user);

            return Ok();
        }

        /// <summary>
        ///     Requests cancellation of the jobs with the specified job identifiers.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The job identifiers.</param>
        /// <response code="202">Job cancellation requests successfully created</response>
        /// <response code="404">Job not found</response>
        [HttpPut("cancel")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        [Authorize(Policy = "EditorsOnly")]
        public async Task<IActionResult> CancelMultiple(string connectionId, [FromBody] Guid[] ids)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);
            foreach (var id in ids)
            {
                var job = jobService.Get(id);
                job.Status = JobStatus.Cancel;
                jobService.Update(job);
                var groups = await GetGroups(user, job.Id, connectionId, jobService);
                var parameters = new Parameters
                {
                    { "id", job.Id.ToString() },
                    { "data", JsonSerializer.Serialize(job, _jsonSerializerOptions) },
                    { "userName", user.GetUserId() },
                    { "connectionId", connectionId }
                };

                await SendMessages(groups, "JobUpdated", parameters);
            }

            return Accepted();
        }

        /// <summary>
        ///     Requests cancellation of the job with the specified job identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The job identifier.</param>
        /// <response code="202">Job cancellation requests successfully created</response>
        /// <response code="404">Job not found</response>
        [HttpPut("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "EditorsOnly")]
        public async Task<IActionResult> Cancel(string connectionId, Guid id)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);
            var job = jobService.Get(id);
            job.Status = JobStatus.Cancel;
            jobService.Update(job);
            var groups = await GetGroups(user, job.Id, connectionId, jobService);
            var parameters = new Parameters
            {
                { "id", job.Id.ToString() },
                { "data", JsonSerializer.Serialize(job, _jsonSerializerOptions) },
                { "userName", user.GetUserId() },
                { "connectionId", connectionId }
            };

            await SendMessages(groups, "JobUpdated", parameters);
            return Accepted();
        }

        private async Task<IEnumerable<string>> GetGroups(ClaimsPrincipal user, Guid id, string connectionId, IJobService<string> jobService)
        {
            var groups = new List<string>();
            foreach (var filter in await _filterService.GetListAsync("Job", connectionId))
            {
                if (filter.QueryConditions is null || !filter.QueryConditions.Any())
                {
                    groups.Add(filter.Id);
                }
                else
                {
                    try
                    {
                        var query = new Query<Job<Guid, string>>(filter.QueryConditions) { new QueryCondition("Id", id) };
                        if (jobService.Get(query, user).Any())
                        {
                            groups.Add(filter.Id);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed getting groups");
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
                    _logger.LogError(e, "Failed sending message");
                }
            }
        }

        private static bool IsFinal(JobStatus status)
        {
            return status == JobStatus.Completed
                || status == JobStatus.Error
                || status == JobStatus.Cancelled
                || status == JobStatus.TimedOut;
        }
    }
}