namespace DHI.Services.Jobs.WebApi
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// job health API
    /// </summary>
    [Produces("application/json")]
    [Route("api/jobs/{connectionId}/health")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for checking job health in past hours")]
    public class JobHealthController : ControllerBase
    {
        public JobHealthController()
        {
        }

        /// <summary>
        ///     Get all jobs in past hours by field taskId(optional)
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="field">The name of DateTime/DateTime? field of Job. Ex. Requested, Finished</param>
        /// <param name="pastHours">hours before current</param>
        /// <param name="taskId">Optional</param>
        /// <response code="400">If field is not exist or field type is not DateTime/DateTime?</response>
        /// <response code="404">If no jobs found for the last hours for field</response>
        [HttpGet("gap/{field}/{pastHours}/{taskId?}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult<IEnumerable<Job<Guid, string>>> GapJobsInPastHoursByFieldForTaskId(string connectionId, string field, double pastHours, string taskId = null)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);

            //validate field 
            var jobType = typeof(Job);
            var property = jobType.GetProperty(field.CapitalizeFirstLetter());
            if (property == null || (property.PropertyType != typeof(DateTime?) && property.PropertyType != typeof(DateTime)))
            {
                return BadRequest($"Field {field} is not exist or its type is not valid");
            }

            var query = new Query<Job<Guid, string>>
            {
                new QueryCondition(property.Name, QueryOperator.GreaterThanOrEqual, DateTime.UtcNow.AddHours(-pastHours)),
            };

            if (taskId != null)
            {
                query.Add(new QueryCondition("TaskId", taskId));
            }

            var jobs = jobService.Get(query, user);

            return jobs.Any() ? Ok(jobs) : NotFound();
        }

        /// <summary>
        /// Get all incomplete jobs requested in past hours for taskId(optional)
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="pastHours">hours before current</param>
        /// <param name="taskId">Optional</param>
        /// <response code="404">If all jobs requested from the past few hours are completed</response>
        [HttpGet("incomplete/{pastHours}/{taskId?}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult<IEnumerable<Job<Guid, string>>> IncompleteJobsInPastHoursForTaskId(string connectionId, double pastHours, string taskId = null)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);

            var since = DateTime.UtcNow.AddHours(-pastHours);
            var inCompleteJobs = jobService.Get(null, since, null, taskId, null, user)
                                           .Where(job => job.Status != JobStatus.Completed);

            return inCompleteJobs.Any() ? Ok(inCompleteJobs) : NotFound();
        }

        /// <summary>
        /// Get all jobs requested in past hours, if the error ratio of all these jobs is within maxErrorRatio
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="pastHours">hours before current</param>
        /// <param name="maxErrorRatio">Represent percentile, range from 0 to 100, both side included </param>
        /// <remarks>Error ratio: number of error jobs / number of finished jobs(status either error or completed)</remarks>
        /// <response code="400">If maxErrorRatio is out of range</response>
        /// <response code="404">If error ratio is greater than maxErrorRatio</response>
        [HttpGet("errorratio/{pastHours}/{maxErrorRatio}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult<IEnumerable<Job<Guid, string>>> ErrorRatioInPastHours(string connectionId, double pastHours, double maxErrorRatio)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);

            // validate maxErroRatio
            if (maxErrorRatio < 0 || maxErrorRatio > 100)
            {
                return BadRequest("maxErrorRatio should be within 0 to 100");
            }

            var since = DateTime.UtcNow.AddHours(-pastHours);
            var jobs = jobService.Get(null, since, null, default, null, user);

            var errorJobs = jobs.Where(job => job.Status == JobStatus.Error);
            var finishedJobs = jobs.Where(job => job.Status == JobStatus.Error || job.Status == JobStatus.Completed);

            return IsWithInMaxErrorRatio(finishedJobs.Count(), errorJobs.Count(), maxErrorRatio) ? Ok(finishedJobs) : NotFound();
        }

        /// <summary>
        /// Get all jobs requested from the past few hours that delays longer than maxDelayMinutes  
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="pastHours">hours before current</param>
        /// <param name="maxDelayMinutes"></param>
        /// <response code="404">If no job delayed longer than maxDelayMinutes</response>
        [HttpGet("delay/{pastHours}/{maxDelayMinutes}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult<IEnumerable<Job<Guid, string>>> DelayedJobsInPastHours(string connectionId, double pastHours, double maxDelayMinutes)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);

            var since = DateTime.UtcNow.AddHours(-pastHours);
            var jobs = jobService.Get(null, since, null, default, null, user);

            var JobsExceedMaxDelayMinutes = jobs.Where(job => job.Started.HasValue && (job.Started.Value - job.Requested).TotalMinutes > maxDelayMinutes);

            return JobsExceedMaxDelayMinutes.Any() ? Ok(JobsExceedMaxDelayMinutes) : NotFound();
        }

        /// <summary>
        /// Get all jobs requested in past hours, if all these jobs are no less than expectedNumberOfJobs
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="pastHours">hours before current</param>
        /// <param name="expectedNumberOfJobs">Expected number of jobs which has started</param>
        /// <response code="404">If jobs requested from the past few hours are less than expectedNumberOfJobs</response>
        [HttpGet("minimumstarted/{pastHours}/{expectedNumberOfJobs}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult<IEnumerable<Job<Guid, string>>> MinimumStartedJobsInPastHours(string connectionId, double pastHours, int expectedNumberOfJobs)
        {
            var user = HttpContext.User;
            var jobService = Services.Get<IJobService<string>>(connectionId);

            var since = DateTime.UtcNow.AddHours(-pastHours);
            var jobs = jobService.Get(null, since, null, null, null, user);

            return jobs.Count() >= expectedNumberOfJobs ? Ok(jobs) : NotFound();
        }

        private static bool IsWithInMaxErrorRatio(int finishedJobsCount, int errorJobsCount, double maxErrorRatio)
        {
            return finishedJobsCount != 0 && (double)errorJobsCount / finishedJobsCount <= maxErrorRatio / 100;
        }

    }
}
