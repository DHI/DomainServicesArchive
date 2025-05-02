namespace DHI.Services.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    ///     User Group Service.
    /// </summary>
    public class UserGroupService : BaseUpdatableDiscreteService<UserGroup, string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UserGroupService" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public UserGroupService(IUserGroupRepository repository) : base(repository)
        {
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IUserGroupRepository>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path)
        {
            return Service.GetProviderTypes<IUserGroupRepository>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">
        ///     The path where to look for compatible providers. If path is null, the path of the executing assembly
        ///     is used.
        /// </param>
        /// <param name="searchPattern">
        ///     File name search pattern. Can contain a combination of valid literal path and wildcard
        ///     (*and ?) characters.
        /// </param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IUserGroupRepository>(path, searchPattern);
        }

        /// <summary>
        ///     Adds a user with the given identifier to the group with the given identifier.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="principal">The current user.</param>
        /// <returns><c>true</c> if the user was successfully added, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">Cannot be null or empty. - userId</exception>
        public bool AddUser(string groupId, string userId, ClaimsPrincipal principal = null)
        {
            Guard.Against.NullOrEmpty(userId, nameof(userId));
            if (!TryGet(groupId, out var group, principal))
            {
                throw new KeyNotFoundException($"Group with id {groupId} not found.");
            }

            if (!group.Users.Add(userId))
            {
                return false;
            }

            Update(group, principal);
            return true;
        }

        /// <summary>
        ///     Removes the user with the given identifier from the group with the given identifier.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="principal">The current user.</param>
        /// <returns><c>true</c> if the user was successfully removed, <c>false</c> otherwise.</returns>
        public bool RemoveUser(string groupId, string userId, ClaimsPrincipal principal = null)
        {
            if (!TryGet(groupId, out var group, principal))
            {
                throw new KeyNotFoundException($"Group with id {groupId} not found.");
            }

            if (!group.Users.Remove(userId))
            {
                return false;
            }

            Update(group, principal);
            return true;
        }

        /// <summary>
        ///     Removes the user with the given identifier from all groups.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="principal">The current user.</param>
        public void RemoveUser(string userId, ClaimsPrincipal principal = null)
        {
            foreach (var groupId in GetIds(userId, principal))
            {
                RemoveUser(groupId, userId, principal);
            }
        }

        /// <summary>
        ///     Determines whether the specified group contains a user with the given identifier.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="principal">The current user.</param>
        /// <returns><c>true</c> if the specified group contains user; otherwise, <c>false</c>.</returns>
        public bool ContainsUser(string groupId, string userId, ClaimsPrincipal principal = null)
        {
            if (!TryGet(groupId, out var group, principal))
            {
                return false;
            }

            return group.Users.Contains(userId);
        }

        /// <summary>
        ///     Determines whether any of the specified groups contains a user with the given identifier.
        /// </summary>
        /// <param name="groupIds">The group ids.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="principal">The current user.</param>
        /// <returns><c>true</c> if any of the specified groups contains user, <c>false</c> otherwise.</returns>
        public bool AnyContainsUser(IEnumerable<string> groupIds, string userId, ClaimsPrincipal principal = null)
        {
            foreach (var groupId in groupIds)
            {
                if (!TryGet(groupId, out var group, principal))
                {
                    continue;
                }

                if (group.Users.Contains(userId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Gets all identifiers for the groups that contains the user with the specified identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="principal">The current user.</param>
        public IEnumerable<string> GetIds(string userId, ClaimsPrincipal principal = null)
        {
            return GetIds(principal).Where(groupId => ContainsUser(groupId, userId, principal));
        }
    }
}