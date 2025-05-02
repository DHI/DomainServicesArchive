namespace DHI.Services.Connections.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;


    [Produces("application/json")]
    [Route("api/connections")]
    [Authorize]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing connections. Connections are used to configure the Web API services. Connections are [factory](https://en.wikipedia.org/wiki/Factory_(object-oriented_programming)) resources that hold the necessary information to create the concrete service objects.")]
    public class ConnectionsController : ControllerBase
    {
        private ConnectionTypeService _connectionTypeService;

        public ConnectionsController(ConnectionTypeService connectionTypeService)
        {
            _connectionTypeService = connectionTypeService ?? throw new ArgumentNullException(nameof(connectionTypeService));
        }

        /// <summary>
        ///     Adds a new connection.
        /// </summary>
        /// <remarks>
        ///     The connection representation must define the connection type using the $type property.
        /// </remarks>
        /// <response code="201">Created</response>
        /// <param name="connection">Class of <seealso cref="IConnection"/> to be added</param>
        [HttpPost]
        [Authorize(Policy = "AdministratorsOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public ActionResult<IConnection> Add([FromBody] IConnection connection)
        {
            var user = HttpContext.User;
            Services.Connections.Add(connection, user);
            if (Services.Connections.TryGet(connection.Id, out var newConnection, user))
            {
                return CreatedAtAction(nameof(Get), new { id = newConnection.Id }, newConnection);
            }

            return BadRequest();
        }


        /// <summary>
        ///     Updates an existing connection.
        /// </summary>
        /// <remarks>
        ///     The connection representation must define the connection type using the $type property.
        /// </remarks>
        /// <response code="404">Connection not found</response>
        /// <param name="connection">Class of <seealso cref="IConnection"/> to be updated</param>
        [HttpPut]
        [Authorize(Policy = "AdministratorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<IConnection> Update([FromBody] IConnection connection)
        {
            var user = HttpContext.User;
            Services.Connections.Update(connection, user);
            if (Services.Connections.TryGet(connection.Id, out var newConnection))
            {
                return Ok(newConnection);
            }

            return NotFound();
        }

        /// <summary>
        ///     Deletes the connection with the specified identifier.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">Connection not found</response>
        /// <param name="id">The connection ID.</param>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdministratorsOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string id)
        {
            var user = HttpContext.User;
            Services.Connections.Remove(id, user);
            return NoContent();
        }

        /// <summary>
        ///     Gets a connection with the specified identifier.
        /// </summary>
        /// <response code="404">Connection not found</response>
        /// <param name="id">The connection ID.</param>
        [HttpGet("{id}")]
        [Authorize(Policy = "AdministratorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IConnection> Get(string id)
        {
            var user = HttpContext.User;
            if (Services.Connections.TryGet(id, out var connection, user))
            {
                return Ok(connection);
            }

            return NotFound();
        }

        /// <summary>
        ///     Gets a list of all connections.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AdministratorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<IConnection>> GetAll()
        {
            var user = HttpContext.User;
            var connections = Services.Connections.GetAll(user);
            return Ok(connections.Select(connection => connection));
        }

        /// <summary>
        ///     Gets a list of all connection ids.
        /// </summary>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<object>> GetIds()
        {
            var user = HttpContext.User;
            var connections = Services.Connections.GetAll(user);
            return Ok(connections.Select(connection => new { connection.Id, Type = connection.GetType() }).ToList());
        }

        /// <summary>
        ///     Gets a list of all connection types.
        /// </summary>
        [HttpGet("types")]
        [Authorize(Policy = "AdministratorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ConnectionType>> GetAllTypes()
        {
            var user = HttpContext.User;
            var connectionTypes = _connectionTypeService.GetAll(user);
            return Ok(connectionTypes);
        }

        /// <summary>
        ///     Gets the total number of connections.
        /// </summary>
        [HttpGet("count")]
        [Authorize(Policy = "AdministratorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount()
        {
            var user = HttpContext.User;
            return Ok(Services.Connections.Count(user));
        }

        /// <summary>
        ///     Gets the connection type with the given identifier.
        /// </summary>
        /// <response code="404">Connection type not found</response>
        /// <param name="typeId">The connection type ID.</param>
        [HttpGet("types/{typeId}")]
        [Authorize(Policy = "AdministratorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ConnectionType> GetType(string typeId)
        {
            var user = HttpContext.User;
            if (_connectionTypeService.TryGet(typeId, out var connectionType, user))
            {
                return Ok(connectionType);
            }

            return NotFound();
        }

        /// <summary>
        ///     Gets a list of connection type IDs.
        /// </summary>
        [HttpGet("types/ids")]
        [Authorize(Policy = "AdministratorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetTypeIds()
        {
            var user = HttpContext.User;
            var connectionType = _connectionTypeService.GetIds(user);
            return Ok(connectionType);
        }

        /// <summary>
        ///     Existing connection verification.
        /// </summary>
        /// <remarks>
        ///     Verifies that an existing connection with the given ID is valid – i.e. that the connection can be established.
        /// </remarks>
        /// <response code="422">Unprocessable. Verification failed</response>
        /// <response code="404">Connection not found</response>
        /// <param name="id">The connection ID.</param>
        [HttpGet("{id}/verification")]
        [Authorize(Policy = "AdministratorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult VerifyExisting(string id)
        {
            var user = HttpContext.User;
            if (!Services.Connections.TryGet(id, out var connection, user))
            {
                return NotFound();
            }

            try
            {
                connection.Create();
                return Ok();
            }
            catch (Exception)
            {
                return UnprocessableEntity();
            }
        }

        /// <summary>
        ///     New connection verification
        /// </summary>
        /// <remarks>
        ///     Verifies that a new connection is valid – i.e. that the connection can be established. The connection
        ///     representation must define the connection type using the $type property.
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="422">Unprocessable. Verification failed</response>
        /// <param name="connection">Class of <see cref="IConnection"/> to be verified</param> 
        [HttpPost("verification")]
        [Authorize(Policy = "AdministratorsOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Consumes("application/json")]
        public IActionResult VerifyNew([FromBody] IConnection connection)
        {
            try
            {
                connection.Create();
                return Ok();
            }
            catch (Exception)
            {
                return UnprocessableEntity();
            }
        }
    }
}