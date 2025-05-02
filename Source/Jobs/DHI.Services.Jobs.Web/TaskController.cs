namespace DHI.Services.Jobs.Web
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using System.Web.Http.Description;
    using Microsoft.Web.Http;
    using Workflows;

    /// <summary>
    ///     Tasks API
    /// </summary>
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/task/{connectionId}")]
    [ControllerExceptionFilter]
    [ApiVersion("1")]
    public class TaskController : ApiController
    {
        /// <summary>
        ///     Gets the task with the specified identifier.
        /// </summary>
        /// <remarks>
        ///     A task is a unit of work that can be executed as a job. Some tasks take input parameters.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Task not found</response>
        [Route("{id}")]
        [ResponseType(typeof(ITask<string>))]
        public IHttpActionResult Get(string connectionId, string id)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var taskService = Services.Get<ITaskService<Workflow, string>>(connectionId);
            return Ok(taskService.Get(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets a list of all task IDs.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [Route("ids")]
        [ResponseType(typeof(IEnumerable<string>))]
        public IHttpActionResult GetIds(string connectionId)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var tableService = Services.Get<ITaskService<Workflow, string>>(connectionId);
            return Ok(tableService.GetIds(user));
        }

        /// <summary>
        ///     Gets a list of all tasks.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [Route("list")]
        [ResponseType(typeof(IEnumerable<ITask<string>>))]
        public IHttpActionResult GetAll(string connectionId)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var taskService = Services.Get<ITaskService<Workflow, string>>(connectionId);
            return Ok(taskService.GetAll(user));
        }

        /// <summary>
        ///     Gets the total number of tasks.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [Route("count")]
        [ResponseType(typeof(int))]
        public IHttpActionResult GetCount(string connectionId)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var taskService = Services.Get<ITaskService<Workflow, string>>(connectionId);
            return Ok(taskService.Count(user));
        }
    }
}