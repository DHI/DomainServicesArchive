namespace DHI.Services.Jobs.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using System.Web.Http.Description;
    using Microsoft.Web.Http;
    using Newtonsoft.Json.Linq;
    using Workflows;

    /// <summary>
    ///     Jobs API
    /// </summary>
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/job/{connectionId}")]
    [ControllerExceptionFilter]
    [ApiVersion("1")]
    public class JobController : ApiController
    {
        /// <summary>
        ///     Deletes the job with the specified identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Job not found</response>
        [Route("{id}")]
        [Authorize(Roles = "Administrator")]
        public void Delete(string connectionId, Guid id)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var jobService = Services.Get<JobService<Workflow, string>>(connectionId);
            jobService.Remove(id, user);
        }

        /// <summary>
        ///     Deletes all the jobs meeting the criteria specified by the given parameters.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="account">The account.</param>
        /// <param name="before">The before datetime.</param>
        /// <response code="204">No Content. Successfully deleted</response>
        [Route("")]
        [Authorize(Roles = "Administrator")]
        public void Delete(string connectionId, string account = null, string before = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var jobService = Services.Get<JobService<Workflow, string>>(connectionId);

            DateTime? beforeDateTime = null;
            if (before != null)
            {
                beforeDateTime = Datetime.Parse(before);
            }

            jobService.Remove(account, beforeDateTime, user:user);
        }

        /// <summary>
        ///     Gets the job with the specified identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Job not found</response>
        [Route("{id}")]
        [ResponseType(typeof(Job<Guid, string>))]
        public IHttpActionResult Get(string connectionId, Guid id)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var jobService = Services.Get<JobService<Workflow, string>>(connectionId);
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
        [Route("last")]
        [ResponseType(typeof(Job<Guid, string>))]
        public IHttpActionResult GetLast(string connectionId, string account = null, string status = null, string task = null, string tag = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var jobService = Services.Get<JobService<Workflow, string>>(connectionId);

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
        [Route("list")]
        [ResponseType(typeof(IEnumerable<Job<Guid, string>>))]
        public IHttpActionResult Get(string connectionId, string account = null, string since = null, string status = null, string task = null, string tag = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var jobService = Services.Get<JobService<Workflow, string>>(connectionId);

            DateTime? sinceDateTime = null;
            if (since != null)
            {
                sinceDateTime = Datetime.Parse(since);
            }

            JobStatus? jobStatus = null;
            if (status != null)
            {
                jobStatus = (JobStatus)Enum.Parse(typeof(JobStatus), status);
            }

            return Ok(jobService.Get(account, sinceDateTime, jobStatus, task, tag, user));
        }


        /// <summary>
        ///      Gets all the jobs meeting the criteria specified by the given query.
        /// </summary>
        /// <param name="connectionId">The connection ID.</param>
        /// <param name="jobjects">The query body</param>
        [Route("query")]
        [HttpPost]
        [ResponseType(typeof(IEnumerable<Job<Guid, string>>))]
        public IHttpActionResult GetByQuery(string connectionId, [FromBody] JObject[] jobjects)
        {
            var query = _ToQuery(jobjects);
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var jobService = Services.Get<JobService<Workflow, string>>(connectionId);

            return Ok(jobService.Get(new Query<Job<Guid, string>>(query), user));
        }

        /// <summary>
        ///     Gets the total number of jobs.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [Route("count")]
        [ResponseType(typeof(int))]
        public IHttpActionResult GetCount(string connectionId)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var jobService = Services.Get<JobService<Workflow, string>>(connectionId);
            return Ok(jobService.Count(user));
        }

        /// <summary>
        ///     Executes a job.
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
        /// <response code="201">Created</response>
        [Route("")]
        [ValidateModel]
        [Authorize(Roles = "Editor")]
        [HttpPost]
        [ResponseType(typeof(Job<Guid, string>))]
        public IHttpActionResult Execute(string connectionId, [FromBody] JobDTO jobDTO)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var job = new Job<Guid, string>(Guid.NewGuid(), jobDTO.TaskId, User.Identity.Name)
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

            var jobService = Services.Get<JobService<Workflow, string>>(connectionId);
            jobService.Add(job, user);
            return Created($"{Request.RequestUri}/{job.Id}", job);
        }

        /// <summary>
        ///     Requests cancellation of the jobs with the specified job identifiers.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The job identifiers.</param>
        /// <response code="202">Job cancellation requests successfully created</response>
        /// <response code="404">Job not found</response>
        [Route("cancel")]
        [HttpPut]
        [Authorize(Roles = "Editor")]
        public IHttpActionResult CancelMultiple(string connectionId, [FromBody] Guid[] ids)
        {
            var jobService = Services.Get<JobService<Workflow, string>>(connectionId);
            foreach (var id in ids)
            {
                var job = jobService.Get(id);
                job.Status = JobStatus.Cancel;
                jobService.Update(job);
            }

            return Content(HttpStatusCode.Accepted, "");
        }

        /// <summary>
        ///     Requests cancellation of the job with the specified job identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The job identifier.</param>
        /// <response code="202">Job cancellation requests successfully created</response>
        /// <response code="404">Job not found</response>
        [Route("{id}/cancel")]
        [HttpPut]
        [Authorize(Roles = "Editor")]
        public IHttpActionResult Cancel(string connectionId, Guid id)
        {
            var jobService = Services.Get<JobService<Workflow, string>>(connectionId);
            var job = jobService.Get(id);
            job.Status = JobStatus.Cancel;
            jobService.Update(job);
            return Content(HttpStatusCode.Accepted, "");
        }

        private static List<QueryCondition> _ToQuery(JObject[] jobjects)
        {
            var query = new List<QueryCondition>();
            foreach (var jobject in jobjects)
            {
                var item = (string)jobject["Item"];
                var value = (string)jobject["Value"];
                var jqueryOperator = jobject["QueryOperator"];
                var queryOperator = jqueryOperator.ToObject<QueryOperator>();
                query.Add(new QueryCondition(item, queryOperator, value.ToObject()));
            }

            return query;
        }
    }
}