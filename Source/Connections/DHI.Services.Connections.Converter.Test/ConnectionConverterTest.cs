namespace DHI.Services.Connections.Converter.Test
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using DHI.Services.GIS;
    using Xunit;

    public class ConnectionConverterTest : TestFixtureBase
    {
        public ConnectionConverterTest()
        {
        }

        [Fact]
        public void DeserializeConnectionSettingOk()
        {
            var connPath = Path.Combine(TempAppDataPath, "connections.json");
            var json = File.ReadAllText(connPath);

            var connections = JsonSerializer.Deserialize<IDictionary<string, IConnection>>(json, SerializerOptions);
            Assert.NotNull(connections);
        }

        [Fact]
        public void DeserializeConnectionOk()
        {
            var json = "{\"$type\":\"DHI.Services.GIS.GisServiceConnection<System.String>, DHI.Services.GIS\",\"ConnectionString\":\"[AppData]shp\",\"RepositoryType\":\"DHI.Services.Provider.ShapeFile.FeatureRepository, DHI.Services.Provider.ShapeFile\",\"Name\":\"Shape file gis connection\",\"Id\":\"shape\"}";

            var connections = JsonSerializer.Deserialize<GisServiceConnection<string>>(json, SerializerOptions);
            Assert.NotNull(connections);
        }

        [Fact]
        public void SerializedConnectionOk()
        {
            IConnection conn = new GisServiceConnection<string>("shape", "Shape file gis connection")
            {
                ConnectionString = "[AppData]shp",
                RepositoryType = "DHI.Services.Provider.ShapeFile.FeatureRepository, DHI.Services.Provider.ShapeFile"
            };

            var json = JsonSerializer.Serialize(conn, SerializerOptions);

            var expected = "{\"$type\":\"DHI.Services.GIS.GisServiceConnection, DHI.Services.GIS\",\"ConnectionString\":\"[AppData]shp\",\"RepositoryType\":\"DHI.Services.Provider.ShapeFile.FeatureRepository, DHI.Services.Provider.ShapeFile\",\"Name\":\"Shape file gis connection\",\"Id\":\"shape\"}";

            Assert.Equal(expected, json);
        }
    }
}
