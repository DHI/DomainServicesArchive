namespace DHI.Services.Spreadsheets.WebApi
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.IO;
    using WebApiCore;

    /// <summary>
    ///     Spreadsheets API
    /// </summary>
    [Produces("application/json")]
    [Route("api/spreadsheets/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing and retrieving spreadsheets and spreadsheet data.")]
    public class SpreadsheetsController : ControllerBase
    {
        /// <summary>
        ///     Creates a new spreadsheet.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="spreadsheetDTO">The spreadsheet dto.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        [Authorize(Policy = "EditorsOnly")]
        public ActionResult<Spreadsheet<string>> Add(string connectionId, [FromBody] SpreadsheetDTO spreadsheetDTO)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            var spreadsheet = spreadsheetDTO.ToSpreadsheet();
            spreadsheetService.Add(spreadsheet, user);
            return CreatedAtAction(nameof(Get), new { connectionId, id = FullNameString.ToUrl(spreadsheet.Id) }, spreadsheet);
        }

        /// <summary>
        ///     Updates an existing spreadsheet.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="spreadsheetDTO">The spreadsheet dto.</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        [Authorize(Policy = "EditorsOnly")]
        public ActionResult<Spreadsheet<string>> Update(string connectionId, [FromBody] SpreadsheetDTO spreadsheetDTO)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            var spreadsheet = spreadsheetDTO.ToSpreadsheet();
            spreadsheetService.Update(spreadsheet, user);
            return Ok(spreadsheetService.Get(spreadsheet.Id, user));
        }

        /// <summary>
        ///     Deletes the spreadsheet with the given ID.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The spreadsheet identifier.</param>
        /// <response code="204">No Content. Spreadsheet successfully deleted.</response>
        /// <response code="404">Spreadsheet not found.</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string connectionId, string id)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            spreadsheetService.Remove(FullNameString.FromUrl(id), user);
            return NoContent();
        }

        /// <summary>
        ///     Deletes all spreadsheets within the given group.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        /// <response code="204">No Content. Group successfully deleted.</response>
        /// <response code="404">Spreadsheet not found.</response>
        [HttpDelete("group/{group}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteByGroup(string connectionId, string group)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            spreadsheetService.RemoveByGroup(FullNameString.FromUrl(group), user);
            return NoContent();
        }

        /// <summary>
        ///     Gets a spreadsheet with the given ID.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The spreadsheet identifier.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Spreadsheet<string>> Get(string connectionId, string id)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            return Ok(spreadsheetService.Get(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets the cell value for the given cell in the given sheet in the given spreadsheet.
        /// </summary>
        /// <remarks>
        ///     A cell is given in the format R{row}C{col}. The row and column indices are zero-based.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The spreadsheet identifier.</param>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="cell">The cell. A cell is given in the format R{row}C{col}. The row and column indices are zero-based.</param>
        [HttpGet("{id}/{sheetName}/cell;{cell}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCellValue(string connectionId, string id, string sheetName, string cell)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            return Ok(spreadsheetService.GetCellValue(FullNameString.FromUrl(id), sheetName, Cell.Parse(cell), user));
        }

        /// <summary>
        ///     Gets the total number of spreadsheets.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("count")]
        public ActionResult<int> GetCount(string connectionId)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            return Ok(spreadsheetService.Count(user));
        }

        /// <summary>
        ///     Gets a list of spreadsheet full-name identifiers.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">A group.</param>
        [HttpGet("fullnames")]
        public ActionResult<string[]> GetFullNames(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            return group == null ? Ok(spreadsheetService.GetFullNames(user)) : Ok(spreadsheetService.GetFullNames(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Gets a list of all spreadsheets.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">A group.</param>
        [HttpGet]
        public ActionResult<Spreadsheet<string>[]> GetList(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            return group == null ? Ok(spreadsheetService.GetAll(user)) : Ok(spreadsheetService.GetByGroup(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Gets the cell values in the given named range in the given sheet in the given spreadsheet.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The spreadsheet identifier.</param>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="rangeName">Name of the range.</param>
        /// <returns>ActionResult.</returns>
        [HttpGet("{id}/{sheetName}/namedrange;{rangeName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<object[,]> GetNamedRangeValues(string connectionId, string id, string sheetName, string rangeName)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            return Ok(spreadsheetService.GetNamedRange(FullNameString.FromUrl(id), sheetName, rangeName, user));
        }

        /// <summary>
        ///     Gets  the cell values in the given range in the given sheet in the given spreadsheet. 
        /// </summary>
        /// <remarks>
        ///     A range is given in the format {cell1},{cell2}. The row and column indices are zero-based.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The spreadsheet identifier.</param>
        /// <param name="sheetName">Name of the sheet.</param>
        /// <param name="range">
        ///     The range. A range is given in the format {cell1},{cell2}. The row and column indices are zero-based.
        /// </param>
        [HttpGet("{id}/{sheetName}/range;{range}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<object[,]> GetRangeValues(string connectionId, string id, string sheetName, string range)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            return Ok(spreadsheetService.GetRange(FullNameString.FromUrl(id), sheetName, Range.Parse(range), user));
        }

        /// <summary>
        ///     Gets the cell values in the used range in the given sheet in the given spreadsheet.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The spreadsheet identifier.</param>
        /// <param name="sheetName">Name of the sheet.</param>
        [HttpGet("{id}/{sheetName}/usedrange")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<object[,]> GetUsedRangeValues(string connectionId, string id, string sheetName)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            return Ok(spreadsheetService.GetUsedRange(FullNameString.FromUrl(id), sheetName, user));
        }

        /// <summary>
        ///     Gets the cell value formats of the used range in the given sheet in the given spreadsheet.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The spreadsheet identifier.</param>
        /// <param name="sheetName">Name of the sheet.</param>
        [HttpGet("{id}/{sheetName}/formats/usedrange")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<object[,]> GetUsedRangeFormats(string connectionId, string id, string sheetName)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            return Ok(spreadsheetService.GetUsedRangeFormats(FullNameString.FromUrl(id), sheetName, user));
        }

        /// <summary>
        ///     Downloads the spreadsheet with the given ID as a file.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The spreadsheet identifier.</param>
        [HttpGet("stream/{id}")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/vnd.ms-excel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        public IActionResult GetStream(string connectionId, string id)
        {
            var user = HttpContext.User;
            var spreadsheetService = Services.Get<SpreadsheetService>(connectionId);
            var (stream, fileType, fileName) = spreadsheetService.GetStream(FullNameString.FromUrl(id), user);
            string contentType;
            switch (fileType)
            {
                case "xlsx":
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case "xls":
                    contentType = "application/vnd.ms-excel";
                    break;
                default:
                   throw new  NotSupportedException($"{nameof(fileType)} of type '{fileType}' is not supported.");                    
            }

            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, contentType, fileName);
        }
    }
}