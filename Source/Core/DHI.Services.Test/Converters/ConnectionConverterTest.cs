namespace DHI.Services.Test.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using DHI.Services.Converters;
    using Xunit;

    public class ConnectionConverterTest
    {
        private readonly JsonSerializerOptions _options;

        public ConnectionConverterTest()
        {
            _options = new JsonSerializerOptions
            {
                Converters = {
                    new ConnectionConverter(),
                    new ConnectionDictionaryConverter()
                }
            };
        }

        [Fact]
        public void CanConvert()
        {
            Assert.True(new ConnectionConverter().CanConvert(typeof(IConnection)));
            Assert.True(new ConnectionDictionaryConverter().CanConvert(typeof(IDictionary<string, IConnection>)));
        }

        [Fact]
        public void CanSerializeConnectionWithTypeDiscriminator()
        {
            var id = Guid.NewGuid().ToString("N");
            var connection = new FakeConnection(id, $"{typeof(FakeConnection).Name}-{id}");
            connection.AddPermissions(new[] { "Administrators" }, new[] { "read", "update", "delete" });

            var json = JsonSerializer.Serialize(connection, _options);

            var expected = "{\"$type\":\"DHI.Services.Test.FakeConnection, DHI.Services.Test\",\"Name\":\"FakeConnection-" + id + "\",\"Permissions\":[{\"Principals\":[\"Administrators\"],\"Operation\":\"read\",\"Type\":0},{\"Principals\":[\"Administrators\"],\"Operation\":\"update\",\"Type\":0},{\"Principals\":[\"Administrators\"],\"Operation\":\"delete\",\"Type\":0}],\"Id\":\"" + id + "\",\"Added\":null,\"Updated\":null,\"Metadata\":{}}";

            Assert.Equal(expected, json);
        }

        [Fact]
        public void CanDeserializeConnectionWithTypeDiscriminator()
        {
            var id = Guid.NewGuid().ToString("N");
            var json = "{\"$type\":\"DHI.Services.Test.FakeConnection, DHI.Services.Test\",\"Name\":\"FakeConnection-" + id + "\",\"Permissions\":[{\"Principals\":[\"Administrators\"],\"Operation\":\"read\",\"Type\":0},{\"Principals\":[\"Administrators\"],\"Operation\":\"update\",\"Type\":0},{\"Principals\":[\"Administrators\"],\"Operation\":\"delete\",\"Type\":0}],\"Id\":\"" + id + "\",\"Added\":null,\"Updated\":null,\"Metadata\":{}}";
            var actual = JsonSerializer.Deserialize<FakeConnection>(json, _options);

            var expected = new FakeConnection(id, $"{typeof(FakeConnection).Name}-{id}");
            expected.AddPermissions(new[] { "Administrators" }, new[] { "read", "update", "delete" });

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            Assert.All(actual.Permissions, x => Assert.Contains(x, expected.Permissions));
        }

        [Fact]
        public void CanDeserializeInterfaceConnectionUsingTypeDiscriminator()
        {
            var id = Guid.NewGuid().ToString("N");
            var json = "{\"$type\":\"DHI.Services.Test.FakeConnection, DHI.Services.Test\",\"Name\":\"FakeConnection-" + id + "\",\"Permissions\":[{\"Principals\":[\"SuperUser\"],\"Operation\":\"read\",\"Type\":0},{\"Principals\":[\"SuperUser\"],\"Operation\":\"update\",\"Type\":0},{\"Principals\":[\"SuperUser\"],\"Operation\":\"delete\",\"Type\":0}],\"Id\":\"" + id + "\",\"Added\":null,\"Updated\":null,\"Metadata\":{}}";

            var actual = JsonSerializer.Deserialize<IConnection>(json, _options);

            var expected = new FakeConnection(id, $"{typeof(FakeConnection).Name}-{id}");
            expected.AddPermissions(new[] { "SuperUser" }, new[] { "read", "update", "delete" });

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
            Assert.All(actual.Permissions, x => Assert.Contains(x, expected.Permissions));
        }

        [Fact]
        public void CanDeserializeDictionaryConnectionUsingTypeDiscriminator()
        {
            var json = "{\"$type\":\"System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[DHI.Services.IConnection, DHI.Services]], mscorlib\",\"connection-1\":{\"$type\":\"DHI.Services.Test.FakeConnection, DHI.Services.Test\",\"Name\":\"FakeConnection-1\",\"Permissions\":[{\"Principals\":[\"Administrators\"],\"Operation\":\"read\",\"Type\":0},{\"Principals\":[\"Administrators\"],\"Operation\":\"update\",\"Type\":0},{\"Principals\":[\"Administrators\"],\"Operation\":\"delete\",\"Type\":0}],\"Id\":\"1\",\"Added\":null,\"Updated\":null,\"Metadata\":{}},\"connection-2\":{\"$type\":\"DHI.Services.Test.FakeConnection, DHI.Services.Test\",\"Name\":\"FakeConnection-2\",\"Id\":\"2\",\"Added\":null,\"Updated\":null,\"Metadata\":{}},\"connection-3\":{\"$type\":\"DHI.Services.Test.FakeConnection, DHI.Services.Test\",\"Name\":\"FakeConnection-3\",\"Id\":\"3\",\"Added\":null,\"Updated\":null,\"Metadata\":{}}}";

            var actual = JsonSerializer.Deserialize<IDictionary<string, IConnection>>(json, _options);

            Assert.Equal(3, actual.Count);
            Assert.All(actual.Values, x => Assert.IsType<FakeConnection>(x));
        }
    }
}
