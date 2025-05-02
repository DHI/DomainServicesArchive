// ReSharper disable UnusedMember.Global
namespace DHI.Services.Meshes.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using AutoFixture.Xunit2;
    using Provider.MIKECore;
    using Xunit;

    public class GetContoursTest : IDisposable
    {
        private readonly string _jsonFilePath = Path.Combine(Path.GetTempPath(), "PY_F012.json");
        private readonly string _rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"../../../../Data"));
        private readonly MeshService _meshService;

        public GetContoursTest()
        {
            _meshService = new MeshService(new DfsuMeshRepository(new FileSource(_rootPath)));
        }

        [Theory, AutoData]
        public void GetContoursForNonExistingThrows(string id)
        {
            var thresholdValues = new[] { 0.1, 0.25, 0.5, 0.75, 1.0, 2.0, 3.0 };
            var e = Assert.Throws<KeyNotFoundException>(() =>
                _meshService.GetContours(id, "1", thresholdValues, new DateTime(2014, 1, 1)));
            Assert.Contains($"Mesh with id '{id}' was not found.", e.Message);
        }

        [Theory, AutoData]
        public void GetContoursForNonExistingDateTimeThrows(DateTime dateTime)
        {
            var thresholdValues = new[] { 0.1, 0.25, 0.5, 0.75, 1.0, 2.0, 3.0 };
            var e = Assert.Throws<ArgumentException>(() =>
                _meshService.GetContours("PY_F012.dfsu", "1", thresholdValues, dateTime));
            Assert.Contains($"DateTime {dateTime} was not found.", e.Message);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("Sign. Wave Height")]
        public void GetContoursIsOk(string item)
        {
            var thresholdValues = new [] { 0.1, 0.25, 0.5, 0.75, 1.0, 2.0, 3.0 };
            var featureCollection = _meshService.GetContours("PY_F012.dfsu", item, thresholdValues, new DateTime(2014, 1, 1));

            Assert.True(featureCollection.Features.Any());

            var jsonSerializerSettings = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            jsonSerializerSettings.Converters.Add(new Converters.FeatureCollectionConverter());
            File.WriteAllText(_jsonFilePath, JsonSerializer.Serialize(featureCollection, jsonSerializerSettings));

            var expected = File.ReadAllText(Path.Combine(_rootPath, "PY_F012.json"));
            var actual = File.ReadAllText(_jsonFilePath);
            Assert.Equal(expected, actual);
        }

        public void Dispose()
        {
            File.Delete(_jsonFilePath);
        }
    }
}
