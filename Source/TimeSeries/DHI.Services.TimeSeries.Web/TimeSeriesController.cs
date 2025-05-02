namespace DHI.Services.TimeSeries.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using System.Web.Http.Description;
    using Microsoft.Web.Http;
    using WebApi.OutputCache.V2;

    /// <summary>
    ///     Time Series API
    /// </summary>
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/timeseries/{connectionId}")]
    [ControllerExceptionFilter]
    [CacheTimeSeriesOutput(MustRevalidate = true)]
    [AutoInvalidateCacheOutput]
    [ApiVersion("1")]
    public class TimeSeriesController : ApiController
    {
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
        [Route("")]
        [ValidateModel]
        [Authorize(Roles = "Editor")]
        [HttpPost]
        [ResponseType(typeof(TimeSeries<string, double>))]
        public IHttpActionResult Add(string connectionId, [FromBody] TimeSeriesDTO timeSeriesDTO)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<IUpdatableTimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesDTO.ToTimeSeries();
            timeSeriesService.Add(timeSeries, user);
            return Created($"{Request.RequestUri}/{FullNameString.ToUrl(timeSeries.Id)}", timeSeries);
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
        [Route("")]
        [ValidateModel]
        [HttpPut]
        [Authorize(Roles = "Editor")]
        [ResponseType(typeof(TimeSeries<string, double>))]
        public IHttpActionResult Update(string connectionId, [FromBody] TimeSeriesDTO timeSeriesDTO)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<IUpdatableTimeSeriesService<string, double>>(connectionId);
            var timeSeries = timeSeriesDTO.ToTimeSeries();
            timeSeriesService.Update(timeSeries, user);
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
        [Route("{id}/values")]
        [ValidateModel]
        [HttpPut]
        [Authorize(Roles = "Editor")]
        [ResponseType(typeof(TimeSeries<string, double>))]
        public IHttpActionResult SetValues(string connectionId, string id, [FromBody] TimeSeriesDataDTO timeSeriesDataDTO)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<IDiscreteTimeSeriesService<string, double>>(connectionId);
            var timeSeriesData = timeSeriesDataDTO.ToTimeSeriesData();
            var timeSeriesId = FullNameString.FromUrl(id);
            timeSeriesService.SetValues(timeSeriesId, timeSeriesData, user);
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
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}")]
        [Authorize(Roles = "Editor")]
        public void Delete(string connectionId, string id)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<IUpdatableTimeSeriesService<string, double>>(connectionId);
            timeSeriesService.Remove(FullNameString.FromUrl(id), user);
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
        [Route("{id}/values")]
        [Authorize(Roles = "Editor")]
        public void DeleteValues(string connectionId, string id, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<IUpdatableTimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                timeSeriesService.RemoveValues(FullNameString.FromUrl(id), user:user);
            }
            else
            {
                var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
                var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
                timeSeriesService.RemoveValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user);
            }
        }

        /// <summary>
        ///     Gets the time series with the specified identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}")]
        [ResponseType(typeof(TimeSeries<string, double>))]
        public IHttpActionResult Get(string connectionId, string id)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.Get(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets the total number of time series.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [Route("count")]
        [ResponseType(typeof(int))]
        public IHttpActionResult GetCount(string connectionId)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
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
        [Route("{id}/datetimes")]
        [ResponseType(typeof(SortedSet<DateTime>))]
        public IHttpActionResult GetDateTimes(string connectionId, string id)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetDateTimes(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets the first date timefor the time series with the given identifier.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}/datetime/first")]
        [ResponseType(typeof(DateTime?))]
        public IHttpActionResult GetFirstDateTime(string connectionId, string id)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
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
        /// <response code="404">Time series not found</response>
        [Route("{id}/value/first")]
        [ResponseType(typeof(DataPoint<double>))]
        public IHttpActionResult GetFirstValue(string connectionId, string id)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetFirstValue(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets the first value (corresponding to the first time step) for the given list of time series
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series ids.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("list/value/first")]
        [HttpPost]
        [ResponseType(typeof(IDictionary<string, DataPoint<double>>))]
        public IHttpActionResult GetFirstValueList(string connectionId, [FromBody] string[] ids)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
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
        /// <param name="dateTimeString">The datetime string.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}/value/firstafter/{dateTimeString:date}")]
        [ResponseType(typeof(DataPoint<double>))]
        public IHttpActionResult GetFirstValueAfter(string connectionId, string id, string dateTimeString)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetFirstValueAfter(FullNameString.FromUrl(id), Datetime.Parse(dateTimeString), user));
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
        [Route("fullnames")]
        [ResponseType(typeof(IEnumerable<string>))]
        public IHttpActionResult GetFullNames(string connectionId, string group = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<IGroupedService<TimeSeries<string, double>>>(connectionId);
            return group == null ? Ok(timeSeriesService.GetFullNames(user)) : Ok(timeSeriesService.GetFullNames(group, user));
        }

        /// <summary>
        ///     Gets a list of all time series IDs.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [Route("ids")]
        [ResponseType(typeof(IEnumerable<string>))]
        public IHttpActionResult GetIds(string connectionId)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
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
        /// <response code="404">Time series not found</response>
        [Route("{id}/datetime/last")]
        [ResponseType(typeof(DateTime?))]
        public IHttpActionResult GetLastDateTime(string connectionId, string id)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
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
        /// <response code="404">Time series not found</response>
        [Route("{id}/value/last")]
        [ResponseType(typeof(DataPoint<double>))]
        public IHttpActionResult GetLastValue(string connectionId, string id)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetLastValue(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets the last value (corresponding to the last time step) for the given list of time series
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The time series ids.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("list/value/last")]
        [HttpPost]
        [ResponseType(typeof(IDictionary<string, DataPoint<double>>))]
        public IHttpActionResult GetLastValueList(string connectionId, [FromBody] string[] ids)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
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
        /// <param name="dateTimeString">The datetime string.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}/value/lastbefore/{dateTimeString:date}")]
        [ResponseType(typeof(DataPoint<double>))]
        public IHttpActionResult GetLastValueBefore(string connectionId, string id, string dateTimeString)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetLastValueBefore(FullNameString.FromUrl(id), Datetime.Parse(dateTimeString), user));
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
        [Route("list")]
        [ResponseType(typeof(IEnumerable<TimeSeries<string, double>>))]
        public IHttpActionResult GetList(string connectionId, string group = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<IDiscreteTimeSeriesService<string, double>>(connectionId);
            return group == null ? Ok(timeSeriesService.GetAll(user)) : Ok(((IGroupedService<TimeSeries<string, double>>)timeSeriesService).GetByGroup(group, user));
        }

        /// <summary>
        ///     Gets a list of time series within the given list of groups (recursive).
        /// </summary>
        /// <remarks>
        ///     NOTE: This is only applicable if the connection type is grouped (hierarchical).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="groups">The list of groups.</param>
        [Route("list")]
        [HttpPost]
        [ResponseType(typeof(IEnumerable<TimeSeries<string, double>>))]
        public IHttpActionResult GetListByGroups(string connectionId, [FromBody] string[] groups)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<IDiscreteTimeSeriesService<string, double>>(connectionId);
            return Ok(((IGroupedService<TimeSeries<string, double>>)timeSeriesService).GetByGroups(groups, user));
        }

        /// <summary>
        ///     Gets the value at the given datetime for the given time series.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="dateTimeString">The datetime string.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("{id}/value/{dateTimeString:date}")]
        [ResponseType(typeof(DataPoint<double>))]
        public IHttpActionResult GetValue(string connectionId, string id, string dateTimeString)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            return Ok(timeSeriesService.GetValue(FullNameString.FromUrl(id), Datetime.Parse(dateTimeString), user));
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
        /// <response code="404">Time series not found</response>
        [Route("{id}/values")]
        public IHttpActionResult GetValues(string connectionId, string id, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), user:user));
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(id), fromDateTime, toDateTime, user));
        }

        /// <summary>
        ///     Gets a list of values within the given time interval for the given list of time series.
        /// </summary>
        /// <remarks>
        ///     All combinations of the optional from and to parameters are possible.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The ids.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Time series not found</response>
        [Route("list/values")]
        [HttpPost]
        public IHttpActionResult GetValuesList(string connectionId, [FromBody] string[] ids, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(ids), user:user));
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            return Ok(timeSeriesService.GetValues(FullNameString.FromUrl(ids), fromDateTime, toDateTime, user));
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
        /// <response code="404">Time series not found</response>
        [Route("vectors")]
        [HttpPost]
        public IHttpActionResult GetVectors(string connectionId, [FromBody] ComponentsDTO componentsDTO, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetVectors(FullNameString.FromUrl(componentsDTO.X), FullNameString.FromUrl(componentsDTO.Y), user: user));
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
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
        /// <response code="404">Time series not found</response>
        [Route("list/vectors")]
        [HttpPost]
        public IHttpActionResult GetVectorsList(string connectionId, [FromBody] ComponentsDTO[] componentsDTOList, string from = null, string to = null)
        {
            var user = (ClaimsPrincipal)RequestContext.Principal;
            var timeSeriesService = Services.Get<ITimeSeriesService<string, double>>(connectionId);
            var componentsList = componentsDTOList.Select(componentsDTO => componentsDTO.ToValueTuple()).ToList();

            if (from == null && to == null)
            {
                return Ok(timeSeriesService.GetVectors(componentsList.ToArray(), user: user));
            }

            var fromDateTime = from == null ? DateTime.MinValue : Datetime.Parse(from);
            var toDateTime = to == null ? DateTime.MaxValue : Datetime.Parse(to);
            return Ok(timeSeriesService.GetVectors(componentsList.ToArray(), fromDateTime, toDateTime, user));
        }
    }
}