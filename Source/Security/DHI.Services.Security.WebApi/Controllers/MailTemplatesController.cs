namespace DHI.Services.Security.WebApi.Controllers
{
    using System.Collections.Generic;
    using DTOs;
    using Mails;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    ///     Mail Templates API
    /// </summary>
    [Produces("application/json")]
    [Route("api/mailtemplates")]
    [Authorize(Policy = "AdministratorsOnly")]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing mail templates. The mail templates are used in the account registration/activation workflow and password reset.")]
    public class MailTemplatesController : ControllerBase
    {
        private readonly MailTemplateService _mailTemplateService;

        public MailTemplatesController(IMailTemplateRepository mailTemplateRepository)
        {
            _mailTemplateService = new MailTemplateService(mailTemplateRepository);
        }

        /// <summary>
        ///     Gets the email template with the specified identifier.
        /// </summary>
        /// <response code="404">Email template not found</response>
        /// <param name="id">The email template ID.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<MailTemplate> Get(string id)
        {
            var user = HttpContext.User;
            if (!_mailTemplateService.TryGet(id, out var mailTemplate, user))
            {
                return NotFound();
            }

            return Ok(mailTemplate);
        }

        /// <summary>
        ///     Gets a list of all email templates.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<MailTemplate>> GetAll()
        {
            var user = HttpContext.User;
            return Ok(_mailTemplateService.GetAll(user));
        }

        /// <summary>
        ///     Gets the total number of email templates.
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount()
        {
            var user = HttpContext.User;
            return Ok(_mailTemplateService.Count(user));
        }

        /// <summary>
        ///     Gets a list of email template IDs.
        /// </summary>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds()
        {
            var user = HttpContext.User;
            return Ok(_mailTemplateService.GetIds(user));
        }

        /// <summary>
        ///     Adds a new email template
        /// </summary>
        /// <response code="201">Created</response>
        /// <param name="mailTemplateDTO">The email template body.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public ActionResult<MailTemplate> Add([FromBody] MailTemplateDTO mailTemplateDTO)
        {
            var user = HttpContext.User;
            var template = mailTemplateDTO.ToMailTemplate();
            _mailTemplateService.Add(template, user);
            return CreatedAtAction(nameof(Get), new { id = template.Id }, template);
        }

        /// <summary>
        ///     Updates an existing email template.
        /// </summary>
        /// <response code="404">Email template not found</response>
        /// <param name="mailTemplateDTO">The email template body.</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<MailTemplate> Update([FromBody] MailTemplateDTO mailTemplateDTO)
        {
            var user = HttpContext.User;
            var template = mailTemplateDTO.ToMailTemplate();
            _mailTemplateService.Update(template, user);
            if (!_mailTemplateService.TryGet(template.Id, out var mailTemplate, user))
            {
                return NotFound();
            }

            return Ok(mailTemplate);
        }

        /// <summary>
        ///     Deletes the email template with the specified identifier.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Email template not found</response>
        /// <param name="id">The email template ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string id)
        {
            var user = HttpContext.User;
            _mailTemplateService.Remove(id, user);
            return NoContent();
        }
    }
}