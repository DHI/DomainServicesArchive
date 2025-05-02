namespace DHI.Services.Test.Converters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using DHI.Services.Converters;
    using Xunit;

    public class PermissionsConverterTest
    {
        private readonly JsonSerializerOptions _options;

        public PermissionsConverterTest()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new PermissionConverter() }
            };
        }

        [Fact]
        public void CanConvert()
        {
            Assert.True(new PermissionConverter().CanConvert(typeof(DHI.Services.Authorization.Permission)));
        }

        [Fact]
        public void CanSerializePermissions()
        {
            var permissions = new List<DHI.Services.Authorization.Permission>
            {
                new DHI.Services.Authorization.Permission(new[] { "Administrator" }, "Read", DHI.Services.Authorization.PermissionType.Allowed),
                new DHI.Services.Authorization.Permission(new[] { "Administrator" }, "Update", DHI.Services.Authorization.PermissionType.Allowed),
                new DHI.Services.Authorization.Permission(new[] { "Administrator" }, "Delete", DHI.Services.Authorization.PermissionType.Allowed),
                new DHI.Services.Authorization.Permission(new[] { "Administrator" }, "Insert", DHI.Services.Authorization.PermissionType.Allowed)
            };

            var actual = JsonSerializer.Serialize(permissions, _options);

            var expected = "[{\"operation\":\"read\",\"type\":\"Allowed\",\"principals\":[\"Administrator\"]},{\"operation\":\"update\",\"type\":\"Allowed\",\"principals\":[\"Administrator\"]},{\"operation\":\"delete\",\"type\":\"Allowed\",\"principals\":[\"Administrator\"]},{\"operation\":\"insert\",\"type\":\"Allowed\",\"principals\":[\"Administrator\"]}]";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanDeserializePermissions()
        {
            var actual = new List<DHI.Services.Authorization.Permission>
            {
                new DHI.Services.Authorization.Permission(new[] { "Administrator" }, "Read", DHI.Services.Authorization.PermissionType.Allowed),
                new DHI.Services.Authorization.Permission(new[] { "Administrator" }, "Update", DHI.Services.Authorization.PermissionType.Allowed),
                new DHI.Services.Authorization.Permission(new[] { "Administrator" }, "Delete", DHI.Services.Authorization.PermissionType.Allowed),
                new DHI.Services.Authorization.Permission(new[] { "Administrator" }, "Insert", DHI.Services.Authorization.PermissionType.Allowed)
            };

            var json = "[{\"operation\":\"read\",\"type\":\"Allowed\",\"principals\":[\"Administrator\"]},{\"operation\":\"update\",\"type\":\"Allowed\",\"principals\":[\"Administrator\"]},{\"operation\":\"delete\",\"type\":\"Allowed\",\"principals\":[\"Administrator\"]},{\"operation\":\"insert\",\"type\":\"Allowed\",\"principals\":[\"Administrator\"]}]";

            var expected = JsonSerializer.Deserialize<List<DHI.Services.Authorization.Permission>>(json, _options);

            Assert.Equal(expected.Count, actual.Count);
            Assert.All(actual, x => Assert.Contains(x.ToString(), expected.Select(x => x.ToString()).ToArray()));
        }
    }
}
