namespace DHI.Services.GIS.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Spatial;
    using WebApiCore;
    using Xunit;
    using Attribute = Spatial.Attribute;

    [Collection("Controllers collection")]
    public class GISControllerTest
    {
        public GISControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
            _options = new JsonSerializerOptions(_fixture.SerializerOptions);
        }

        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;

        private const string _connectionId = "mclite";

        [Fact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/featurecollections/{_connectionId}/RainStation");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/NonExistingId");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetStreamForNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/stream/NonExistingId");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync($"api/featurecollections/{_connectionId}/NonExistingId");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/count");
            var actual = await DeserializeFromResponseAsync<int>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(actual > 0);
        }

        [Fact]
        public async Task GetIdsIsOk()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/ids");
            var actual = await DeserializeFromResponseAsync<string[]>(response);
            var expected = new string[] { "RainStation", "Roads", "New group/Reservoirs" };

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Contains(expected[0], actual);
            Assert.Contains(expected[1], actual);
            Assert.Contains(expected[2], actual);
        }


        [Fact]
        public async Task GetGeometryTypesIsOk()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/geometrytypes");
            var actual = await DeserializeFromResponseAsync<Dictionary<string, string[]>>(response, _options);
            var expected = new Dictionary<string, string[]>()
            {
                { "New group/Reservoirs", new[] { "MultiPolygon" } },
                { "Roads", new[] { "MultiLineString" } },
                { "RainStation", new[] { "Point" } },
            };

            foreach (var kvp in expected)
            {
                Assert.True(actual.ContainsKey(kvp.Key));
            }
        }

        [Fact]
        public async Task GetPropertiesIsOk()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/properties/RainStation");
            var actual = await DeserializeFromResponseAsync<FeatureCollectionInfo<string>>(response, _options);
            var expected = new FeatureCollectionInfo<string>("a948160f-2a06-4dfc-8c26-d15f7442daa4", "RainStation");

            Assert.Equal(actual.FullName, expected.FullName);
            Assert.Equal(actual.Id, expected.Id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/RainStation");
            var actual = await DeserializeFromResponseAsync<IFeatureCollection>(response, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(actual.Features);
        }

        [Fact]
        public async Task GetFeatureInfoIsOk()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/RainStation/feature/41b28d3d-3130-4d3c-b923-3b1a5e5f13cb?geometry=false");
            var actual = await DeserializeFromResponseAsync<FeatureInfo>(response, _options);
            var expected = new Dictionary<string, object>()
            {
                { "id",  new Guid("41b28d3d-3130-4d3c-b923-3b1a5e5f13cb") },
            };

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expected["id"], actual.AttributeValues["id"]);
        }

        [Fact]
        public async Task GetStreamIsOk()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/RainStation/stream");
            Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
        }

        [Fact]
        public async Task GetByQueryStringIsOk()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/RainStation?id_1=OXR020");
            var actual = await DeserializeFromResponseAsync<IFeatureCollection>(response, _options);
            var expected = new Dictionary<string, object>()
            {
                { "id_1", "OXR020" },
            };

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(actual.Features);
            Assert.Equal(expected["id_1"], actual.Features[0].AttributeValues["id_1"]);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}");
            var actual = await DeserializeFromResponseAsync<IList<IFeatureCollection>>(response, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(actual.Count > 0);
            Assert.NotNull(actual[0].Features);
            Assert.True(actual[0].Features.Count > 0);
        }

        [Fact]
        public async Task GetListIsOk()
        {
            // Arrange
            var expected = new string[]
            {
                "New group/Reservoirs",
                "Roads",
                "RainStation"
            };

            var request = new
            {
                Url = $"api/featurecollections/{_connectionId}/list",
                Body = expected
            };

            // Act
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            // Assert
            var actual = await DeserializeFromResponseAsync<Dictionary<string, FeatureCollection>>(response, _options);

            Assert.Equal(expected.Count(), actual.Count());
            foreach (var instance in expected)
            {
                Assert.True(actual.ContainsKey(instance));
                Assert.True(actual[instance].Features.Count > 0);
                Assert.True(actual[instance].Attributes.Count > 0);
            }
        }

        [Fact]
        public async Task GetGeometryCollectionIsOk()
        {
            var response = await _client.GetAsync($"api/geometrycollections/{_connectionId}/Roads");
            var actual = await DeserializeFromResponseAsync<GeometryCollection>(response, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(actual.Geometries.Count > 0);
            Assert.True(actual.Geometries.All(a => a.Type == "MultiLineString"));
        }

        [Fact]
        public async Task GetGeometryCollectionByQueryStringIsOk()
        {
            var response = await _client.GetAsync($"api/geometrycollections/{_connectionId}/RainStation?id_1=OXR020");
            var actual = await DeserializeFromResponseAsync<GeometryCollection>(response, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(actual.Geometries);
            Assert.Equal("Point", actual.Geometries[0].Type);
        }

        [Fact]
        public async Task AddUpdateDeleteOk()
        {
            var collection = new FeatureCollection();
            var feature = new Feature(Geometry.FromWKT("POINT (12.5683 55.6761)"));
            feature.AttributeValues.Add("id", System.Guid.NewGuid());
            feature.AttributeValues.Add("name", "Copenhagen");
            collection.Features.Add(feature);
            feature = new Feature(Geometry.FromWKT("POINT (10.7522 59.9139)"));
            feature.AttributeValues.Add("id", System.Guid.NewGuid());
            feature.AttributeValues.Add("name", "Oslo");
            collection.Features.Add(feature);
            collection.Attributes.Add(new Attribute("id", typeof(System.Guid), 16));
            collection.Attributes.Add(new Attribute("name", typeof(string), 256));

            var json = JsonSerializer.Serialize(collection, _options);
            var fullNameUrl = FullNameString.ToUrl("Cities/Scandinavia");

            // Add
            var request = new
            {
                Url = $"api/featureCollections/{_connectionId}/{fullNameUrl}",
                Body = json
            };
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            json = await response.Content.ReadAsStringAsync();
            var featureCollection = JsonSerializer.Deserialize<IFeatureCollection>(json, _options);

            Assert.NotNull(featureCollection);
            Assert.Equal(2, featureCollection.Features.Count);
            Assert.Equal(2, featureCollection.Attributes.Count);

            // Update
            collection.Features.RemoveAt(0);
            json = JsonSerializer.Serialize(collection, _options);
            request = new
            {
                Url = $"api/featureCollections/{_connectionId}/{fullNameUrl}",
                Body = json
            };
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            json = await response.Content.ReadAsStringAsync();
            featureCollection = JsonSerializer.Deserialize<IFeatureCollection>(json, _options);

            Assert.Equal(1, featureCollection.Features.Count);
            Assert.Equal(2, featureCollection.Attributes.Count);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Tests that serialized object received from Get is acceptable for Post
        /// </summary>
        /// <remarks>Tests resolution to Issue #44 on GitHub</remarks>
        [Fact]
        public async Task SerializationIsConsistent()
        {
            // Arrange
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/Roads");
            var actual = await DeserializeFromResponseAsync<IFeatureCollection>(response, _options);

            var request = new
            {
                Url = $"api/featureCollections/{_connectionId}/RoadsCopy",
                Body = await response.Content.ReadAsStringAsync(),
            };

            // Act
            response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            var expected = await DeserializeFromResponseAsync<IFeatureCollection>(response, _options);

            // Assert
            Assert.Equal(expected.Features.Count, actual.Features.Count);
            Assert.Equal(expected.Attributes.Count, actual.Attributes.Count);
            Assert.NotStrictEqual(expected.Attributes, actual.Attributes);
        }

        [Fact]
        public async Task AddFeatureIsOk()
        {
            // Arrange, Add
            var request = new
            {
                Url = $"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature",
                Body = JsonSerializer.Serialize(FeatureTypeFixture(), _options)
            };

            // Act
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task UpdateFeatureIsOk()
        {
            // Arrange, Create
            var create = new
            {
                Url = $"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature",
                Body = JsonSerializer.Serialize(FeatureTypeFixture(), _options)
            };

            var response = await _client.PostAsync(create.Url, ContentHelper.GetStringContent(create.Body));
            var created = await DeserializeFromResponseAsync<IFeature>(response, _options);

            created.AttributeValues["name"] = "Other Station";
            created.Geometry = Geometry.FromWKT("POINT (12.4583 55.7781)");

            var update = new
            {
                Url = $"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature",
                Body = JsonSerializer.Serialize(created, _options)
            };

            // Act, Update
            response = await _client.PutAsync(update.Url, ContentHelper.GetStringContent(update.Body));

            // Assert
            var updated = await DeserializeFromResponseAsync<IFeature>(response, _options);

            Assert.Equal(created.AttributeValues["id"], updated.AttributeValues["id"]);
            Assert.Equal("Other Station", updated.AttributeValues["name"]);
            Assert.IsType<Point>(updated.Geometry);
            Assert.Equal(12.4583, updated.Geometry.Coordinates.X, 4);
        }

        [Fact]
        public async Task DeleteFeatureIsOk()
        {
            // Arrange, Create
            var create = new
            {
                Url = $"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature",
                Body = JsonSerializer.Serialize(FeatureTypeFixture(), _options)
            };

            var response = await _client.PostAsync(create.Url, ContentHelper.GetStringContent(create.Body));
            var created = await DeserializeFromResponseAsync<IFeature>(response, _options);

            // Act, Delete
            response = await _client.DeleteAsync($"{create.Url}/{created.AttributeValues["id"]}");

            // Assert, Get
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{create.Url}/{created.AttributeValues["id"]}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Fact]
        public async Task GetFeatureIdsOk()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/ids");
            var actual = await DeserializeFromResponseAsync<Guid[]>(response, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(new Guid("79ebf100-f3a2-489d-a0c7-2b99b3ae4e76"), actual);
            Assert.True(actual.Length > 0);
        }


        [Fact]
        public async Task GetFeatureOk()
        {
            var featureGuid = new Guid("79ebf100-f3a2-489d-a0c7-2b99b3ae4e76");
            var response = await _client.GetAsync($"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureGuid}");
            var actual = await DeserializeFromResponseAsync<IFeature>(response, _options);
            var expected = new Position(153.075, -27.402);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(featureGuid, (Guid)actual.AttributeValues["id"]);
            Assert.NotNull(actual.Geometry);
            Assert.IsType<Point>(actual.Geometry);
            Assert.Equal(expected.X, actual.Geometry.Coordinates.X, 3);
            Assert.Equal(expected.Y, actual.Geometry.Coordinates.Y, 3);
        }


        [Fact]
        public async Task GetAllAttributesNoContent()
        {
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/attribute/");
            var actual = await DeserializeFromResponseAsync<IList<IAttribute>>(response, _options);

            Assert.True(actual.Count > 0);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        [Fact]
        public async Task GetNameAttributeByIndex()
        {
            var index = 4; // 4 is index for name Attribute Value
            var response = await _client.GetAsync($"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/attribute-index/{index}");
            var actual = await DeserializeFromResponseAsync<IAttribute>(response, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(actual.Name == "name");
        }


        [Fact]
        public async Task UpdateAttributeValueForFeature()
        {
            // Arrange
            var featureId = "79ebf100-f3a2-489d-a0c7-2b99b3ae4e76";
            var index = "4"; // 4 is index for the name attribute

            var update = new
            {
                Url = $"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureId}/attribute/{index}",
                Body = JsonSerializer.Serialize(new { value = $"attribute name change {featureId}" }, _options)
            };

            // Act
            _ = await _client.PutAsync(update.Url, ContentHelper.GetStringContent(update.Body));

            // Assert
            var response = await _client.GetAsync($"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureId}");
            var actual = await DeserializeFromResponseAsync<IFeature>(response, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal($"\"attribute name change {featureId}\"", actual.AttributeValues["name"]);
        }


        [Fact]
        public async Task UpdateAttributeValueForFeatures()
        {
            // Arrange
            var index = "4"; // 4 is index for the name attribute
            var featureIds = new List<Guid>
            {
                new Guid("79ebf100-f3a2-489d-a0c7-2b99b3ae4e76"),
                new Guid("41b28d3d-3130-4d3c-b923-3b1a5e5f13cb")
            };

            var update = new
            {
                Url = $"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/attribute/{index}",
                Body = JsonSerializer.Serialize(new { value = $"attribute name change {featureIds[0]}", featureIds = featureIds }, _options)
            };

            // Act
            _ = await _client.PutAsync(update.Url, ContentHelper.GetStringContent(update.Body));

            // Assert
            var response = await _client.GetAsync($"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureIds[0]}");
            var actual = await DeserializeFromResponseAsync<IFeature>(response, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(actual.AttributeValues);
            Assert.Equal($"\"attribute name change {featureIds[0]}\"", actual.AttributeValues["name"]);
        }



        [Fact]
        public async Task UpdateAttributeValueByNameForFeature()
        {
            // Arrange
            var featureId = "79ebf100-f3a2-489d-a0c7-2b99b3ae4e76";
            var attributeName = "name";

            var update = new
            {
                Url = $"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureId}/attribute-by-name/{attributeName}",
                Body = JsonSerializer.Serialize(new { value = $"attribute name change {featureId}" }, _options)
            };

            // Act
            _ = await _client.PutAsync(update.Url, ContentHelper.GetStringContent(update.Body));

            // Assert
            var response = await _client.GetAsync($"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureId}");
            var actual = await DeserializeFromResponseAsync<IFeature>(response, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(actual.AttributeValues);
            Assert.Equal($"\"attribute name change {featureId}\"", actual.AttributeValues["name"]);
        }


        [Fact]
        public async Task UpdateAttributeValueByNameForFeatures()
        {
            // Arrange
            var featureIds = new List<Guid>
            {
                new Guid("79ebf100-f3a2-489d-a0c7-2b99b3ae4e76"),
                new Guid("41b28d3d-3130-4d3c-b923-3b1a5e5f13cb")
            };
            var attributeName = "name";

            var update = new
            {
                Url = $"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/attribute-by-name/{attributeName}",
                Body = JsonSerializer.Serialize(new { value = $"attribute name change {featureIds[0]}", featureIds = featureIds }, _options)
            };

            // Act
            _ = await _client.PutAsync(update.Url, ContentHelper.GetStringContent(update.Body));

            //Assert
            var response = await _client.GetAsync($"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureIds[0]}");
            var actual = await DeserializeFromResponseAsync<IFeature>(response, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(actual.AttributeValues);
            Assert.Equal($"\"attribute name change {featureIds[0]}\"", actual.AttributeValues["name"]);
        }


        [Fact]
        public async Task UpdateAttributeValuesForFeatureNoContent()
        {
            // Arrange
            var featureId = "79ebf100-f3a2-489d-a0c7-2b99b3ae4e76";

            var featureAttritube = new
            {
                attributes = new Dictionary<int, object>
                {
                    { 4, $"attribute name change {featureId}" } // 4 is index for the name attribute
                }
            };

            var update = new
            {
                Url = $"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureId}/attributes/",
                Body = ContentHelper.GetStringContent(featureAttritube)
            };

            // Act
            _ = await _client.PutAsync(update.Url, update.Body);

            // Assert
            var response = await _client.GetAsync($"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureId}");
            var actual = await DeserializeFromResponseAsync<IFeature>(response, _options);

            Assert.NotNull(actual.AttributeValues);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal($"attribute name change {featureId}", actual.AttributeValues["name"]);
        }

        /// <summary>
        /// Update Attribute Value for Feature with Feature Id.
        /// GISController Endpoint target is "UpdateAttributeValuesByNameForFeature".
        /// </summary>
        [Fact] // success & tidy up
        public async Task UpdateAttributeValuesByNameForFeatureWithFeatureId()
        {
            // Arrange
            var featureId = "79ebf100-f3a2-489d-a0c7-2b99b3ae4e76";

            var featureAttritube = new
            {
                attributes = new Dictionary<string, object>
                {
                    { "name", $"attribute name change {featureId}" }
                }
            };

            var update = new
            {
                Url = $"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureId}/attributes-by-name/",
                Body = ContentHelper.GetStringContent(featureAttritube)
            };

            // Act
            _ = await _client.PutAsync(update.Url, update.Body);

            // Assert
            var response = await _client.GetAsync($"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureId}");
            var actual = await DeserializeFromResponseAsync<IFeature>(response, _options);

            Assert.NotNull(actual.AttributeValues);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal($"attribute name change {featureId}", actual.AttributeValues["name"]);
        }

        /// <summary>
        /// Update Attribute Value for multiple Features with FeatureIds.
        /// GISController Endpoint target is "UpdateAttributeValuesByNameForFeatures".
        /// </summary>
        [Fact]
        public async Task UpdateAttributeValuesByNameForFeaturesWithFeatureIds()
        {
            // Arrange
            var featureIds = new List<Guid>
            {
                new Guid("79ebf100-f3a2-489d-a0c7-2b99b3ae4e76"),
                new Guid("41b28d3d-3130-4d3c-b923-3b1a5e5f13cb")
            };

            var featureAttribute = new
            {
                attributes = new Dictionary<string, object>
                {
                    { "name", $"attribute name change {string.Join(", ", featureIds)}" }
                },
                featureIds = featureIds
            };

            var update = new
            {
                Url = $"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/attributes-by-name/",
                Body = ContentHelper.GetStringContent(featureAttribute)
            };

            // Act
            _ = await _client.PutAsync(update.Url, update.Body);

            // Assert
            foreach (var featureId in featureIds)
            {
                var response = await _client.GetAsync($"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureId}");
                var actual = await DeserializeFromResponseAsync<IFeature>(response, _options);

                Assert.NotNull(actual.AttributeValues);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Matches($"attribute name change {string.Join(", ", featureIds)}", actual.AttributeValues["name"].ToString());
            }
        }

        /// <summary>
        /// Update Attribute Value with Index for multiple Features with Feature Ids. 
        /// GISController Endpoint target is "UpdateAttributeValuesForFeatures".
        /// </summary>
        [Fact]
        public async Task UpdateAttributeValuesForFeaturesWithFeatureIds()
        {
            // Arrange
            var featureIds = new List<Guid>
            {
                new Guid("79ebf100-f3a2-489d-a0c7-2b99b3ae4e76"),
                new Guid("41b28d3d-3130-4d3c-b923-3b1a5e5f13cb")
            };

            var featureAttribute = new
            {
                attributes = new Dictionary<int, object>
                {
                    { 4, $"attribute name change {string.Join(", ", featureIds)}"} // 4 is index for the name attribute
                },
                featureIds = featureIds
            };

            var update = new
            {
                Url = $"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/attributes/",
                Body = ContentHelper.GetStringContent(featureAttribute)
            };

            // Act
            _ = await _client.PutAsync(update.Url, update.Body);

            // Assert
            foreach (var featureId in featureIds)
            {
                var response = await _client.GetAsync($"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureId}");
                var actual = await DeserializeFromResponseAsync<IFeature>(response, _options);

                Assert.NotNull(actual.AttributeValues);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Matches($"attribute name change {string.Join(", ", featureIds)}", actual.AttributeValues["name"].ToString());
            }
        }

        /// <summary>
        /// Update Attribute Value for multiple Features with Attribute Name & Where condition.
        /// GISController Endpoint target is "UpdateAttributeValuesByNameForFeaturesWhere".
        /// </summary>
        [Fact]
        public async Task UpdateAttributeValuesByNameForFeaturesWhereAttributeNameCondition()
        {
            // Arrange
            var featureIds = new List<Guid>
            {
                new Guid("79ebf100-f3a2-489d-a0c7-2b99b3ae4e76"),
                new Guid("2868d90d-b94f-4bb0-b568-94d56034a93b")
            };

            var queryFilter = new List<QueryCondition>()
            {
                new QueryCondition("name", QueryOperator.Equal, "Pound Ck @ Hendra Pony Club D/S Nudgee Rd (PDA845 PDR844)"),
                new QueryCondition("name", QueryOperator.Equal, "Oxley Ck @ Calamavale Telecom OXR114")
            };

            var valueParameter = new
            {
                attributes = new Dictionary<string, object>
                {
                    { "id1", 48},
                },
                filter = queryFilter
            };

            var update = new
            {
                Url = $"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/attributes-by-name-where/",
                Body = ContentHelper.GetStringContent(valueParameter)
            };

            // Act
            _ = await _client.PutAsync(update.Url, update.Body);

            // Assert
            var response = await _client.GetAsync($"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureIds[0]}");
            var actual = await DeserializeFromResponseAsync<IFeature>(response, _options);

            Assert.NotNull(actual.AttributeValues);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(48, actual.AttributeValues.FirstOrDefault(kv => kv.Key == "id1").Value);
        }

        /// <summary>
        /// Update Attribute Value for multiple Features with Feature Ids Where condition.
        /// GISController Endpoint target is "UpdateAttributeValuesForFeaturesWhere".
        /// </summary>
        [Fact]
        public async Task UpdateAttributeValuesForFeaturesWithFeatureIdsWhereCondition()
        {
            // Arrange
            var featureIds = new List<Guid>
            {
                new Guid("79ebf100-f3a2-489d-a0c7-2b99b3ae4e76"),
                new Guid("2868d90d-b94f-4bb0-b568-94d56034a93b")
            };

            var queryFilter = new List<QueryCondition>()
            {
                new QueryCondition("id", QueryOperator.Equal, featureIds[0]),
                new QueryCondition("id", QueryOperator.Equal, featureIds[1])
            };

            var valueParameter = new
            {
                attributes = new Dictionary<int, object>
                {
                    { 4, $"attribute name change {string.Join(", ", featureIds)}" } // index is coresponding to column name database
                },
                filter = queryFilter
            };

            var update = new
            {
                Url = $"api/featurecollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/attributes-where/",
                Body = ContentHelper.GetStringContent(valueParameter)
            };

            // Act
            _ = await _client.PutAsync(update.Url, update.Body);

            // Assert
            foreach (var featureId in featureIds)
            {
                var response = await _client.GetAsync($"api/featureCollections/{_connectionId}/{FullNameString.ToUrl("RainStation")}/feature/{featureId}");
                var actual = await DeserializeFromResponseAsync<IFeature>(response, _options);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.NotNull(actual.AttributeValues);
                Assert.Equal($"attribute name change {string.Join(", ", featureIds)}", actual.AttributeValues["name"].ToString());
            }
        }




        /// <summary>
        /// Serializes the Response coming from the HttpResponseMessage
        /// </summary>
        /// <typeparam name="T">The type you wish to deserialize to</typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task<T> DeserializeFromResponseAsync<T>(HttpResponseMessage response, JsonSerializerOptions options = null)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, options);
        }

        private async Task<T> DeserializeFromResponseAsync<T>(HttpResponseMessage response)
        {
            return await DeserializeFromResponseAsync<T>(response, _options);
        }

        private Feature FeatureTypeFixture()
        {
            var point = Geometry.FromWKT("POINT (12.5683 55.6761)");
            var feature = new Feature(point);
            var featureId = System.Guid.NewGuid();
            feature.AttributeValues.Add("id", featureId);
            feature.AttributeValues.Add("id1", 52);
            feature.AttributeValues.Add("x", 12.5683);
            feature.AttributeValues.Add("y", 55.6761);
            feature.AttributeValues.Add("name", "Dummy Station");
            feature.AttributeValues.Add("id_1", "Dummy");

            return feature;
        }
    }
}