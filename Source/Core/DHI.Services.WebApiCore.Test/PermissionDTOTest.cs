
namespace DHI.Services.WebApiCore.Test
{
    using System;
    using System.Collections.Generic;
    using Authorization;
    using Xunit;

    public class PermissionDTOTest
    {
        [Fact]
        public void ConstructorIsOk()
        {
            var permission = new Permission(new []{"Administrators", "Editors"}, "delete");
            var permissionDTO = new PermissionDTO(permission);

            Assert.Equal(2, permissionDTO.Principals.Count);
            Assert.Equal("delete", permissionDTO.Operation);
            Assert.Equal("Allowed", permissionDTO.Type);
        }

        [Fact]
        public void ToPermissionThrowsOnIllegalTypeOk()
        {
            var permissionDto = new PermissionDTO
            {
                Principals = new List<string> { "Administrators", "John.Doe" },
                Operation = "read",
                Type = "IllegalType"
            };

            Assert.Throws<ArgumentException>(() => permissionDto.ToPermission());
        }

        [Fact]
        public void ToPermissionIsOk()
        {
            var permissionDto = new PermissionDTO
            {
                Principals = new List<string> {"Administrators", "John.Doe"},
                Operation = "read"
            };

            var permission = permissionDto.ToPermission();
            Assert.Equal(PermissionType.Allowed, permission.Type);
            Assert.Equal(permissionDto.Operation, permission.Operation);
            Assert.Equal(permissionDto.Principals, permission.Principals);
        }

        [Fact]
        public void ToDeniedPermissionIsOk()
        {
            var permissionDto = new PermissionDTO
            {
                Principals = new List<string> { "Administrators", "John.Doe" },
                Operation = "read",
                Type = "Denied"
            };

            var permission = permissionDto.ToPermission();
            Assert.Equal(PermissionType.Denied, permission.Type);
            Assert.Equal(permissionDto.Operation, permission.Operation);
            Assert.Equal(permissionDto.Principals, permission.Principals);
        }
    }
}