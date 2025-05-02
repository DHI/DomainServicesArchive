namespace DHI.Services.Documents.WebApi
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Documents API
    /// </summary>
    [Produces("application/json")]
    [Route("api/documents/{connectionId}")]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for uploading and downloading documents.")]
    public class DocumentsController : ControllerBase
    {
        /// <summary>
        ///     Deletes the document with the specified identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The document identifier.</param>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Document not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "AdministratorsOnly")]
        public IActionResult Delete(string connectionId, string id)
        {
            var user = HttpContext.User;
            var documentService = Services.Get<DocumentService<string>>(connectionId);
            documentService.Remove(FullNameString.FromUrl(id), user);
            return NoContent();
        }

        /// <summary>
        ///     Downloads the document with the specified identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The document identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Document not found</response>
        [Produces("application/octet-stream",
            "image/bmp",
            "image/gif",
            "image/jpeg",
            "image/png",
            "image/tiff",
            "application/zip",
            "application/pdf",
            "text/csv",
            "text/html",
            "text/xml")]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public IActionResult Get(string connectionId, string id)
        {
            var user = HttpContext.User;
            var documentService = Services.Get<DocumentService<string>>(connectionId);
            var (stream, fileType, fileName) = documentService.Get(FullNameString.FromUrl(id), user);

            if (stream is null)
            {
                return NotFound();
            }

            string contentType;
            switch (fileType)
            {
                case "bmp":
                case "gif":
                case "jpeg":
                case "png":
                case "tiff":
                    contentType = $"image/{fileType}";
                    break;
                case "zip":
                case "pdf":
                    contentType = $"application/{fileType}";
                    break;
                case "csv":
                case "html":
                case "xml":
                    contentType = $"text/{fileType}";
                    break;
                default:
                    contentType = "application/octet-stream";
                    break;

            }

            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, contentType, fileName);
        }

        /// <summary>
        ///     Gets the total number of documents.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public ActionResult<int> GetCount(string connectionId)
        {
            var user = HttpContext.User;
            var documentService = Services.Get<DocumentService<string>>(connectionId);
            return Ok(documentService.Count(user));
        }

        /// <summary>
        ///     Gets a list of all document IDs.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public ActionResult<IEnumerable<string>> GetIds(string connectionId)
        {
            var user = HttpContext.User;
            var documentService = Services.Get<DocumentService<string>>(connectionId);
            return Ok(documentService.GetIds(user));
        }

        /// <summary>
        ///     Gets the metadata for the document with the specified ID.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The document identifier.</param>
        /// <response code="404">Document not found</response>
        [HttpGet("{id}/metadata")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult<IDictionary<string, string>> GetMetadata(string connectionId, string id)
        {
            var user = HttpContext.User;
            var documentService = Services.Get<DocumentService<string>>(connectionId);
            return Ok(documentService.GetMetadata(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets the metadata for documents whose metadata full-fills the given search filter.
        /// </summary>
        /// <remarks>
        ///    If no filter is given, the metadata for all documents is returned.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="filter">The search filter.</param>
        /// <response code="404">Document not found</response>
        [HttpGet("metadata")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public ActionResult<IDictionary<string, string>> GetMetadataByFilter(string connectionId, [FromQuery]string filter = null)
        {
            var user = HttpContext.User;
            var documentService = Services.Get<DocumentService<string>>(connectionId);
            if (filter is null)
            {
                return Ok(documentService.GetAllMetadata(user));
            }

            var queryParameters = Request.Query.ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value.ToString());
            var parameters = new Parameters();
            foreach (var pair in queryParameters.Where(pair => pair.Key != "filter" && !string.IsNullOrWhiteSpace(pair.Value)))
            {
                parameters.Add(pair.Key, pair.Value);
            }
            
            return Ok(documentService.GetMetadataByFilter(filter, parameters, user));
        }

        /// <summary>
        ///     Uploads a new document with the specified identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The document identifier.</param>
        /// <param name="file">The file to upload.</param>
        /// <response code="400">Document validation failed.</response>
        [HttpPost("{id}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "EditorsOnly")]
        public IActionResult Add(string connectionId, string id, IFormFile file)
        {
            var user = HttpContext.User;
            var documentService = Services.Get<DocumentService<string>>(connectionId);
            var queryParameters = Request.Query.ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value.ToString());
            var parameters = new Parameters { {"fileName", file.FileName} };
            foreach (var pair in queryParameters.Where(pair => !string.IsNullOrWhiteSpace(pair.Value)))
            {
                parameters.Add(pair.Key, pair.Value);
            }
            
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                stream.Seek(0, SeekOrigin.Begin);

                if (stream.Length != 0)
                {
                    documentService.Add(stream, FullNameString.FromUrl(id), parameters, user);
                }

                return CreatedAtAction(nameof(Get), new { connectionId, id }, Request.Body);
            }
        }

        /// <summary>
        ///     Gets all document fullname identifiers within a given group.
        ///     If no group is given, all document fullname identifiers are returned.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is grouped (hierarchical).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        [HttpGet("fullnames")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetFullNames(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var documentService = Services.Get<GroupedDocumentService<string>>(connectionId);
            return group == null ? Ok(documentService.GetFullNames(user)) :
                Ok(documentService.GetFullNames(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Gets a list of all documents within a given group.
        ///     If no group is given, all documents are returned.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is grouped (hierarchical).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        [HttpGet]
        public ActionResult<Document<string>> GetList(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var documentService = Services.Get<GroupedDocumentService<string>>(connectionId);
            return group == null ? Ok(documentService.GetAll(user)) :
                Ok(documentService.GetByGroup(FullNameString.FromUrl(group), user));
        }
    }
}