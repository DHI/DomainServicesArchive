using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("DHI.Services.GIS.WebApi.Test")]
[assembly: InternalsVisibleTo("DHI.Services.GIS.WebApi.Host.Test")]

namespace DHI.Services.GIS.WebApi
{
    using DHI.Services.Filters;
    using DHI.Spatial.Data;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Spatial;
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using WebApiCore;
    using Attribute = Spatial.Attribute;
    using FeatureCollection = GIS.FeatureCollection;

    /// <summary>
    ///     Feature Collections API
    /// </summary>
    [Produces("application/json")]
    [Route("api")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing and retrieving vector-based GIS data.")]
    public class GISController : ControllerBase
    {
        /// <summary>
        ///     Gets the feature collection with the specified identifier.
        /// </summary>
        /// <remarks>
        ///     If the provider supports associations of time series, spreadsheets etc., these associations will be included in the
        ///     response if the associations parameter is set to true.
        ///     The result can be filtered using query string parameters.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="associations">if set to <c>true</c> associations are returned.</param>
        /// <param name="outSpatialReference">The output spatial reference.</param>
        /// <response code="200">OK</response>
        /// <response code="404">feature collection not found</response>
        [HttpGet("featurecollections/{connectionId}/{featureCollectionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string connectionId, string featureCollectionId, bool associations = false, string outSpatialReference = null)
        {
            var user = HttpContext.User;
            var filter = new List<QueryCondition>();
            foreach (var condition in Request.Query)
            {
                if (condition.Key == "associations")
                {
                    associations = condition.Value == "1" || condition.Value == "true";
                    continue;
                }

                if (condition.Key == "outSpatialReference")
                {
                    outSpatialReference = condition.Value;
                    continue;
                }

                var queryCondition = new QueryCondition(condition.Key, condition.Value);
                filter.Add(queryCondition);
            }

            var gisService = Services.Get<IGisService<string>>(connectionId);

            FeatureCollection<string> featureCollection;
            if (filter.Count > 0)
            {
                featureCollection = gisService.Get(FullNameString.FromUrl(featureCollectionId), filter, associations, outSpatialReference, user);
            }
            else
            {
                featureCollection = gisService.Get(FullNameString.FromUrl(featureCollectionId), associations, outSpatialReference, user);
            }

            return Ok(featureCollection);
        }

        /// <summary>
        ///     Gets a list of feature collections with the specified identifiers
        /// </summary>
        /// <remarks>
        ///     If the provider supports associations of time series, spreadsheets etc., these associations will be included in the
        ///     response if the associations parameter is set to true.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="ids">The feature collection IDs.</param>
        /// <param name="associations">if set to <c>true</c> associations are returned.</param>
        /// <param name="outSpatialReference">The output spatial reference.</param>
        /// <response code="200">OK</response>
        /// <response code="404">feature collection not found</response>
        [HttpPost("featurecollections/{connectionId}/list")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetListByIds(string connectionId, [FromBody] string[] ids, bool associations = false, string outSpatialReference = null)
        {
            var user = HttpContext.User;
            var gisService = Services.Get<IGisService<string>>(connectionId);
            return Ok(gisService.Get(FullNameString.FromUrl(ids), associations, outSpatialReference, user));
        }

        /// <summary>
        ///     Gets the total number of feature collections.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("featurecollections/{connectionId}/count")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount(string connectionId)
        {
            var user = HttpContext.User;
            var gisService = Services.Get<IGisService<string>>(connectionId);
            return Ok(gisService.Count(user));
        }

        /// <summary>
        ///     Gets all fullname identifiers.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is grouped (hierarchical).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        [HttpGet("featurecollections/{connectionId}/fullnames")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetFullNames(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var gisService = Services.Get<IGroupedGisService<string>>(connectionId);
            return group == null ? Ok(gisService.GetFullNames(user)) :
                Ok(gisService.GetFullNames(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Gets all geometry types within the given group.
        /// </summary>
        /// <remarks>
        ///     If no group is given, a dictionary of geometry types within every feature collection is retrieved.
        ///     If a group is given, a  dictionary of geometry types within the given group (recursive) is retrieved.
        ///     This is only applicable if the connection type is grouped (hierarchical).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        [HttpGet("featurecollections/{connectionId}/geometrytypes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IDictionary<string, string[]>> GetGeometryTypes(string connectionId, string group = null)
        {
            var user = HttpContext.User;
            var gisService = Services.Get<IGisService<string>>(connectionId);
            return group is null ? Ok(gisService.GetGeometryTypes(user)) :
                Ok(((IGroupedGisService<string>)gisService).GetGeometryTypes(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///   Gets properties for the feature collection with the given identifier
        /// </summary>
        /// <remarks>
        ///     This is only applicable if the connection type is grouped (hierarchical).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The Feature Collection Id.</param>
        [HttpGet("featurecollections/{connectionId}/properties/{featureCollectionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<FeatureCollectionInfo<string>> GetProperties(string connectionId, string featureCollectionId)
        {
            var user = HttpContext.User;
            var gisService = Services.Get<IGisService<string>>(connectionId);
            var info = gisService.GetInfo(FullNameString.FromUrl(featureCollectionId), user);
            return Ok(info);
        }

        /// <summary>
        ///     Gets properties for the feature collections within given group
        /// </summary>
        /// <remarks>
        ///     This is only applicable if the connection type is grouped (hierarchical).
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The feature collection identifier.</param>
        [HttpGet("featurecollections/{connectionId}/properties")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<FeatureCollectionInfo<string>>> GetInfoList(string connectionId, string group = "")
        {
            var user = HttpContext.User;
            var gisService = Services.Get<IGisService<string>>(connectionId);
            return Ok(((IGroupedGisService<string>)gisService).GetInfo(FullNameString.FromUrl(group), user));
        }

        /// <summary>
        ///     Gets a list of all feature collection IDs.
        /// </summary>
        /// <remarks>
        ///     The result can be filtered using query string parameters.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        [HttpGet("featurecollections/{connectionId}/ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds(string connectionId)
        {
            var user = HttpContext.User;
            var gisService = Services.Get<IGisService<string>>(connectionId);
            var query = Request.Query.ToQuery<FeatureCollection<string>>();
            return Ok(query.Any() ? gisService.GetIds(query, user) : gisService.GetIds(user));
        }

        /// <summary>
        ///     Gets a geometry collection from the feature collection with the given ID.
        /// </summary>
        /// <remarks>
        ///     Only the feature geometry is returned – no properties (attributes) are included.
        ///     The result can be filtered using query string parameters.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Feature collection not found</response>
        [HttpGet("geometrycollections/{connectionId}/{featureCollectionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetGeometry(string connectionId, string featureCollectionId)
        {
            var user = HttpContext.User;
            var query = Request.Query.ToQuery<FeatureCollection<string>>();

            var gisService = Services.Get<IGisService<string>>(connectionId);
            return query.Any() ? Ok(gisService.GetGeometry(FullNameString.FromUrl(featureCollectionId), query, user: user)) : Ok(gisService.GetGeometry(FullNameString.FromUrl(featureCollectionId), user: user));
        }

        /// <summary>
        ///     Gets a list of all feature collections within the given group.
        ///     If no group is given, all feature collections are returned.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="group">The group.</param>
        [HttpGet("featurecollections/{connectionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetList(string connectionId, string group = null)
        {
            var gisService = Services.Get<IGisService<string>>(connectionId);
            return group == null ? Ok(gisService.GetAll()) :
                Ok(((IGroupedGisService<string>)gisService).GetByGroup(FullNameString.FromUrl(group)));
        }

        /// <summary>
        ///     Downloads the feature collection with the given ID as a file.
        /// </summary>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        [HttpGet("featurecollections/{connectionId}/{featureCollectionId}/stream/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetStream(string connectionId, string featureCollectionId)
        {
            var user = HttpContext.User;
            var gisService = Services.Get<IGroupedGisService<string>>(connectionId);
            var (stream, fileType, fileName) = gisService.GetStream(FullNameString.FromUrl(featureCollectionId), user);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, fileType, fileName);
        }

        /// <summary>
        ///     Gets a list of all feature collection IDs fullfilling the given query
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="queryDTO">The query dto.</param>
        [HttpPost("featurecollections/{connectionId}/query")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> QueryFeatureCollection(string connectionId, [FromBody] QueryDTO<FeatureCollection<string>> queryDTO)
        {
            var user = HttpContext.User;
            var query = queryDTO.ToQuery();
            var gisService = Services.Get<IGisService<string>>(connectionId);
            return Ok(gisService.GetIds(query, user));
        }

        /// <summary>
        ///     Adds a new feature collection.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="collection">The feature collection body in GeoJson (https://geojson.org/) format.</param>
        /// <response code="201">Created</response>
        [HttpPost("featurecollections/{connectionId}/{featureCollectionId}")]
        [Authorize(Policy = "EditorsOnly")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult AddFeatureCollection(string connectionId, string featureCollectionId, [FromBody] IFeatureCollection collection)
        {
            var user = HttpContext.User;
            var fullNameString = FullNameString.FromUrl(featureCollectionId);
            var fullName = FullName.Parse(fullNameString);
            var featureCollection = new FeatureCollection(fullName.ToString(), fullName.Name, fullName.Group, collection);
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            updatableService.Add(featureCollection, user);
            return CreatedAtAction(nameof(Get), new { connectionId, featureCollectionId = FullNameString.ToUrl(featureCollection.Id) }, featureCollection);
        }

        /// <summary>
        ///     Updates an existing feature collection.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection ID.</param>
        /// <param name="collection">The feature collection body.</param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}")]
        [Authorize(Policy = "EditorsOnly")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult UpdateFeatureCollection(string connectionId, string featureCollectionId, [FromBody] IFeatureCollection collection)
        {
            var user = HttpContext.User;
            var fullNameString = FullNameString.FromUrl(featureCollectionId);
            var fullName = FullName.Parse(fullNameString);
            var featureCollection = new FeatureCollection(fullName.ToString(), fullName.Name, fullName.Group, collection);
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            updatableService.AddOrUpdate(featureCollection, user);
            return Ok(featureCollection);
        }

        /// <summary>
        ///     Deletes the feature collection with the given ID.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <response code="204">No content</response>
        /// <response code="404">Feature collection not found</response>
        [HttpDelete("featurecollections/{connectionId}/{featureCollectionId}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteFeatureCollection(string connectionId, string featureCollectionId)
        {
            var user = HttpContext.User;
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            updatableService.Remove(FullNameString.FromUrl(featureCollectionId), user);
            return NoContent();
        }

        /// <summary>
        ///     Gets the feature collection envelope.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="outSpatialReference">The output spatial reference.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Feature collection not found</response>
        [HttpGet("featurecollections/{connectionId}/{featureCollectionId}/envelope")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetFeatureCollectionEnvelope(string connectionId, string featureCollectionId, string outSpatialReference = null)
        {
            var user = HttpContext.User;
            var gisService = Services.Get<IGisService<string>>(connectionId);
            return Ok(gisService.GetEnvelope(FullNameString.FromUrl(featureCollectionId), outSpatialReference, user));
        }

        /// <summary>
        ///     Gets the feature collection footprint.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="outSpatialReference">The output spatial reference.</param>
        /// <param name="simplifyDistance">The simplify distance.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Feature collection not found</response>
        [HttpGet("featurecollections/{connectionId}/{featureCollectionId}/footprint")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IGeometry> GetFeatureCollectionFootprint(string connectionId, string featureCollectionId, string outSpatialReference = null, double? simplifyDistance = null)
        {
            var user = HttpContext.User;
            var gisService = Services.Get<IGisService<string>>(connectionId);
            return Ok(gisService.GetFootprint(FullNameString.FromUrl(featureCollectionId), outSpatialReference, simplifyDistance, user));
        }

        /// <summary>
        ///     Gets the footprint of a list of feature collections.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionIds">The feature collection identifiers.</param>
        /// <param name="outSpatialReference">The output spatial reference.</param>
        /// <param name="simplifyDistance">The simplify distance.</param>
        /// <response code="200">OK</response>
        [HttpPost("featurecollections/{connectionId}/footprint")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IGeometry> GetFeatureCollectionFootprintList(string connectionId, [FromBody] IEnumerable<string> featureCollectionIds, [FromQuery] string outSpatialReference = null, [FromQuery] double? simplifyDistance = null)
        {
            var user = HttpContext.User;
            var gisService = Services.Get<IGisService<string>>(connectionId);
            return Ok(gisService.GetFootprint(featureCollectionIds, outSpatialReference, simplifyDistance, user));
        }

        /// <summary>
        ///     Gets the feature IDs for the feature collection with the given ID.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Feature collection not found</response>
        [HttpGet("featurecollections/{connectionId}/{featureCollectionId}/ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<Guid>> GetFeatureIds(string connectionId, string featureCollectionId)
        {
            var user = HttpContext.User;
            var query = Request.Query.ToQuery<FeatureCollection<string>>();
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            var ids = updatableService.GetFeatureIds(FullNameString.FromUrl(featureCollectionId), query, user);
            return Ok(ids);
        }

        /// <summary>
        ///     Queries the features.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="queryDTO">The query dto.</param>
        /// <param name="associations">if set to <c>true</c> [associations].</param>
        /// <param name="outSpatialReference">The output spatial reference.</param>
        /// <response code="200">OK</response>
        /// <response code="404">Feature collection not found</response>
        [HttpPost("featurecollections/{connectionId}/{featureCollectionId}/query")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult QueryFeatures(string connectionId, string featureCollectionId, [FromBody] QueryDTO<IFeature> queryDTO, bool associations = false, string outSpatialReference = null)
        {
            var user = HttpContext.User;
            var query = queryDTO.ToQuery();
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            var features = updatableService.Get(FullNameString.FromUrl(featureCollectionId), query, associations, outSpatialReference, user);
            return Ok(features);
        }

        /// <summary>
        ///     Creates a new feature in the feature collection with the give identifier.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="feature">The feature object to create.</param>
        /// <response code="201">Created</response>
        [HttpPost("featurecollections/{connectionId}/{featureCollectionId}/feature/")]
        [Authorize(Policy = "EditorsOnly")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult AddFeature(string connectionId, string featureCollectionId, [FromBody] IFeature feature)
        {
            var user = HttpContext.User;
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);

            // Work around until UpdatableGisService.AddFeature returns new FeatureId
            var existingFeatureIds = updatableService.GetFeatureIds(FullNameString.FromUrl(featureCollectionId), null);

            updatableService.AddFeature(FullNameString.FromUrl(featureCollectionId), feature, user);

            var resultingFeatureIds = updatableService.GetFeatureIds(FullNameString.FromUrl(featureCollectionId), null);
            var newId = resultingFeatureIds.Except(existingFeatureIds).First();

            var newFeature = updatableService.GetFeature(featureCollectionId, newId);

            return CreatedAtAction(nameof(GetFeature), new { connectionId, featureCollectionId, featureId = newId }, newFeature);
        }

        /// <summary>
        ///     Gets the feature with the given ID within the feature collection with the given ID.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="featureId">The feature identifier.</param> 
        /// <param name="associations">If set to <c>true</c> associations are returned.</param>
        /// <param name="outSpatialReference">The output spatial reference.</param>
        /// <param name="geometry">If set to <c>false</c> no geometry is returned (only info).</param>
        /// <response code="200">OK</response>
        /// <response code="404">Feature collection not found</response>
        [HttpGet("featurecollections/{connectionId}/{featureCollectionId}/feature/{featureId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetFeature(string connectionId, string featureCollectionId, string featureId, bool associations = false, string outSpatialReference = null, bool geometry = true)
        {
            var user = HttpContext.User;
            var guid = new Guid(featureId);
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            return geometry ? Ok(updatableService.GetFeature(FullNameString.FromUrl(featureCollectionId), guid, associations, outSpatialReference, user)) :
                Ok(updatableService.GetFeatureInfo(FullNameString.FromUrl(featureCollectionId), guid, associations, user));
        }

        /// <summary>
        ///     Updates a feature.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="feature">The feature to be updated.</param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/feature/")]
        [Authorize(Policy = "EditorsOnly")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateFeature(string connectionId, string featureCollectionId, [FromBody] IFeature feature)
        {
            var user = HttpContext.User;
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            updatableService.UpdateFeature(FullNameString.FromUrl(featureCollectionId), feature, user);
            return Ok(feature);
        }

        /// <summary>
        ///     Removes the feature with the given ID.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="featureId">The feature identifier.</param>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Feature not found</response>
        [HttpDelete("featurecollections/{connectionId}/{featureCollectionId}/feature/{featureId}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RemoveFeature(string connectionId, string featureCollectionId, string featureId)
        {
            var user = HttpContext.User;
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            var guid = new Guid(featureId);
            updatableService.RemoveFeature(FullNameString.FromUrl(featureCollectionId), guid, user);
            return NoContent();
        }

        /// <summary>
        ///     Removes the features with the given IDs.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="featureIds">The feature ids.</param>
        [HttpDelete("featurecollections/{connectionId}/{featureCollectionId}/feature/")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult RemoveFeatures(string connectionId, string featureCollectionId, [FromBody] Guid[] featureIds)
        {
            var user = HttpContext.User;
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            updatableService.RemoveFeatures(FullNameString.FromUrl(featureCollectionId), featureIds, user);
            return NoContent();
        }

        /// <summary>
        ///     Creates a new attribute in the feature collection with the given ID.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="featureCollectionId">The feature collection identifier.</param>
        /// <param name="attribute">The attribute dto.</param>
        [HttpPost("featurecollections/{connectionId}/{featureCollectionId}/attribute/")]
        [Authorize(Policy = "EditorsOnly")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult CreateAttribute(string connectionId, string featureCollectionId, [FromBody] IAttribute attribute)
        {
            var user = HttpContext.User;
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            updatableService.AddAttribute(FullNameString.FromUrl(featureCollectionId), attribute, user);
            // TODO: Cannot construct attribute url because there is no clear way to reach the attribute id [edit: What about the name?]
            return Created("Attribute", attribute);
        }

        /// <summary>
        ///     List all attributes of a feature collection
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        [HttpGet("featurecollections/{connectionId}/{featureCollectionId}/attribute/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IList<Attribute>> GetAllAttributes(string connectionId, string featureCollectionId)
        {
            var user = HttpContext.User;
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            var featureCollection = updatableService.Get(FullNameString.FromUrl(featureCollectionId), user);
            return Ok(featureCollection.Attributes);
        }

        /// <summary>
        ///     List all attributes of a feature collection
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="attributeIndex"></param>
        [HttpGet("featurecollections/{connectionId}/{featureCollectionId}/attribute-index/{attributeIndex}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IList<Attribute>> GetAllAttributesByIndex(string connectionId, string featureCollectionId, int attributeIndex)
        {
            var user = HttpContext.User;
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            var featureCollection = updatableService.GetAttribute(FullNameString.FromUrl(featureCollectionId), attributeIndex, user);
            return Ok(featureCollection);
        }

        /// <summary>
        ///     Update attribute field of a feature collection
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="attribute">Attribute data transfer object</param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/attribute/")]
        [Authorize(Policy = "EditorsOnly")]
        [Consumes("application/json")]
        public void UpdateAttribute(string connectionId, string featureCollectionId, [FromBody] IAttribute attribute)
        {
            var user = HttpContext.User;
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            updatableService.UpdateAttribute(FullNameString.FromUrl(featureCollectionId), attribute, user);
        }

        /// <summary>
        ///     Remove attribute field from a feature collection
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="attributeIndex">Zero-based index of attribute field to remove.</param>
        [HttpDelete("featurecollections/{connectionId}/{featureCollectionId}/attribute/{attributeIndex}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RemoveAttribute(string connectionId, string featureCollectionId, int attributeIndex)
        {
            var user = HttpContext.User;
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            updatableService.RemoveAttribute(FullNameString.FromUrl(featureCollectionId), attributeIndex, user);
            return NoContent();
        }

        /// <summary>
        ///     Update attribute value for a feature in a feature collection
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="featureId">Id of the feature to update.</param>
        /// <param name="attributeIndex">Zero-based index of attribute field to update.</param>
        /// <param name="value">Value object like {"value": 1}.</param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/feature/{featureId}/attribute/{attributeIndex}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public void UpdateAttributeValueForFeature(string connectionId, string featureCollectionId, string featureId, int attributeIndex, [FromBody] JsonDocument value)
        {
            if (value.RootElement.TryGetProperty("value", out JsonElement valueElement))
            {
                var val = valueElement.GetRawText();
                var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
                var featureGuid = new Guid(featureId);
                updatableService.UpdateAttributeValue(FullNameString.FromUrl(featureCollectionId), featureGuid, attributeIndex, val, HttpContext.User);
            }
        }

        /// <summary>
        ///     Update attribute value for features that match a list of feature ids in a feature collection
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="attributeIndex">Zero-based index of attribute field to update.</param>
        /// <param name="parameters">Object like {"value": 1, "featureIds": [featureId, featureId, ...]}.</param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/attribute/{attributeIndex}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public void UpdateAttributeValueForFeatures(string connectionId, string featureCollectionId, int attributeIndex, [FromBody] JsonDocument parameters)
        {
            var user = HttpContext.User;

            if (parameters.RootElement.TryGetProperty("value", out JsonElement valueElement))
            {
                var val = valueElement.GetRawText();
                if (parameters.RootElement.TryGetProperty("featureIds", out JsonElement featureIdsElement))
                {
                    var itemList = new List<Guid>();
                    foreach (JsonElement itemElement in featureIdsElement.EnumerateArray())
                    {
                        if (itemElement.ValueKind == JsonValueKind.String)
                        {
                            var itemValue = new Guid(itemElement.GetString());
                            itemList.Add(itemValue);
                        }
                    }
                    var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
                    updatableService.UpdateAttributeValue(FullNameString.FromUrl(featureCollectionId), itemList, attributeIndex, val, user);
                }
            }
        }

        /// <summary>
        ///     Update attribute value for features in a feature collection that match a specified filter.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="attributeIndex">Zero-based index of attribute field to update.</param>
        /// <param name="parameters">Object like {"value": 1, "filter": [{"Item": "country", "Operator": "Equal", "DK"}, ...]}.</param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/attribute-where/{attributeIndex}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public void UpdateAttributeValueForFeaturesWhere(string connectionId, string featureCollectionId, int attributeIndex, [FromBody] JsonDocument parameters)
        {
            var user = HttpContext.User;

            if (parameters.RootElement.TryGetProperty("value", out JsonElement valueElement))
            {
                var val = valueElement.GetRawText();
                if (parameters.RootElement.TryGetProperty("filter", out JsonElement filterParameterElement))
                {
                    var filterParameter = filterParameterElement.GetRawText();
                    //var filter = _JsonToFilter(filterParameter);
                    var filterDeserialized = JsonSerializer.Deserialize<List<QueryCondition>>(filterParameter.ToString(), SerializerOptionsDefault.Options);
                    var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
                    updatableService.UpdateAttributeValue(FullNameString.FromUrl(featureCollectionId), filterDeserialized, attributeIndex, val, user);
                }
            }
        }

        /// <summary>
        ///     Update attribute value by name for a feature in a feature collection
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="featureId">Id of the feature to update.</param>
        /// <param name="attributeName">Name of the attribute field to update.</param>
        /// <param name="value">Value object like {"value": 1}.</param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/feature/{featureId}/attribute-by-name/{attributeName}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public void UpdateAttributeValueByNameForFeature(string connectionId, string featureCollectionId, string featureId, string attributeName, [FromBody] JsonDocument value)
        {
            var user = HttpContext.User;

            if (value.RootElement.TryGetProperty("value", out JsonElement valueElement))
            {
                var val = valueElement.GetRawText();
                var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
                var featureGuid = new Guid(featureId);
                updatableService.UpdateAttributeValue(FullNameString.FromUrl(featureCollectionId), featureGuid, attributeName, val, user);
            }
        }

        /// <summary>
        ///     Update attribute value  by name for features that match a list of feature ids in a feature collection
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="attributeName">Name of the attribute field to update.</param>
        /// <param name="parameters">Object like {"value": 1, "featureIds": [featureId, featureId, ...]}.</param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/attribute-by-name/{attributeName}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public void UpdateAttributeValueByNameForFeatures(string connectionId, string featureCollectionId, string attributeName, [FromBody] JsonDocument parameters)
        {
            var user = HttpContext.User;

            if (parameters.RootElement.TryGetProperty("value", out JsonElement valueElement))
            {
                var val = valueElement.GetRawText();
                if (parameters.RootElement.TryGetProperty("featureIds", out JsonElement featureIdsElement))
                {
                    var itemList = new List<Guid>();
                    foreach (JsonElement itemElement in featureIdsElement.EnumerateArray())
                    {
                        if (itemElement.ValueKind == JsonValueKind.String)
                        {
                            var itemValue = new Guid(itemElement.GetString());
                            itemList.Add(itemValue);
                        }
                    }
                    var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
                    updatableService.UpdateAttributeValue(FullNameString.FromUrl(featureCollectionId), itemList, attributeName, val, user);
                }
            }
        }

        /// <summary>
        ///     Update attribute value  by name for features in a feature collection that match a specified filter.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="attributeName">Name of the attribute field to update.</param>
        /// <param name="parameters">Object like {"value": 1, "filter": [{"Item": "country", "Operator": "Equal", "DK"}, ...]}.</param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/attribute-by-name-where/{attributeName}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public void UpdateAttributeValueByNameForFeaturesWhere(string connectionId, string featureCollectionId, string attributeName, [FromBody] JsonDocument parameters)
        {
            var user = HttpContext.User;

            if (parameters.RootElement.TryGetProperty("value", out JsonElement valueElement))
            {
                var val = valueElement.GetRawText();
                if (parameters.RootElement.TryGetProperty("filter", out JsonElement filterParameterElement))
                {
                    var filterParameter = filterParameterElement.GetRawText();
                    //var filter = _JsonToFilter(filterParameter.ToString());
                    var filterDeserialized = JsonSerializer.Deserialize<List<QueryCondition>>(filterParameter.ToString(), SerializerOptionsDefault.Options);
                    var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
                    updatableService.UpdateAttributeValue(FullNameString.FromUrl(featureCollectionId), filterDeserialized, attributeName, val, user);
                }
            }
        }

        /// <summary>
        ///     Update attribute values for a feature in a feature collection.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="featureId">Feature identifier</param>
        /// <param name="parameters">Object like {"attributes": {1: "red", 2: 1}}.</param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/feature/{featureId}/attributes/")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public void UpdateAttributeValuesForFeature(string connectionId, string featureCollectionId, string featureId, [FromBody] JsonDocument parameters)
        {
            var user = HttpContext.User;

            // Get the root element of the JSON document
            JsonElement root = parameters.RootElement;

            // Check if "attributes" property exists
            if (root.TryGetProperty("attributes", out JsonElement attributesElement) && attributesElement.ValueKind == JsonValueKind.Object)
            {
                // Deserialize the JSON object into a Dictionary<int, object>
                var attributes = JsonSerializer.Deserialize<Dictionary<int, object>>(attributesElement.GetRawText(), SerializerOptionsDefault.Options);
                var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
                updatableService.UpdateAttributeValues(FullNameString.FromUrl(featureCollectionId), Guid.Parse(featureId), attributes, user);
            }
        }


        /// <summary>
        ///     Update attribute values for a feature in a feature collection.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="featureId">Feature identifier</param>
        /// <param name="parameters">Object like {"attributes": {"attr1": "red", "attr2": 1}}.</param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/feature/{featureId}/attributes-by-name/")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public void UpdateAttributeValuesByNameForFeature(string connectionId, string featureCollectionId, string featureId, [FromBody] JsonDocument parameters)
        {
            var user = HttpContext.User;

            // Get the root element of the JSON document
            JsonElement root = parameters.RootElement;

            // Check if "attributes" property exists
            if (root.TryGetProperty("attributes", out JsonElement attributesElement) && attributesElement.ValueKind == JsonValueKind.Object)
            {
                // Deserialize the JSON object into a Dictionary<int, object>
                var attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(attributesElement.GetRawText(), SerializerOptionsDefault.Options);
                var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
                updatableService.UpdateAttributeValues(FullNameString.FromUrl(featureCollectionId), Guid.Parse(featureId), attributes, user);
            }
        }

        /// <summary>
        ///     Update attribute values for features that match a list of feature ids in a feature collection
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="parameters">
        ///     Object like {"attributes": {"attr1": "red", "attr2": 1 }, "featureIds": [featureId, featureId,
        ///     ...]}.
        /// </param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/attributes-by-name/")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public void UpdateAttributeValuesByNameForFeatures(string connectionId, string featureCollectionId, [FromBody] JsonDocument parameters)
        {
            var user = HttpContext.User;

            // Get the root element of the JSON document
            JsonElement root = parameters.RootElement;

            // Check if "attributes" property exists
            if (root.TryGetProperty("attributes", out JsonElement attributesElement) && attributesElement.ValueKind == JsonValueKind.Object)
            {
                // Deserialize the JSON object into a Dictionary<int, object>
                var attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(attributesElement.GetRawText(), SerializerOptionsDefault.Options);

                if (root.TryGetProperty("featureIds", out JsonElement featureIdsElement))
                {
                    var featureIds = JsonSerializer.Deserialize<Guid[]>(featureIdsElement.GetRawText(), SerializerOptionsDefault.Options);
                    var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
                    updatableService.UpdateAttributeValues(FullNameString.FromUrl(featureCollectionId), featureIds, attributes, user);
                }
            }
        }

        /// <summary>
        ///     Update attribute values for features that match a list of feature ids in a feature collection
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="parameters">Object like {"attributes": {1: "red", 2: 1 }, "featureIds": [featureId, featureId, ...]}.</param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/attributes/")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public void UpdateAttributeValuesForFeatures(string connectionId, string featureCollectionId, [FromBody] JsonDocument parameters)
        {
            var user = HttpContext.User;

            // Get the root element of the JSON document
            JsonElement root = parameters.RootElement;

            // Check if "attributes" property exists
            if (root.TryGetProperty("attributes", out JsonElement attributesElement) && attributesElement.ValueKind == JsonValueKind.Object)
            {
                // Deserialize the JSON object into a Dictionary<int, object>
                var attributes = JsonSerializer.Deserialize<Dictionary<int, object>>(attributesElement.GetRawText(), SerializerOptionsDefault.Options);

                if (root.TryGetProperty("featureIds", out JsonElement featureIdsElement))
                {
                    var featureIds = JsonSerializer.Deserialize<Guid[]>(featureIdsElement.GetRawText(), SerializerOptionsDefault.Options);
                    var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
                    updatableService.UpdateAttributeValues(FullNameString.FromUrl(featureCollectionId), featureIds, attributes, user);
                }
            }
        }

        /// <summary>
        ///     Update attribute values for features in a feature collection that match a specified filter.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="parameters">
        ///     Object like {"attributes": {"attr1": "red", "attr2": 1}, "filter": [{"Item": "country",
        ///     "Operator": "Equal", "DK"}, ...]}.
        /// </param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/attributes-by-name-where/")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public void UpdateAttributeValuesByNameForFeaturesWhere(string connectionId, string featureCollectionId, [FromBody] JsonDocument parameters)
        {
            var user = HttpContext.User;

            // Check if "attributes" property exists
            if (parameters.RootElement.TryGetProperty("attributes", out JsonElement valueElement))
            {
                // Deserialize the JSON object into a Dictionary<int, object>
                var attributes = JsonSerializer.Deserialize<Dictionary<string, object>>(valueElement.GetRawText(), SerializerOptionsDefault.Options);

                if (parameters.RootElement.TryGetProperty("filter", out JsonElement filterParameterElement))
                {
                    //var filter = _JsonToFilter(filterParameterElement.ToString());
                    var filterDeserialized = JsonSerializer.Deserialize<List<QueryCondition>>(filterParameterElement.ToString(), SerializerOptionsDefault.Options);
                    var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);

                    //updatableService.UpdateAttributeValues(FullNameString.FromUrl(featureCollectionId), filter, attributes, user);
                    foreach (var filterResult in filterDeserialized)
                    {
                        var filterFetch = new List<QueryCondition>();
                        filterFetch.Add(filterResult);
                        updatableService.UpdateAttributeValues(FullNameString.FromUrl(featureCollectionId), filterFetch, attributes, user);
                    }
                }
            }
        }

        /// <summary>
        ///     Update attribute values for features in a feature collection that match a specified filter.
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="parameters">
        ///     Object like {"attributes": {1: "red", 2: 1}, "filter": [{"Item": "country", "Operator":
        ///     "Equal", "DK"}, ...]}.
        /// </param>
        [HttpPut("featurecollections/{connectionId}/{featureCollectionId}/attributes-where/")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public void UpdateAttributeValuesForFeaturesWhere(string connectionId, string featureCollectionId, [FromBody] JsonDocument parameters)
        {
            var user = HttpContext.User;

            // Check if "attributes" property exists
            if (parameters.RootElement.TryGetProperty("attributes", out JsonElement valueElement))
            {
                // Deserialize the JSON object into a Dictionary<int, object>
                var attributes = JsonSerializer.Deserialize<Dictionary<int, object>>(valueElement.GetRawText(), SerializerOptionsDefault.Options);

                if (parameters.RootElement.TryGetProperty("filter", out JsonElement filterParameterElement))
                {
                    //var filter = _JsonToFilter(filterParameterElement.ToString());
                    var filterDeserialized = JsonSerializer.Deserialize<List<QueryCondition>>(filterParameterElement.ToString(), SerializerOptionsDefault.Options);
                    var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);

                    //updatableService.UpdateAttributeValues(FullNameString.FromUrl(featureCollectionId), filterDeserialized, attributes, user);
                    foreach (var filterResult in filterDeserialized)
                    {
                        var filterFetch = new List<QueryCondition>();
                        filterFetch.Add(filterResult);
                        updatableService.UpdateAttributeValues(FullNameString.FromUrl(featureCollectionId), filterFetch, attributes, user);
                    }
                }
            }
        }

        /// <summary>
        ///     Remove attribute field  by name from a feature collection
        /// </summary>
        /// <remarks>
        ///     NOTE: Only applicable if the connection type is updatable.
        /// </remarks>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="featureCollectionId">Feature collection identifier</param>
        /// <param name="attributeName">Name of the attribute field to remove.</param>
        [HttpDelete("featurecollections/{connectionId}/{featureCollectionId}/attribute-by-name/{attributeName}")]
        [Authorize(Policy = "EditorsOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult RemoveAttributeByName(string connectionId, string featureCollectionId, string attributeName)
        {
            var user = HttpContext.User;
            var updatableService = Services.Get<IUpdatableGisService<string, Guid>>(connectionId);
            updatableService.RemoveAttribute(FullNameString.FromUrl(featureCollectionId), attributeName, user);
            return NoContent();
        }
    }
}