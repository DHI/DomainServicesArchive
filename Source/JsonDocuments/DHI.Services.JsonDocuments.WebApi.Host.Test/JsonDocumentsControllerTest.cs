namespace DHI.Services.JsonDocuments.WebApi.Host.Test
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using WebApiCore;
    using Xunit;

    [Collection("Controllers collection")]
    public class JsonDocumentsControllerTest
    {
        public JsonDocumentsControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private const string ConnectionId = "fake";

        [Fact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync("api/jsondocuments/MyDocument");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingWithConnectionIdReturns404NotFound()
        {
            var response = await _client.GetAsync("api/jsondocuments/NonExistingConnection/MyDocument");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}/NonExisting");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = "MyGroup/MySubGroup/NonExistingJsonDocument",
                    Data = JsonSerializer.Serialize(new {foo = "bar"})
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task AddExistingReturns400BadRequest()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = $"MyGroup/MySubGroup/{Guid.NewGuid()}",
                    Data = "{ \"string\": \"Hello World\" }"
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already exists", json);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync($"api/jsondocuments/{ConnectionId}/MyGroup|NonExistingJsonDocument");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = $"MyGroup/MySubGroup/{Guid.NewGuid()}",
                    Data = JsonSerializer.Serialize(new { foo = "bar" }),
                    Permissions = new List<PermissionDTO>
                    {
                        new PermissionDTO
                        {
                            Operation = "read",
                            Principals = new List<string> { "Administrators", "Editors"}
                        }
                    }
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}/{FullNameString.ToUrl(request.Body.FullName)}");
            var json = await response.Content.ReadAsStringAsync();
            var jsonDocumentDTO = JsonSerializer.Deserialize<JsonDocumentDTO>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(request.Body.FullName, jsonDocumentDTO.FullName);
            Assert.Equal(request.Body.Data, jsonDocumentDTO.Data);
            Assert.True(jsonDocumentDTO.ToJsonDocument().IsAllowed(new []{"Administrators"}, "read"));
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = $"MyGroup/MySubGroup/{Guid.NewGuid()}",
                    Data = JsonSerializer.Serialize(new { foo = 1, bar = 2, baz = 3 })
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}");
            var json = await response.Content.ReadAsStringAsync();
            var jsonDocumentDTOs = JsonSerializer.Deserialize<IEnumerable<JsonDocumentDTO>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(jsonDocumentDTOs);
        }

        [Fact]
        public async Task GetAllWithDataSelectorsIsOk()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = $"MyGroup/MySubGroup/{Guid.NewGuid()}",
                    Data = JsonSerializer.Serialize(new { foo = 1, bar = 2, baz = 3 })
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}?dataSelectors=[foo, baz]");
            var json = await response.Content.ReadAsStringAsync();
            var jsonDocumentDTOs = JsonSerializer.Deserialize<IEnumerable<JsonDocumentDTO>>(json).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(jsonDocumentDTOs);
            Assert.Contains(jsonDocumentDTOs, dto => dto.Data == JsonSerializer.Serialize(new {foo = 1, baz = 3}));
        }

        [Fact]
        public async Task GetByGroupIsOk()
        {
            var group = $"MyGroup/{Guid.NewGuid()}";
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = $"{group}/{Guid.NewGuid()}",
                    Data = JsonSerializer.Serialize(new { foo = 1, bar = 2, baz = 3 })
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}/group/{FullNameString.ToUrl(group)}");
            var json = await response.Content.ReadAsStringAsync();
            var jsonDocumentDTOs = JsonSerializer.Deserialize<IEnumerable<JsonDocumentDTO>>(json).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single((IEnumerable)jsonDocumentDTOs);
            Assert.Equal(request.Body.FullName, jsonDocumentDTOs[0].FullName);
        }

        [Fact]
        public async Task GetByGroupWithDataSelectorsIsOk()
        {
            var group = $"MyGroup/{Guid.NewGuid()}";
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = $"{group}/{Guid.NewGuid()}",
                    Data = JsonSerializer.Serialize(new { foo = 1, bar = 2, baz = 3 })
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}/group/{FullNameString.ToUrl(group)}?dataSelectors=[foo, baz]");
            var json = await response.Content.ReadAsStringAsync();
            var jsonDocumentDTOs = JsonSerializer.Deserialize<IEnumerable<JsonDocumentDTO>>(json).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var document = (JsonDocumentDTO)Assert.Single((IEnumerable)jsonDocumentDTOs);
            Assert.Equal(request.Body.FullName, document?.FullName);
            Assert.Equal(JsonSerializer.Serialize(new { foo = 1, baz = 3 }), document?.Data);
        }

        [Fact]
        public async Task GetFullNamesIsOk()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = $"MyGroup/MySubGroup/{Guid.NewGuid()}",
                    Data = "{ \"string\": \"Hello World\" }"
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}/fullnames");
            var json = await response.Content.ReadAsStringAsync();
            var fullnames = JsonSerializer.Deserialize<IEnumerable<string>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(request.Body.FullName, fullnames);
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = $"MyGroup/MySubGroup/{Guid.NewGuid()}",
                    Data = "{ \"string\": \"Hello World\" }"
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(count > 0);
        }

        [Fact]
        public async Task GetWithinTimeIntervalIsOk()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = "MyGroup/MySubGroup/MyJsonDocument",
                    Data = JsonSerializer.Serialize(new { foo = 1 }),
                    DateTime = new DateTime(2019, 07, 20)
                }
            };
            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = "MyGroup/MySubGroup/MyJsonDocument2",
                    Data = JsonSerializer.Serialize(new { foo = 1 }),
                    DateTime = new DateTime(2019, 07, 21)
                }
            };
            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = "MyGroup/MySubGroup/MyJsonDocument3",
                    Data = JsonSerializer.Serialize(new { foo = 1 }),
                    DateTime = new DateTime(2019, 07, 22)
                }
            };
            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}?from=2019-07-20T13:00:00&to=2019-07-23");
            var json = await response.Content.ReadAsStringAsync();
            var documents = JsonSerializer.Deserialize<JsonDocumentDTO[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, documents.Length);
            Assert.DoesNotContain(documents, d => d.FullName == "MyGroup/MySubGroup/MyJsonDocument1");
        }

        [Fact]
        public async Task GetWithDataSelectorsIsOk()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = $"{Guid.NewGuid()}",
                    Data = JsonSerializer.Serialize(new { foo = 1, bar = 2, baz = 3 }),
                    DateTime = new DateTime(2019, 07, 20)
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            var response = await _client.GetAsync($"api/jsondocuments/{ConnectionId}/{request.Body.FullName}?dataSelectors=[foo]");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var actual = JsonSerializer.Deserialize<JsonDocumentDTO>(json);
            Assert.Equal(JsonSerializer.Serialize(new { foo = 1 }), actual.Data);
        }

        [Fact]
        public async Task GetByQueryIsOk()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = "foo",
                    Data = JsonSerializer.Serialize(new { data = 1 })
                }
            };
            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = "bar",
                    Data = JsonSerializer.Serialize(new { data = 2 })
                }
            };
            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = "baz",
                    Data = JsonSerializer.Serialize(new { data = 2 })
                }
            };
            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));

            var queryRequest = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}/query",
                Body = new object[]
                {
                    new {Item = "Data", QueryOperator = "Equal", Value = JsonSerializer.Serialize(new { data = 2 })},
                    new {Item = "Id", QueryOperator = "NotEqual", Value = "baz"}
                }
            };

            var response = await _client.PostAsync(queryRequest.Url, ContentHelper.GetStringContent(queryRequest.Body));
            var json = await response.Content.ReadAsStringAsync();
            var documents = JsonSerializer.Deserialize<JsonDocumentDTO[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(documents);
            Assert.Equal("bar", documents[0].FullName);
        }

        [Fact]
        public async Task GetByQueryNotMetReturnsEmpty()
        {
            var queryRequest = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}/query",
                Body = new object[]
                {
                    new {Item = "Data", QueryOperator = "Equal", Value = JsonSerializer.Serialize(new { foo = 99 })}
                }
            };

            var response = await _client.PostAsync(queryRequest.Url, ContentHelper.GetStringContent(queryRequest.Body));
            var json = await response.Content.ReadAsStringAsync();
            var documents = JsonSerializer.Deserialize<JsonDocumentDTO[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(documents);
        }

        [Fact]
        public async Task GetByQueryWithDataSelectorsIsOk()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = $"{Guid.NewGuid()}",
                    Data = JsonSerializer.Serialize(new { foo = 1, bar = 2, baz = 3 }),
                    DateTime = new DateTime(2019, 08, 20)
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var queryRequest = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}/query?dataSelectors=[foo,bar]",
                Body = new object[]
                {
                    new {Item = "Id", QueryOperator = "Equal", Value = $"{request.Body.FullName}"}
                }
            };

            var response = await _client.PostAsync(queryRequest.Url, ContentHelper.GetStringContent(queryRequest.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var actual = Assert.Single(JsonSerializer.Deserialize<JsonDocumentDTO[]>(json));
            Assert.Equal(JsonSerializer.Serialize(new { foo = 1, bar = 2 }), actual.Data);
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = $"MyGroup/{Guid.NewGuid()}",
                    Data = "{ \"string\": \"Hello World\" }"
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var jsonDocumentDTO = JsonSerializer.Deserialize<JsonDocumentDTO>(json);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/jsondocuments/{ConnectionId}/{FullNameString.ToUrl(jsonDocumentDTO.FullName)}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.FullName, jsonDocumentDTO.FullName);
            Assert.NotNull(jsonDocumentDTO.Added);

            // Update
            request.Body.Data = "{ \"string\": \"Howdy World\" }";
            request.Body.Updated = new DateTime(2021, 3, 10, 10, 49, 0);
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            jsonDocumentDTO = JsonSerializer.Deserialize<JsonDocumentDTO>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(request.Body.Data, jsonDocumentDTO.Data);
            Assert.NotNull(jsonDocumentDTO.Updated);
            Assert.Equal(request.Body.Updated, jsonDocumentDTO.Updated);
            Assert.NotNull(jsonDocumentDTO.Added);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{FullNameString.ToUrl(jsonDocumentDTO.FullName)}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{FullNameString.ToUrl(jsonDocumentDTO.FullName)}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddAndSoftDeleteIsOk()
        {
            var request = new
            {
                Url = $"/api/jsondocuments/{ConnectionId}",
                Body = new JsonDocumentDTO
                {
                    FullName = $"MyGroup/{Guid.NewGuid()}",
                    Data = "{ \"string\": \"Hello World\" }"
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var jsonDocumentDTO = JsonSerializer.Deserialize<JsonDocumentDTO>(json);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/jsondocuments/{ConnectionId}/{FullNameString.ToUrl(jsonDocumentDTO.FullName)}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.FullName, jsonDocumentDTO.FullName);
            Assert.NotNull(jsonDocumentDTO.Added);

            // Soft Delete
            response = await _client.DeleteAsync($"{request.Url}/{FullNameString.ToUrl(jsonDocumentDTO.FullName)}?softDelete=true");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync($"{request.Url}/{FullNameString.ToUrl(jsonDocumentDTO.FullName)}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            json = await response.Content.ReadAsStringAsync();
            jsonDocumentDTO = JsonSerializer.Deserialize<JsonDocumentDTO>(json);
            Assert.NotNull(jsonDocumentDTO.Deleted);
        }
    }
}