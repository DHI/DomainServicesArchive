namespace DHI.Services.Security.WebApi.Controllers
{
    using System.Collections.Generic;
    using Accounts;
    using Authorization;
    using DTOs;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;

    /// <summary>
    ///     User Groups API
    /// </summary>
    [Produces("application/json")]
    [Route("api/usergroups")]
    [Authorize(Policy = "AdministratorsOnly")]
    [ApiController]
    [ApiVersion("1")]
    [SwaggerTag("Actions for managing user groups. User groups are used to define permissions on entities.")]
    public class UserGroupsController : ControllerBase
    {
        private readonly UserGroupService _userGroupService;
        private readonly AccountService _accountService;

        public UserGroupsController(IUserGroupRepository userGroupRepository, IAccountRepository accountRepository)
        {
            _userGroupService = new UserGroupService(userGroupRepository);
            _accountService = new AccountService(accountRepository);
        }

        /// <summary>
        ///     Gets the user group with the specified identifier.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">User group not found</response>
        /// <param name="id">The user group ID.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UserGroup> Get(string id)
        {
            var principal = HttpContext.User;

            if (!_userGroupService.TryGet(id, out var userGroup, principal))
            {
                return NotFound();
            }

            return Ok(userGroup);
        }

        /// <summary>
        ///     Gets a list of all user groups.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<UserGroup>> GetAll()
        {
            var principal = HttpContext.User;
            return Ok(_userGroupService.GetAll(principal));
        }

        /// <summary>
        ///     Gets the total number of user groups.
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> GetCount()
        {
            var principal = HttpContext.User;
            return Ok(_userGroupService.Count(principal));
        }

        /// <summary>
        ///     Gets a list of user group identifiers.
        ///     If no user identifier is given, all groups are returned.
        ///     If a user identifier is given, all groups that the given user is a member of is returned.
        /// </summary>
        [HttpGet("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<string>> GetIds(string userId)
        {
            var principal = HttpContext.User;
            return userId is null ? Ok(_userGroupService.GetIds(principal)) : Ok(_userGroupService.GetIds(userId, principal));
        }

        /// <summary>
        ///     Adds a new user group.
        /// </summary>
        /// <response code="201">Created</response>
        /// <param name="userGroupDto">The user group body.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public ActionResult<UserGroup> Add([FromBody] UserGroupDTO userGroupDto)
        {
            var principal = HttpContext.User;
            var userGroup = userGroupDto.ToUserGroup();
            _userGroupService.Add(userGroup, principal);
            return CreatedAtAction(nameof(Get), new { id = userGroup.Id }, userGroup);
        }

        /// <summary>
        ///     Adds a user to all groups with the specified identifiers.
        /// </summary>
        /// <response code="404">User not found</response>
        /// <response code="404">User group not found</response>
        /// <param name="userId">The user ID</param>
        /// <param name="groupIds">The user group IDs.</param>
        [HttpPost("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public IActionResult AddUser(string userId, [FromBody] string[] groupIds)
        {
            if (!_accountService.Exists(userId))
            {
                throw new KeyNotFoundException($"User with id '{userId}' was not found.");
            }

            var principal = HttpContext.User;
            foreach (var groupId in groupIds)
            {
                _userGroupService.AddUser(groupId, userId, principal);
            }

            return Ok();
        }

        /// <summary>
        ///     Updates an existing user group.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">User group not found</response>
        /// <param name="userGroupDto">The user group body.</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        public ActionResult<UserGroup> Update([FromBody] UserGroupDTO userGroupDto)
        {
            var principal = HttpContext.User;
            var userGroup = userGroupDto.ToUserGroup();
            _userGroupService.Update(userGroup, principal);
            if (!_userGroupService.TryGet(userGroup.Id, out var value, principal))
            {
                return NotFound();
            }

            return Ok(value);
        }

        /// <summary>
        ///     Deletes the user group with the specified identifier.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">User group not found</response>
        /// <param name="id">The user group ID.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string id)
        {
            var principal = HttpContext.User;
            _userGroupService.Remove(id, principal);
            return NoContent();
        }

        /// <summary>
        ///     Removes the user with the given identifier from the group with the given identifier.
        ///     If no group identifier is specified, the user is removed from all groups.
        /// </summary>
        /// <response code="204">No Content. Successfully deleted</response>
        /// <response code="404">User group not found</response>
        /// <param name="userId">The userId</param>
        /// <param name="groupId">The user group ID.</param>
        [HttpDelete("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteUser(string userId, string groupId)
        {
            var principal = HttpContext.User;
            if (groupId is null)
            {
                _userGroupService.RemoveUser(userId, principal);
            }
            else
            {
                _userGroupService.RemoveUser(groupId, userId, principal);
            }

            return Ok();
        }
    }
}