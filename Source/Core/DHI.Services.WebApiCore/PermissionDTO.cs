namespace DHI.Services.WebApiCore
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Authorization;

    /// <summary>
    ///     Data transfer object for a permission resource representation.
    /// </summary>
    public class PermissionDTO
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PermissionDTO"/> class.
        /// </summary>
        public PermissionDTO()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PermissionDTO"/> class.
        /// </summary>
        /// <param name="permission">The permission.</param>
        public PermissionDTO(Permission permission)
        {
            Principals = permission.Principals.ToList();
            Operation = permission.Operation;
            Type = permission.Type.ToString();
        }

        /// <summary>
        ///     Gets or sets the permission principals.
        /// </summary>
        /// <value>The permission principals.</value>
        [Required]
        public List<string> Principals { get; set; }

        /// <summary>
        ///     Gets or sets the operation.
        /// </summary>
        /// <value>The operation.</value>
        [Required]
        public string Operation { get; set; }

        /// <summary>
        ///     Gets or sets the permission type.
        /// </summary>
        /// <value>The permission type.</value>
        public string Type { get; set; }

        /// <summary>
        /// Converts the DTO to a permission object.
        /// </summary>
        public Permission ToPermission()
        {
            Permission permission;
            if (Type is null)
            {
                permission = new Permission(Principals, Operation);
            }
            else
            {
                permission = new Permission(Principals, Operation, (PermissionType)Enum.Parse(typeof(PermissionType), Type));
            }

            return permission;
        }
    }
}