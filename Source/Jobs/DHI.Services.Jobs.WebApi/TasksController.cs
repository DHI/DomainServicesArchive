namespace DHI.Services.Jobs.WebApi
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;
    using Workflows;

    /// <summary>
    ///     Tasks API
    /// </summary>
    [Produces("application/json")]
    [Route("api/tasks/{connectionId}")]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing job tasks.")]
    public class TasksController : ControllerBase
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
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult<ITask<string>> Get(string connectionId, string id)
        {
            var user = HttpContext.User;
            try
            {
                var taskService = Services.Get<ITaskService<Workflow, string>>(connectionId);
                return Ok(taskService.Get(FullNameString.FromUrl(id), user));
            }
            catch (InvalidCastException e)
            {
                // this is a hack; remove the try when XAML Workflows are sunset
                var taskService = Services.Get<ITaskService<CodeWorkflow, string>>(connectionId);
                return Ok(taskService.Get(FullNameString.FromUrl(id), user));
            }

            return BadRequest();
        }

        /// <summary>
        ///     Gets a list of all task IDs.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public ActionResult<IEnumerable<string>> GetIds(string connectionId)
        {
            var user = HttpContext.User;
            var taskService = Services.Get<IDiscreteService<ITask<string>, string>>(connectionId);
            return Ok(taskService.GetIds(user));
        }

        /// <summary>
        ///     Gets a list of all tasks.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public ActionResult<IEnumerable<ITask<string>>> GetAll(string connectionId)
        {
            var user = HttpContext.User;
            var taskService = Services.Get<IDiscreteService<ITask<string>, string>>(connectionId);
            return Ok(taskService.GetAll(user));
        }

        /// <summary>
        ///     Gets the total number of tasks.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public ActionResult<int> GetCount(string connectionId)
        {
            var user = HttpContext.User;
            var taskService = Services.Get<IDiscreteService<ITask<string>, string>>(connectionId);
            return Ok(taskService.Count(user));
        }
    }
}