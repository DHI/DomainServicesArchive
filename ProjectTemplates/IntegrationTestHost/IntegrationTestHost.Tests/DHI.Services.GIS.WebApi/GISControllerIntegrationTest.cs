namespace IntegrationTestHost.Tests
{
    using DHI.Services;
    using DHI.Services.WebApiCore;
    using DHI.Spatial;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class GISControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;

        private const string ConnectionId = "gis-mclite";
        private const string FeatureCollectionId = "RainStation";
        private const string ScratchFeatureCollection = "Cities/Scandinavias";

        private IFeature? _createdFeature;
        private FeatureCollection _scratchPayload;
        private int _demoAttributeIndex;

        public GISControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _output = output;
        }

        [Fact(DisplayName = "Run GISController Integration Flow")]
        public async Task Run_GISController_IntegrationFlow()
        {
            // Basic GET checks
            await Step("Get FeatureCollection by ID", () => Get($"api/featurecollections/{ConnectionId}/{FeatureCollectionId}"));
            await Step("Get All FeatureCollection IDs", () => Get($"api/featurecollections/{ConnectionId}/ids"));
            await Step("Get FeatureCollection Count", () => Get($"api/featurecollections/{ConnectionId}/count"));
            await Step("Get FeatureCollection Stream", () => ExpectStatus($"api/featurecollections/{ConnectionId}/{FeatureCollectionId}/stream/", HttpStatusCode.NotImplemented));
            await Step("Get Geometry Collection", () => Get($"api/geometrycollections/{ConnectionId}/{FeatureCollectionId}"));
            await Step("Get FeatureCollection Properties", () => Get($"api/featurecollections/{ConnectionId}/properties/{FeatureCollectionId}"));
            await Step("Get Envelope", () => Get($"api/featurecollections/{ConnectionId}/{FeatureCollectionId}/envelope"));
            await Step("Get Footprint", () => Get($"api/featurecollections/{ConnectionId}/{FeatureCollectionId}/footprint"));
            await Step("Get Feature IDs", () => Get($"api/featurecollections/{ConnectionId}/{FeatureCollectionId}/ids"));
            await Step("Get Feature by ID", GetFeatureById);
            await Step("Get Full Names", () => Get($"api/featurecollections/{ConnectionId}/fullnames"));
            await Step("Get Geometry Types", () => Get($"api/featurecollections/{ConnectionId}/geometrytypes"));
            await Step("Get All FeatureCollections", () => Get($"api/featurecollections/{ConnectionId}"));

            // Query & ID operations
            await Step("Post ID list", PostListByIds);
            await Step("Post Collection Query DTO", PostCollectionQueryDto);

            // Feature operations
            await Step("Add Feature", AddFeature);
            await Step("Update Feature", UpdateFeature);
            await Step("Get All Attributes", GetAllAttributes);
            await Step("Update Feature Attribute Value", UpdateFeatureAttributeValue);
            await Step("Delete Feature", DeleteFeature);
            await Step("Post Footprint List", PostFootprintList);
            await Step("Post Feature Query", PostFeatureQuery);

            // FeatureCollection operations
            await Step("Add FeatureCollection", AddFeatureCollection);
            await Step("Update FeatureCollection", UpdateFeatureCollection);
            await Step("Create Attribute", CreateAttribute);
            await Step("Get Attribute by Index", GetAttributeByIndex);
            // await Step("Update Attribute Meta", UpdateAttributeMeta);
            await Step("Remove Attribute by Name", RemoveAttributeByName);
            await Step("Delete FeatureCollection", DeleteFeatureCollection);

            // Root properties
            await Step("Get Root Properties List", () => Get($"api/featurecollections/{ConnectionId}/properties"));

            await Step("Update Attribute Values for Feature", UpdateAttributeValuesForFeature);
            await Step("Update Attribute Values by Name for Feature", UpdateAttributeValuesByNameForFeature);

            await Step("Update Attribute Value for Features (index)", UpdateAttributeValueForFeatures);
            await Step("Update Attribute Value for Features Where", UpdateAttributeValueForFeaturesWhere);

            await Step("Update Attribute Value by Name for Features", UpdateAttributeValueByNameForFeatures);
            await Step("Update Attribute Value by Name for Features Where", UpdateAttributeValueByNameForFeaturesWhere);

            await Step("Update Attribute Values for Features", UpdateAttributeValuesForFeatures);
            await Step("Update Attribute Values by Name for Features", UpdateAttributeValuesByNameForFeatures);

            await Step("Update Attribute Values for Features Where", UpdateAttributeValuesForFeaturesWhere);
            await Step("Update Attribute Values by Name for Features Where", UpdateAttributeValuesByNameForFeaturesWhere);

            //await Step("Remove Attribute (index)", RemoveAttribute);

            await Step("Remove Features (bulk)", RemoveFeatures);
        }

        // --------------------------
        // Utilities
        // --------------------------
        private async Task Step(string name, Func<Task> action)
        {
            _output.WriteLine($">>> {name}");
            try
            {
                await action();
                _output.WriteLine($"✔ {name}");
            }
            catch
            {
                _output.WriteLine($"✖ {name}");
                throw;
            }
        }

        private async Task Get(string url) => await ExpectStatus(url, HttpStatusCode.OK);

        private async Task ExpectStatus(string url, HttpStatusCode expected)
        {
            var resp = await _client.GetAsync(url);
            Assert.Equal(expected, resp.StatusCode);
        }

        private string Url(string relative) => $"api/featurecollections/{ConnectionId}/{relative}";

        private string UrlEncoded(string name) => FullNameString.ToUrl(name);

        // --------------------------
        // Feature Operations
        // --------------------------
        private async Task GetFeatureById()
        {
            var id = "41b28d3d-3130-4d3c-b923-3b1a5e5f13cb";
            await Get($"{Url($"{FeatureCollectionId}/feature/{id}")}");
        }

        private async Task AddFeature()
        {
            var feature = FeatureTypeFixture();
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/feature");
            var response = await _client.PostAsync(url, GISContentHelper.GetStringContent(feature));

            response.EnsureSuccessStatusCode();
            _createdFeature = await response.Content.ReadFromJsonAsync<Feature>(DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options);
        }

        private async Task UpdateFeature()
        {
            if (_createdFeature is null) throw new InvalidOperationException("Feature must be created first.");

            _createdFeature.AttributeValues["name"] = "Other Station";
            _createdFeature.Geometry = Geometry.FromWKT("POINT (12.4583 55.7781)");

            var url = Url($"{UrlEncoded(FeatureCollectionId)}/feature");
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(_createdFeature));
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        private async Task DeleteFeature()
        {
            var id = _createdFeature!.AttributeValues["id"];
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/feature/{id}");
            var resp = await _client.DeleteAsync(url);
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
        }

        private async Task UpdateFeatureAttributeValue()
        {
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/feature/{_createdFeature!.AttributeValues["id"]}/attribute/4");
            var body = new { value = "integration-rename" };
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        // --------------------------
        // FeatureCollection Operations
        // --------------------------
        private async Task PostListByIds()
        {
            var body = new[] { "RainStation", "Roads" };
            var resp = await _client.PostAsync(Url("list"), GISContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        private async Task PostCollectionQueryDto()
        {
            var dto = new[] { new { item = "id_1", queryOperator = "Equal", value = "OXR020" } };
            var resp = await _client.PostAsync(Url("query"), GISContentHelper.GetStringContent(dto));
            Assert.Equal(HttpStatusCode.NotImplemented, resp.StatusCode);
        }

        private async Task PostFootprintList()
        {
            var body = new[] { "RainStation", "Roads" };
            var resp = await _client.PostAsync(Url("footprint"), GISContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        private async Task PostFeatureQuery()
        {
            var body = new[] { new { item = "id_1", queryOperator = "Equal", value = "OXR020" } };
            var resp = await _client.PostAsync(Url($"{FeatureCollectionId}/query"), GISContentHelper.GetStringContent(body));
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        private async Task AddFeatureCollection()
        {
            var collection = BuildScratchCollection();
            _scratchPayload = collection;

            var url = $"api/featurecollections/{ConnectionId}/{UrlEncoded(ScratchFeatureCollection)}";
            var json = JsonSerializer.Serialize(collection, DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options);
            var response = await _client.PostAsync(url, GISContentHelper.GetStringContent(json));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task UpdateFeatureCollection()
        {
            _scratchPayload.Features.RemoveAt(0);
            var url = $"api/featurecollections/{ConnectionId}/{UrlEncoded(ScratchFeatureCollection)}";
            var json = JsonSerializer.Serialize(_scratchPayload, DHI.Services.GIS.WebApi.SerializerOptionsDefault.Options);
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(json));
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        private async Task DeleteFeatureCollection()
        {
            var url = $"api/featurecollections/{ConnectionId}/{UrlEncoded(ScratchFeatureCollection)}";
            var resp = await _client.DeleteAsync(url);
            Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
        }

        // --------------------------
        // Attribute Operations
        // --------------------------
        private async Task GetAllAttributes()
        {
            await Get($"{Url($"{UrlEncoded(FeatureCollectionId)}/attribute/")}");
        }

        private async Task CreateAttribute()
        {
            var attr = new DHI.Spatial.Attribute("demo", typeof(string), 20);
            var url = $"{Url($"{UrlEncoded(ScratchFeatureCollection)}/attribute/")}";
            var resp = await _client.PostAsync(url, GISContentHelper.GetStringContent(attr));
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var listUrl = $"{Url($"{UrlEncoded(ScratchFeatureCollection)}/attribute/")}";
            var all = await _client.GetFromJsonAsync<List<DHI.Spatial.Attribute>>(listUrl);
            _demoAttributeIndex = all.FindIndex(a => a.Name == "demo");
        }

        private async Task GetAttributeByIndex()
        {
            var url = $"{Url($"{UrlEncoded(ScratchFeatureCollection)}/attribute-index/{_demoAttributeIndex}")}";
            await Get(url);
        }

        private async Task UpdateAttributeMeta()
        {
            var attr = new DHI.Spatial.Attribute("demo", typeof(string), 100);
            var url = $"{Url($"{UrlEncoded(ScratchFeatureCollection)}/attribute/")}";
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(attr));
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        private async Task RemoveAttributeByName()
        {
            var featureId = "79ebf100-f3a2-489d-a0c7-2b99b3ae4e76";
            var attrName = "name";
            var url = $"{Url($"{featureId}/attribute-by-name/{attrName}")}";
            var body = new { value = $"attribute name change {featureId}" };
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(body));
            _output.WriteLine(await resp.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }

        private async Task UpdateAttributeValuesForFeature()
        {
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/feature/{_createdFeature!.AttributeValues["id"]}/attributes/");
            var body = new
            {
                attributes = new Dictionary<int, object> { { 4, "multi-index-update" } } // 4 = “name” column
            };
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(body));
            resp.EnsureSuccessStatusCode();
        }

        private async Task UpdateAttributeValuesByNameForFeature()
        {
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/feature/{_createdFeature!.AttributeValues["id"]}/attributes-by-name/");
            var body = new
            {
                attributes = new Dictionary<string, object> { { "name", "multi-name-update" } }
            };
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(body));
            resp.EnsureSuccessStatusCode();
        }

        private async Task UpdateAttributeValueForFeatures()
        {
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/attribute/4");
            var body = new
            {
                value = "bulk-index-value",
                featureIds = new[] { Guid.Parse(_createdFeature!.AttributeValues["id"].ToString()) }
            };
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(body));
            resp.EnsureSuccessStatusCode();
        }

        private async Task UpdateAttributeValueForFeaturesWhere()
        {
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/attribute-where/4");
            var filter = new List<QueryCondition>
    {
        new QueryCondition("id", QueryOperator.Equal, Guid.Parse(_createdFeature!.AttributeValues["id"].ToString()))
    };
            var body = new { value = "bulk-index-value-where", filter };
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(body));
            resp.EnsureSuccessStatusCode();
        }

        private async Task UpdateAttributeValueByNameForFeatures()
        {
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/attribute-by-name/name");
            var body = new
            {
                value = "bulk-name-value",
                featureIds = new[] { Guid.Parse(_createdFeature!.AttributeValues["id"].ToString()) }
            };
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(body));
            resp.EnsureSuccessStatusCode();
        }

        private async Task UpdateAttributeValueByNameForFeaturesWhere()
        {
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/attribute-by-name-where/name");
            var filter = new List<QueryCondition>
    {
        new QueryCondition("id", QueryOperator.Equal, Guid.Parse(_createdFeature!.AttributeValues["id"].ToString()))
    };
            var body = new { value = "bulk-name-value-where", filter };
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(body));
            resp.EnsureSuccessStatusCode();
        }

        private async Task UpdateAttributeValuesForFeatures()
        {
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/attributes/");
            var body = new
            {
                attributes = new Dictionary<int, object> { { 4, "bulk-attr-map-index" } },
                featureIds = new[] { Guid.Parse(_createdFeature!.AttributeValues["id"].ToString()) }
            };
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(body));
            resp.EnsureSuccessStatusCode();
        }

        private async Task UpdateAttributeValuesByNameForFeatures()
        {
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/attributes-by-name/");
            var body = new
            {
                attributes = new Dictionary<string, object> { { "name", "bulk-attr-map-name" } },
                featureIds = new[] { Guid.Parse(_createdFeature!.AttributeValues["id"].ToString()) }
            };
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(body));
            resp.EnsureSuccessStatusCode();
        }

        private async Task UpdateAttributeValuesForFeaturesWhere()
        {
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/attributes-where/");
            var filter = new List<QueryCondition>
    {
        new QueryCondition("id", QueryOperator.Equal, Guid.Parse(_createdFeature!.AttributeValues["id"].ToString()))
    };
            var body = new
            {
                attributes = new Dictionary<int, object> { { 4, "bulk-attr-map-index-where" } },
                filter
            };
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(body));
            resp.EnsureSuccessStatusCode();
        }

        private async Task UpdateAttributeValuesByNameForFeaturesWhere()
        {
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/attributes-by-name-where/");
            var filter = new List<QueryCondition>
    {
        new QueryCondition("id", QueryOperator.Equal, Guid.Parse(_createdFeature!.AttributeValues["id"].ToString()))
    };
            var body = new
            {
                attributes = new Dictionary<string, object> { { "name", "bulk-attr-map-name-where" } },
                filter
            };
            var resp = await _client.PutAsync(url, GISContentHelper.GetStringContent(body));
            resp.EnsureSuccessStatusCode();
        }

        private async Task RemoveAttribute()
        {
            var url = Url($"{UrlEncoded(ScratchFeatureCollection)}/attribute/{_demoAttributeIndex}");
            var resp = await _client.DeleteAsync(url);
            _output.WriteLine(await resp.Content.ReadAsStringAsync());
            resp.EnsureSuccessStatusCode();
        }

        // --------------------- bulk feature delete -------------------------------
        private async Task RemoveFeatures()
        {
            var url = Url($"{UrlEncoded(FeatureCollectionId)}/feature/");
            var ids = new[] { Guid.Parse(_createdFeature!.AttributeValues["id"].ToString()) };
            var req = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = GISContentHelper.GetStringContent(ids)
            };
            var resp = await _client.SendAsync(req);
            Assert.True(resp.IsSuccessStatusCode);
        }

        // --------------------------
        // Helpers
        // --------------------------
        private FeatureCollection BuildScratchCollection()
        {
            var fc = new FeatureCollection();
            fc.Features.Add(FeatureTypeFixture());
            fc.Features.Add(FeatureTypeFixture());
            fc.Attributes.Add(new DHI.Spatial.Attribute("id", typeof(Guid), 16));
            fc.Attributes.Add(new DHI.Spatial.Attribute("name", typeof(string), 60));
            return fc;
        }

        private Feature FeatureTypeFixture()
        {
            var point = Geometry.FromWKT("POINT (12.5683 55.6761)");
            var feature = new Feature(point);
            feature.AttributeValues.Add("id", Guid.NewGuid());
            feature.AttributeValues.Add("id1", 52);
            feature.AttributeValues.Add("x", 12.5683);
            feature.AttributeValues.Add("y", 55.6761);
            feature.AttributeValues.Add("name", "Dummy Station");
            feature.AttributeValues.Add("id_1", "Dummy");
            return feature;
        }
    }
}
