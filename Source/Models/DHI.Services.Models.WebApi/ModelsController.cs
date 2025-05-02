namespace DHI.Services.Models.WebApi
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Model Data Readers API
    /// </summary>
    [Produces("application/json")]
    [Route("api/models/readers/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing model data readers.")]
    public class ModelsController : ControllerBase
    {
        /// <summary>
        ///     Gets the model data reader with the specified identifier.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Model not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="id">The model ID.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ModelDataReaderDtoResponse> Get(string connectionId, string id)
        {
            var user = HttpContext.User;
            var modelDataReaderService = Services.Get<ModelDataReaderService>(connectionId);
            return Ok(modelDataReaderService.Get(id, user).ToDTO());
        }

        /// <summary>
        ///     Gets all model data readers.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ModelDataReaderDtoResponse>> GetAll(string connectionId)
        {
            var user = HttpContext.User;
            var modelDataReaderService = Services.Get<ModelDataReaderService>(connectionId);
            return  Ok(modelDataReaderService.GetAll(user).ToDTOs());
        }


        /// <summary>
        ///     Gets the total number of model data readers.
        /// </summary>
        /// <param name="connectionId">The connection identifier</param>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount(string connectionId)
        {
            var user = HttpContext.User;
            var modelDataReaderService = Services.Get<ModelDataReaderService>(connectionId);
            return Ok(modelDataReaderService.Count(user));
        }

        /// <summary>
        ///     Gets a list of all model data reader IDs.
        /// </summary>
        /// <param name="connectionId">The connection identifier</param>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds(string connectionId)
        {
            var user = HttpContext.User;
            var modelDataReaderService = Services.Get<ModelDataReaderService>(connectionId);
            return Ok(modelDataReaderService.GetIds(user));
        }

        /// <summary>
        ///     Adds a new model data reader.
        /// </summary>
        /// <response code="201">Created</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="modelDataReaderDtoRequest">The model data reader body.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public ActionResult<ModelDataReaderDtoResponse> Add(string connectionId, [FromBody] ModelDataReaderDtoRequest modelDataReaderDtoRequest)
        {
            var user = HttpContext.User;
            var modelDataReaderService = Services.Get<ModelDataReaderService>(connectionId);
            var model = modelDataReaderDtoRequest.ToModelDataReader();
            modelDataReaderService.Add(model, user);
            return CreatedAtAction(nameof(Get), new {connectionId, id = model.Id}, model.ToDTO());
        }

        /// <summary>
        ///     Updates an existing model data reader.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Model data reader not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="modelDataReaderDtoRequest">The model body.</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<ModelDataReaderDtoResponse> Update(string connectionId, [FromBody] ModelDataReaderDtoRequest modelDataReaderDtoRequest)
        {
            var user = HttpContext.User;
            var modelDataReaderService = Services.Get<ModelDataReaderService>(connectionId);
            var model = modelDataReaderDtoRequest.ToModelDataReader();
            modelDataReaderService.Update(model, user);
            return Ok(modelDataReaderService.Get(model.Id, user).ToDTO());
        }

        /// <summary>
        ///     Deletes the model data reader with the specified identifier.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Model data reader not found</response>
        /// <param name="connectionId">The connection identifier</param>
        /// <param name="id">The model data reader ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string connectionId, string id)
        {
            var user = HttpContext.User;
            var modelDataReaderService = Services.Get<ModelDataReaderService>(connectionId);
            modelDataReaderService.Remove(FullNameString.FromUrl(id), user);
            return NoContent();
        }
    }
}