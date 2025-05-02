namespace DHI.Services.TimeSeries.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Authorization;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;
    using Swashbuckle.AspNetCore.Annotations;
    using WebApiCore;

    /// <summary>
    ///     Time Series API
    /// </summary>
    [Produces("application/json")]
    [Route("api/timeseries/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing and retrieving time series and time series data.")]
    public class TimeSeriesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public TimeSeriesController(ILogger<TimeSeriesController> logger, IHubContext<NotificationHub> hubContext = null)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <summary>
        ///     Adds a new time series.
        /// </summary>
        /// <remarks>
        ///     The only required information in the request body is the FullName. However, some connection types might require
        ///     additional information such as DataType, Quantity and Unit.
        ///     If the connection type is not a grouped (hierarchical) connection type, the FullName should be a simple flat time
        ///     series ID.
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="timeSeriesDTO">The time series dto.</param>
        /// <response code="201">Created</response>
        [Authorize(Policy = "EditorsOnly")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public async Task<ActionResult<TimeSeries<string, double>>> Add(string connectionId, [FromBody] TimeSeriesDTO timeSeriesDTO)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<IUpdatableTimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesDTO.ToTimeSeries();
            timeSeriesService.Add(timeSeries, user);
            var parameters = new Parameters
            {
                { "id", timeSeries.Id },
                { "data", JsonSerializer.Serialize(timeSeries) },
                { "userName", user.GetUserId() }
            };

            try
            {
                if (_hubContext != null)
                {
                    await _hubContext.Clients.All.SendAsync("TimeSeriesAdded", parameters);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not send signalr message to all client");
            }

            return CreatedAtAction(nameof(Get), new { connectionId, id = FullNameString.ToUrl(timeSeries.Id) }, timeSeries);
        }

        /// <summary>
        ///     Updates an existing time series.
        /// </summary>
        /// <remarks>
        ///     The only required information in the request body is the FullName.
        ///     If the connection type is not a grouped (hierarchical) connection type, the FullName should be a simple flat time
        ///     series ID.
        ///     The updated data and/or meta data will depend on the other information given in the request body.
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="timeSeriesDTO">The time series dto.</param>
        [HttpPut]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public async Task<ActionResult<TimeSeries<string, double>>> Update(string connectionId, [FromBody] TimeSeriesDTO timeSeriesDTO)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<IUpdatableTimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesDTO.ToTimeSeries();
            timeSeriesService.Update(timeSeries, user);
            var parameters = new Parameters
            {
                { "id", timeSeries.Id },
                { "data", JsonSerializer.Serialize(timeSeries) },
                { "userName", user.GetUserId() }
            };

            try
            {
                if (_hubContext != null)
                {
                    await _hubContext.Clients.All.SendAsync("TimeSeriesUpdated", parameters);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not send signalr message to all client");
            }

            return Ok(timeSeriesService.Get(timeSeries.Id, user));
        }

        /// <summary>
        ///     Sets some time series values for the time series with the given ID.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="timeSeriesDataDTO">The time series data dto.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [HttpPut("{id}/values")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public async Task<ActionResult<TimeSeries<string, double>>> SetValues(string connectionId, string id, [FromBody] TimeSeriesDataDTO timeSeriesDataDTO)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<IDiscreteTimeSeriesService<string, double>>(connectionId);
            var timeSeriesData = timeSeriesDataDTO.ToTimeSeriesData();
            var timeSeriesId = FullNameString.FromUrl(id);
            timeSeriesService.SetValues(timeSeriesId, timeSeriesData, user);
            var parameters = new Parameters
            {
                { "id", id },
                { "data", JsonSerializer.Serialize(timeSeriesData) },
                { "userName", user.GetUserId() }
            };

            try
            {
                if (_hubContext != null)
                {
                    await _hubContext.Clients.All.SendAsync("TimeSeriesValuesSet", parameters);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not send signalr message to all client");
            }

            return Ok(timeSeriesService.Get(timeSeriesId, user));
        }

        /// <summary>
        ///     Deletes a time series with the given ID.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="204">No Content. Successfully deleted.</response>
        /// <response code="404">Time series not found.</response>
        [HttpDelete("{id}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string connectionId, string id)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<IUpdatableTimeSeriesService<string, double>>(connectionId);
            var timeSeriesId = FullNameString.FromUrl(id);
            timeSeriesService.Remove(timeSeriesId, user);
            var parameters = new Parameters
            {
                { "id", timeSeriesId },
                { "userName", user.GetUserId() }
            };

            try
            {
                if (_hubContext != null)
                {
                    await _hubContext.Clients.All.SendAsync("TimeSeriesDeleted", parameters);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not send signalr message to all client");
            }

            return NoContent();
        }

        /// <summary>
        ///     Deletes all time series within the given group.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is grouped and updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The identifier.</param>
        /// <response code="204">No Content. Group successfully deleted.</response>
        /// <response code="404">Time series not found.</response>
        [HttpDelete("group/{group}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteByGroup(string connectionId, string group)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<IGroupedUpdatableService>(connectionId);
            var groupId = FullNameString.FromUrl(group);
            timeSeriesService.RemoveByGroup(groupId, user);
            var parameters = new Parameters
            {
                { "group", groupId },
                { "userName", user.GetUserId() }
            };

            try
            {
                if (_hubContext != null)
                {
                    await _hubContext.Clients.All.SendAsync("TimeSeriesGroupDeleted", parameters);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not send signalr message to all client");
            }

            return NoContent();
        }

        /// <summary>
        ///     Deletes the time series values for the given time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Time series not found</response>
        [HttpDelete("{id}/values")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteValues(string connectionId, string id, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<IUpdatableTimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                timeSeriesService.RemoveValues(FullNameString.FromUrl(id), user: user);
            }
            else
            {
                var fromDateTime = from ?? DateTime.MinValue;
                var toDateTime = to ?? DateTime.MaxValue;
                timeSeriesService.RemoveValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user);
            }

            return NoContent();
        }

        /// <summary>
        ///     Gets the time series with the specified identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<TimeSeries<string, double>> Get(string connectionId, string id)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesService.Get(FullNameString.FromUrl(id), user);
            return Ok(timeSeries);
        }

        /// <summary>
        ///     Gets the total number of time series.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount(string connectionId)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<IDiscreteTimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.Count(user));
        }

        /// <summary>
        ///     Gets a list of all datetimes for the time series with the given identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [HttpGet("{id}/datetimes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<SortedSet<DateTime>> GetDateTimes(string connectionId, string id)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetDateTimes(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets the first date time for the time series with the given identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series not found.</response>
        [HttpGet("{id}/datetime/first")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<DateTime?> GetFirstDateTime(string connectionId, string id)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetFirstDateTime(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets the first value (corresponding to the first time step) for the given time series.
        /// </summary>
        /// <remarks>
        ///     If the time series does not contain any data, null is returned.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series not found</response>
        [HttpGet("{id}/value/first")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<DataPoint<double>> GetFirstValue(string connectionId, string id)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetFirstValue(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets the first value (corresponding to the first time step) for the given list of time series
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series ids.</param>
        /// <response code="200">OK</response>
        [HttpPost("list/value/first")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<IDictionary<string, DataPoint<double>>> GetFirstValueList(string connectionId, [FromBody] string[] ids)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetFirstValue(FullNameString.FromUrl(ids), user));
        }

        /// <summary>
        ///     Returns the first value after the given datetime for the given time series.
        /// </summary>
        /// <remarks>
        ///     A datetime value is given as a modified ISO 8601 format (with the ":" separators in the time specification
        ///     omitted).
        ///     If the time series does not contain any data, null is returned.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="date">The datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series not found</response>
        [HttpGet("{id}/value/firstafter/{date:datetime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<DataPoint<double>> GetFirstValueAfter(string connectionId, string id, DateTime date)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetFirstValueAfter(FullNameString.FromUrl(id), date, user));
        }

        /// <summary>
        ///     Gets a list of time series full-name identifiers.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is grouped (hierarchical).
        ///     If a group is given, a list of time series full-name identifiers within the given group (recursive) is retrieved.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        [HttpGet("fullnames")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetFullNames(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<IGroupedService<TimeSeries<string, double>>>(connectionId);
            return group == null ? Ok(timeSeriesService.GetFullNames(user)) : Ok(timeSeriesService.GetFullNames(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Gets a list of all time series IDs.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds(string connectionId)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<IDiscreteTimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetIds(user));
        }

        /// <summary>
        ///     Gets the last datetime for the given time series.
        /// </summary>
        /// <remarks>
        ///     If the time series does not contain any data, null is returned.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series not found.</response>
        [HttpGet("{id}/datetime/last")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<DateTime?> GetLastDateTime(string connectionId, string id)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetLastDateTime(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets the last value (corresponding to the last time step) for the given time series.
        /// </summary>
        /// <remarks>
        ///     If the time series does not contain any data, null is returned.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series not found</response>
        [HttpGet("{id}/value/last")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<DataPoint<double>> GetLastValue(string connectionId, string id)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetLastValue(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets the last value (corresponding to the last time step) for the given list of time series
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series ids.</param>
        /// <response code="200">OK</response>
        [HttpPost("list/value/last")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<IDictionary<string, DataPoint<double>>> GetLastValueList(string connectionId, [FromBody] string[] ids)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetLastValue(FullNameString.FromUrl(ids), user));
        }

        /// <summary>
        ///     Gets the last value before the given datetime for the given time series.
        /// </summary>
        /// <remarks>
        ///     If the time series does not contain any data, null is returned.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="date">The datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series not found</response>
        [HttpGet("{id}/value/lastbefore/{date:datetime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<DataPoint<double>> GetLastValueBefore(string connectionId, string id, DateTime date)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetLastValueBefore(FullNameString.FromUrl(id), date, user));
        }

        /// <summary>
        ///     Gets a list of time series.
        /// </summary>
        /// <remarks>
        ///     If no group is given, a list of all time series is retrieved.
        ///     If a group is given, a list of time series within the given group (recursive) is retrieved.
        ///     This is only applicable if the connection type is grouped (hierarchical).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<TimeSeries<string, double>>> GetList(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<IDiscreteTimeSeriesService<string, double>>(connectionId);
            return group == null ? Ok(timeSeriesService.GetAll(user)) : Ok(((IGroupedService<TimeSeries<string, double>>)timeSeriesService).GetByGroup(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Gets a list of time series for the specified identifiers.
        /// </summary>
        /// <remarks>
        ///     The identifiers must be either groups or time series fullnames.
        ///     If the identifiers are fullnames, the fullnames query parameter must be set to true, otherwise groups are assumed.
        ///     NOTE: Using groups is only applicable if the connection type is grouped (hierarchical).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series groups or fullnames.</param>
        /// <param name="fullnames">if true, the specified ids are treated as time series fullnames, otherwise as groups.</param>
        [HttpPost("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public ActionResult<IEnumerable<TimeSeries<string, double>>> GetListByIds(string connectionId, [FromBody] string[] ids, bool fullnames = false)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(fullnames ? timeSeriesService.Get(ids, user) : ((IGroupedService<TimeSeries<string, double>>)timeSeriesService).GetByGroups(ids, user));
        }

        /// <summary>
        ///     Gets the value at the given datetime for the given time series.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="date">The datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="204">No content. Time series ID or dateTime not found.</response>
        [HttpGet("{id}/value/{date:datetime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<DataPoint<double>> GetValue(string connectionId, string id, DateTime date)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ICoreTimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetValue(FullNameString.FromUrl(id), date, user));
        }

        /// <summary>
        ///     Gets a list of all values for the given time series within the given time interval.
        /// </summary>
        /// <remarks>
        ///     All combinations of the optional from and to parameters are possible.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpGet("{id}/values")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<object[][]> GetValues(string connectionId, string id, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            if (from == null && to == null)
            {
                var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user: user));
            }

            var coreTimeSeriesService = Services.Get<ICoreTimeSeriesService<string, double>>(connectionId);
            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(coreTimeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user));
        }

        /// <summary>
        ///     Gets a list of values within the given time interval for the given list of time series.
        /// </summary>
        /// <remarks>
        ///     All combinations of the optional from and to parameters are possible.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The identifiers for the timeseries to retrieve values from.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <param name="distinctdatetime">Set to true if you want the response formatted with distinct time steps.</param>
        /// <response code="200">OK</response>
        [HttpPost("list/values")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public IActionResult GetValuesList(string connectionId, [FromBody] string[] ids, DateTime? from = null, DateTime? to = null, bool distinctdatetime = false)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ICoreTimeSeriesService<string, double>>(connectionId);

            if (!distinctdatetime)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(ids), from ?? DateTime.MinValue, to ?? DateTime.MaxValue, user));
            }

            var timeSeriesDataList = ids.Select(id => timeSeriesService.GetValues(FullNameString.FromUrl(id), from ?? DateTime.MinValue, to ?? DateTime.MaxValue, user)).ToArray();
            var result = timeSeriesDataList
                .SelectMany(x => x.DateTimes)
                .OrderBy(dateTime => dateTime)
                .Distinct()
                .Select(dateTime => new object[] { dateTime }.Concat(timeSeriesDataList.Select(x => x.ContainsDateTime(dateTime) ? (object)x.Get(dateTime).Value.Value : null)).ToArray())
                .ToArray();

            return Ok(result);
        }

        /// <summary>
        ///     Gets a list of vectors within the given time interval for the given X- and Y-component time series.
        /// </summary>
        /// <remarks>
        ///     All combinations of the optional from and to parameters are possible.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="componentsDTO">The component time series ids.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpPost("vectors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public IActionResult GetVectors(string connectionId, [FromBody] ComponentsDTO componentsDTO, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetVectors(FullNameString.FromUrl(componentsDTO.X), FullNameString.FromUrl(componentsDTO.Y), user: user));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetVectors(FullNameString.FromUrl(componentsDTO.X), FullNameString.FromUrl(componentsDTO.Y), fromDateTime, toDateTime, user));
        }

        /// <summary>
        ///     Gets a list of vectors within the given time interval for the given list of X- and Y-component time series.
        /// </summary>
        /// <remarks>
        ///     All combinations of the optional from and to parameters are possible.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="componentsDTOList">The list of component time series ids.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        [HttpPost("list/vectors")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public IActionResult GetVectorsList(string connectionId, [FromBody] ComponentsDTO[] componentsDTOList, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var componentsList = componentsDTOList.Select(componentsDTO => componentsDTO.ToValueTuple()).ToList();

            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetVectors(componentsList.ToArray(), user: user));
            }

            var fromDateTime = from ?? DateTime.MinValue;
            var toDateTime = to ?? DateTime.MaxValue;
            return Ok(timeSeriesService.GetVectors(componentsList.ToArray(), fromDateTime, toDateTime, user));
        }
    }
}