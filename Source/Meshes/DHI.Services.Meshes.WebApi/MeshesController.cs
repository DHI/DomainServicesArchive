namespace DHI.Services.Meshes.WebApi
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using TimeSeries;
    using WebApiCore;
    using Spatial;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    ///     Meshes API
    /// </summary>
    [Produces("application/json")]
    [Route("api/meshes/{connectionId}")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for querying data from meshes.")]
    public class MeshesController : ControllerBase
    {
        /// <summary>
        ///     Gets a list of available meshes within the given group.
        ///     If no group is given, all available meshes are returned.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<MeshInfo>> GetList(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            return group == null ? Ok(meshService.GetAll(user)) :
                Ok(((IGroupedMeshService)meshService).GetByGroup(FullNameString.FromUrl(group)));
        }

        /// <summary>
        ///     Gets the count of available meshes.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount(string connectionId)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            return Ok(meshService.Count(user));
        }

        /// <summary>
        ///     Gets information about the specified mesh.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Mesh not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The mesh identifier.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<MeshInfo> Get(string connectionId, string id)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            return Ok(meshService.Get(FullNameString.FromUrl(id), user));
        }

        /// <summary>
        ///     Gets a list of all mesh IDs.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds(string connectionId)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            return Ok(meshService.GetIds(user));
        }

        /// <summary>
        ///     Gets all fullname identifiers.
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
            var gisService = Services.Get<IGroupedMeshService<string>>(connectionId);
            return group == null ? Ok(gisService.GetFullNames(user)) :
                Ok(gisService.GetFullNames(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Gets available date times for the specified mesh.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Mesh not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The mesh identifier.</param>
        [HttpGet("{id}/datetimes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<DateTime>> GetDateTimes(string connectionId, string id)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            return Ok(meshService.GetDateTimes(id, user));
        }

        /// <summary>
        ///     Returns a time series of raw values at the specified location (point) for the specified item within the specified time interval.
        /// </summary>
        /// <remarks>
        ///     All combinations of the optional from and to parameters are possible.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="404">Mesh not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="item">The data item.</param>
        /// <param name="point">The location (point) from where to extract data. In <a href="https://datatracker.ietf.org/doc/html/rfc7946#appendix-A.1">GeoJSON format</a> </param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpPost("{id}/{item}/values")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<ITimeSeriesData<double>> GetValues(string connectionId, string id, string item, [FromBody, Required] Point point, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            return Ok(meshService.GetValues(id, item, point, new DateRange(from, to), user));
        }

        /// <summary>
        ///     Returns a dictionary of time series (key=item) of raw values at the specified location (point) for all items within the specified time interval. 
        /// </summary>
        /// <remarks>
        ///     All combinations of the optional from and to parameters are possible.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="404">Mesh not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="point">The location (point) from where to extract data. In <a href="https://datatracker.ietf.org/doc/html/rfc7946#appendix-A.1">GeoJSON format</a> </param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpPost("{id}/values")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<Dictionary<string, ITimeSeriesData<double>>> GetValuesForAllItems(string connectionId, string id, [FromBody, Required] Point point, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            return Ok(meshService.GetValues(id, point, new DateRange(from, to), user));
        }

        /// <summary>
        ///     Returns a time series of aggregated values (across the entire mesh) for the specified item within the specified time interval.
        /// </summary>
        /// <remarks>
        ///     All combinations of the optional from and to parameters are possible.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="404">Mesh not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="item">The data item.</param>
        /// <param name="aggregation">The aggregation type (Minimum|Maximum|Average|Sum)</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("{id}/{item}/{aggregation}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ITimeSeriesData<double>> GetAggregatedValues(string connectionId, string id, string item, string aggregation, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            var aggregationType = Enumeration.FromDisplayName<AggregationType>(aggregation);
            return Ok(meshService.GetAggregatedValues(id, aggregationType, item, new DateRange(from, to), user));
        }

        /// <summary>
        ///     Returns a time series of aggregated values within the specified polygon for the specified item within the specified time interval.
        /// </summary>
        /// <remarks>
        ///     All combinations of the optional from and to parameters are possible.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="404">Mesh not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="item">The data item.</param>
        /// <param name="aggregation">The aggregation type (Minimum|Maximum|Average|Sum).</param>
        /// <param name="polygon">The area (polygon) from which to extract data. In <a href="https://datatracker.ietf.org/doc/html/rfc7946#appendix-A.3">GeoJSON format</a> </param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpPost("{id}/{item}/{aggregation}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<ITimeSeriesData<double>> GetAggregatedValuesWithinPolygon(string connectionId, string id, string item, string aggregation, [FromBody, Required] Polygon polygon, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            var aggregationType = Enumeration.FromDisplayName<AggregationType>(aggregation);
            return Ok(meshService.GetAggregatedValues(id, aggregationType, item, polygon, new DateRange(from, to), user));
        }

        /// <summary>
        ///     Returns a time series of aggregated values (across the entire mesh) for the specified item grouped by the given period (hourly, daily etc.) within the specified time interval.
        /// </summary>
        /// <remarks>
        ///     All combinations of the optional from and to parameters are possible.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="404">Mesh not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="item">The data item.</param>
        /// <param name="aggregation">The aggregation type (Minimum|Maximum|Average|Sum).</param>
        /// <param name="period">Grouping period.</param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpGet("{id}/{item}/{aggregation}/period/{period}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ITimeSeriesData<double>> GetAggregatedValuesByPeriod(string connectionId, string id, string item, string aggregation, Period period, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            var aggregationType = Enumeration.FromDisplayName<AggregationType>(aggregation);
            return Ok(meshService.GetAggregatedValues(id, aggregationType, item, period, new DateRange(from, to), user));
        }

        /// <summary>
        ///     Returns a time series of aggregated values within the specified polygon for the specified item grouped by the given period (hourly, daily etc.) within the specified time interval.
        /// </summary>
        /// <remarks>
        ///     All combinations of the optional from and to parameters are possible.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="404">Mesh not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="item">The data item.</param>
        /// <param name="aggregation">The aggregation type (Minimum|Maximum|Average|Sum).</param>
        /// <param name="period">Grouping period.</param>
        /// <param name="polygon">The area (polygon) from which to extract data. In <a href="https://datatracker.ietf.org/doc/html/rfc7946#appendix-A.3">GeoJSON format</a> </param>
        /// <param name="from">From datetime.</param>
        /// <param name="to">To datetime.</param>
        [HttpPost("{id}/{item}/{aggregation}/period/{period}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<ITimeSeriesData<double>> GetAggregatedValuesWithinPolygonByPeriod(string connectionId, string id, string item, string aggregation, Period period, [FromBody, Required] Polygon polygon, DateTime? from = null, DateTime? to = null)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            var aggregationType = Enumeration.FromDisplayName<AggregationType>(aggregation);
            return Ok(meshService.GetAggregatedValues(id, aggregationType, item, polygon, period, new DateRange(from, to), user));
        }

        /// <summary>
        ///     Returns the aggregated value (across the entire mesh) for the specified item at the specified time.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Mesh not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="item">The data item.</param>
        /// <param name="aggregation">The aggregation type (Minimum|Maximum|Average|Sum).</param>
        /// <param name="dateTime">The datetime.</param>
        [HttpGet("{id}/{item}/{aggregation}/{dateTime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double> GetAggregatedValue(string connectionId, string id, string item, string aggregation, DateTime dateTime)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            var aggregationType = Enumeration.FromDisplayName<AggregationType>(aggregation);
            return Ok(meshService.GetAggregatedValue(id, aggregationType, item, dateTime, user));
        }

        /// <summary>
        ///     Returns the aggregated value within the specified polygon for the specified item at the specified time.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Mesh not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="item">The data item.</param>
        /// <param name="aggregation">The aggregation type (Minimum|Maximum|Average|Sum).</param>
        /// <param name="polygon">The area (polygon) from which to extract data. In <a href="https://datatracker.ietf.org/doc/html/rfc7946#appendix-A.3">GeoJSON format</a> </param>
        /// <param name="dateTime">The datetime.</param>
        [HttpPost("{id}/{item}/{aggregation}/{dateTime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<double> GetAggregatedValueWithinPolygon(string connectionId, string id, string item, string aggregation, [FromBody, Required] Polygon polygon, DateTime dateTime)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            var aggregationType = Enumeration.FromDisplayName<AggregationType>(aggregation);
            return Ok(meshService.GetAggregatedValue(id, aggregationType, item, polygon, dateTime, user));
        }

        /// <summary>
        ///     Returns a feature collection representing contour lines for the specified item at the specified time.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Mesh not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="item">The data item.</param>
        /// <param name="thresholdValues">The contour threshold values.</param>
        /// <param name="dateTime">The datetime.</param>
        [HttpPost("contours/{id}/{item}/{dateTime}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<IFeatureCollection> GetContours(string connectionId, string id, string item, [FromBody, Required] double[] thresholdValues, DateTime dateTime)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);
            var featureCollection = meshService.GetContours(id, item, thresholdValues, dateTime, user);
            return Ok(featureCollection);
        }
        /// <summary>
        ///     Returns a list of feature collections representing contour lines for the specified item in the specified time range.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Mesh not found</response>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="id">The mesh identifier.</param>
        /// <param name="item">The data item.</param>
        /// <param name="thresholdValues">The contour threshold values.</param>
        /// <param name="from">From dateTime.</param>
        /// <param name="to">To dateTime.</param>
        [HttpPost("contours/{id}/{item}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<List<IFeatureCollection>> GetContoursInRange(string connectionId, string id, string item, [FromBody, Required] double[] thresholdValues, [FromQuery, Required] DateTime from, [FromQuery, Required] DateTime to)
        {
            var user = HttpContext.User;
            var meshService = Services.Get<IMeshService>(connectionId);

            var dates = Enumerable.Range(0, 1 + (int)to.Subtract(from).TotalHours)
                .Select(offset => from.AddHours(offset))
                .ToDictionary(x => x.Ticks, x => x);

            var results = new Dictionary<long, IFeatureCollection>();

            Parallel.ForEach(Partitioner.Create(dates, EnumerablePartitionerOptions.NoBuffering), date =>
            {
                results.Add(date.Key, meshService.GetContours(id, item, thresholdValues, date.Value, user));
            });

            var returnObj = results.OrderBy(x => x.Key)
                .Select(x => x.Value)
                .ToList();

            return Ok(returnObj);
        }
    }
}
