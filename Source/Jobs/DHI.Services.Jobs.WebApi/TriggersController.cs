namespace DHI.Services.Jobs.WebApi
{
    using System.Collections.Generic;
    using System.Text.Json;
    using Automations;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Automation trigger API
    /// </summary>
    [Produces("application/json")]
    [Route("api/automations/triggers")]
    [Authorize(Policy = "AdministratorsOnly")]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for getting job automations triggers. Automations trigger are the needed information to create automations.")]
    public class TriggersController : Controller
    {
        private readonly TriggerService _triggerService;
        private readonly JsonSerializerOptions _options;

        public TriggersController(ITriggerRepository repository)
        {
            _triggerService = new TriggerService(repository);

            _options = new JsonSerializerOptions(SerializerOptionsDefault.Options)
            {
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            };
        }

        /// <summary>
        ///     Gets the job automation trigger with the specified identifier.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Trigger not found</response>
        /// <param name="id">The automation trigger ID.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<TriggerParameters> Get(string id)
        {
            var user = HttpContext.User;
            var triggerParameters = _triggerService.Get(FullNameString.FromUrl(id), user);

            return Json(triggerParameters, _options);
        }

        /// <summary>
        ///     Gets a list of job automations triggers.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<TriggerParameters>> GetList()
        {
            var user = HttpContext.User;

            var triggerParameters = _triggerService.GetAll(user);
            return Json(triggerParameters, _options);
        }

        /// <summary>
        ///     Gets the total number of job automations.
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount()
        {
            var user = HttpContext.User;
            return Json(_triggerService.Count(user), _options);
        }

        /// <summary>
        ///     Gets a list of all job automation IDs.
        /// </summary>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds()
        {
            var user = HttpContext.User;
            return Json(_triggerService.GetIds(user), _options);
        }
    }
}