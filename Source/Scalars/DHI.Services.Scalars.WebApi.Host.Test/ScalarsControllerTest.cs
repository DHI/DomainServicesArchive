namespace DHI.Services.Scalars.WebApi.Host.Test
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using WebApiCore;
    using Xunit;

    [Collection("Controllers collection")]
    public class ScalarsControllerTest
    {
        public ScalarsControllerTest(ControllersFixture fixture)
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
            var response = await client.GetAsync($"api/scalars/{ConnectionId}/MyScalar");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/scalars/{ConnectionId}/NonExisting");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = $"/api/scalars/{ConnectionId}",
                Body = new ScalarDTO
                {
                    FullName = "MyGroup/MySubGroup/NonExisting",
                    ValueTypeName = "System.Double"
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task SetDataForNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = $"/api/scalars/{ConnectionId}/NonExistingScalar/data",
                Body = new ScalarDataDTO
                {
                    Value = "99",
                    DateTime = new DateTime(2019, 08, 27, 12, 30, 0)
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task SetLockedForNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = $"/api/scalars/{ConnectionId}/NonExistingScalar/locked",
                Body = new LockedDTO
                {
                    Locked = true
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
                Url = $"/api/scalars/{ConnectionId}",
                Body = new ScalarDTO
                {
                    FullName = "MyGroup/MySubGroup/Scalar1",
                    ValueTypeName = "System.Double"
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
            var response = await _client.DeleteAsync($"api/scalars/{ConnectionId}/MyGroup|NonExistingScalar");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var request = new
            {
                Url = $"/api/scalars/{ConnectionId}",
                Body = new ScalarDTO
                {
                    FullName = "MyGroup/MySubGroup/Scalar123",
                    ValueTypeName = "System.Double",
                    Value = "99.99",
                    DateTime = new DateTime(2019, 08, 27, 12, 30, 0),
                    Flag = 1
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.GetAsync($"api/scalars/{ConnectionId}/MyGroup|MySubGroup|Scalar123");
            var json = await response.Content.ReadAsStringAsync();
            var scalarDTO = JsonSerializer.Deserialize<ScalarDTO>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(request.Body.FullName, scalarDTO.FullName);
            Assert.Equal("99.99", scalarDTO.Value);
            Assert.Equal(new DateTime(2019, 08, 27, 12, 30, 0), scalarDTO.DateTime);
            Assert.Equal(1, scalarDTO.Flag);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var request = new
            {
                Url = $"/api/scalars/{ConnectionId}",
                Body = new ScalarDTO
                {
                    FullName = "MyGroup/MySubGroup/Scalar1",
                    ValueTypeName = "System.Double",
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.GetAsync($"api/scalars/{ConnectionId}");
            var json = await response.Content.ReadAsStringAsync();
            var scalarDTOs = JsonSerializer.Deserialize<IEnumerable<ScalarDTO>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(scalarDTOs);
        }

        [Fact]
        public async Task GetIdsIsOk()
        {
            var request = new
            {
                Url = $"/api/scalars/{ConnectionId}",
                Body = new ScalarDTO
                {
                    FullName = "MyGroup/MySubGroup/Scalar1",
                    ValueTypeName = "System.Double",
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.GetAsync($"api/scalars/{ConnectionId}/ids");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(request.Body.FullName, ids);
        }

        [Fact]
        public async Task GetFullNamesIsOk()
        {
            var request = new
            {
                Url = $"/api/scalars/{ConnectionId}",
                Body = new ScalarDTO
                {
                    FullName = "MyGroup/MySubGroup/Scalar1",
                    ValueTypeName = "System.Double"
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.GetAsync($"api/scalars/{ConnectionId}/fullnames");
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
                Url = $"/api/scalars/{ConnectionId}",
                Body = new ScalarDTO
                {
                    FullName = "MyGroup/MySubGroup/Scalar1",
                    ValueTypeName = "System.Double"
                }
            };

            await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var response = await _client.GetAsync($"api/scalars/{ConnectionId}/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(count > 0);
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            var request = new
            {
                Url = $"/api/scalars/{ConnectionId}",
                Body = new ScalarDTO
                {
                    FullName = "MyGroup/MySubGroup/Scalar99",
                    ValueTypeName = "System.Double"
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var scalarDTO = JsonSerializer.Deserialize<ScalarDTO>(json);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/scalars/{ConnectionId}/MyGroup|MySubGroup|Scalar99", response.Headers.Location.ToString());
            Assert.Equal(request.Body.FullName, scalarDTO.FullName);

            // Update
            request.Body.Description = "Description";
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            scalarDTO = JsonSerializer.Deserialize<ScalarDTO>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(request.Body.Description, scalarDTO.Description);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{FullNameString.ToUrl(scalarDTO.FullName)}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{FullNameString.ToUrl(scalarDTO.FullName)}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddSetDataSetLockedAndDeleteIsOk()
        {
            var request = new
            {
                Url = $"/api/scalars/{ConnectionId}",
                Body = new ScalarDTO
                {
                    FullName = "MyGroup/MySubGroup/Scalar99",
                    ValueTypeName = "System.Double"
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var scalarDTO = JsonSerializer.Deserialize<ScalarDTO>(json);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/scalars/{ConnectionId}/MyGroup|MySubGroup|Scalar99", response.Headers.Location.ToString());
            Assert.Equal(request.Body.FullName, scalarDTO.FullName);

            // Set Data
            var setDataRequest = new
            {
                Url = $"/api/scalars/{ConnectionId}/{FullNameString.ToUrl(request.Body.FullName)}/data",
                Body = new ScalarDataDTO
                {
                    Value = "99.99",
                    DateTime = DateTime.Now
                }
            };
            response = await _client.PutAsync(setDataRequest.Url, ContentHelper.GetStringContent(setDataRequest.Body));
            json = await response.Content.ReadAsStringAsync();
            scalarDTO = JsonSerializer.Deserialize<ScalarDTO>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("99.99", scalarDTO.Value);

            // Set Locked
            var setLockedRequest = new
            {
                Url = $"/api/scalars/{ConnectionId}/{FullNameString.ToUrl(request.Body.FullName)}/locked",
                Body = new LockedDTO
                {
                    Locked = true
                }
            };
            response = await _client.PutAsync(setLockedRequest.Url, ContentHelper.GetStringContent(setLockedRequest.Body));
            json = await response.Content.ReadAsStringAsync();
            scalarDTO = JsonSerializer.Deserialize<ScalarDTO>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(scalarDTO.Locked);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{FullNameString.ToUrl(scalarDTO.FullName)}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{FullNameString.ToUrl(scalarDTO.FullName)}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}