namespace DHI.Services.Security.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DTOs;
    using Xunit;

    [Collection("Controllers collection")]
    public class UserGroupsControllerTest
    {
        public UserGroupsControllerTest(ControllersFixture fixture)
        {
            _client = fixture.Client;
            _options = fixture.SerializerOptions;
        }

        private readonly HttpClient _client;
        private readonly System.Text.Json.JsonSerializerOptions _options;

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync("api/usergroups/NonExistingUserGroup");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = "/api/usergroups",
                Body = new UserGroupDTO
                {
                    Id = "NonExistingUserGroup",
                    Name = "NonExistingUserGroup"
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
                Url = "/api/usergroups",
                Body = new UserGroupDTO
                {
                    Id = "Administrators",
                    Name = "System Administrators"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already exists", json);
        }

        [Fact]
        public async Task AddWithMissingNameReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/usergroups",
                Body = new UserGroupDTO
                {
                    Id = "Administrators",
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The Name field is required", json);
        }

        [Fact]
        public async Task AddNonExistingUserReturns404NotFound()
        {
            var request = new
            {
                Url = "/api/usergroups/user/bill.gates",
                Body = new[] { "Administrators" }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddUserToNonExistingGroupReturns404NotFound()
        {
            var request = new
            {
                Url = "/api/usergroups/user/admin",
                Body = new[] { "NonExistingGroup" }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync("api/usergroups/NonExistingUserGroup");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync("api/usergroups/Administrators");
            var json = await response.Content.ReadAsStringAsync();
            var userGroup = JsonSerializer.Deserialize<UserGroupDTO>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Administrators", userGroup.Id);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync("api/usergroups");
            var json = await response.Content.ReadAsStringAsync();
            var userGroups = JsonSerializer.Deserialize<IEnumerable<UserGroupDTO>>(json, _options).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(4, userGroups.Length);
            Assert.Contains(userGroups, userGroup => userGroup.Id == "Editors");
        }

        [Fact]
        public async Task GetIdsIsOk()
        {
            var response = await _client.GetAsync("api/usergroups/ids");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(4, ids.Length);
            Assert.Contains("Guests", ids);
        }

        [Fact]
        public async Task GetIdsByUserIsOk()
        {
            var response = await _client.GetAsync("api/usergroups/ids?userId=editor");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, ids.Length);
            Assert.DoesNotContain("Administrators", ids);
            Assert.Contains("Guests", ids);
            Assert.Contains("Users", ids);
            Assert.Contains("Editors", ids);
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync("api/usergroups/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(4, count);
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            var request = new
            {
                Url = "/api/usergroups",
                Body = new UserGroupDTO
                {
                    Id = "MyNewUserGroup",
                    Name = "My new user group",
                    Users = new HashSet<string> { "john.doe" }
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var userGroup = JsonSerializer.Deserialize<UserGroupDTO>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("http://localhost/api/usergroups/MyNewUserGroup", response.Headers.Location.ToString());
            Assert.Equal(request.Body.Id, userGroup.Id);

            // Update
            request.Body.Metadata = new Dictionary<string, object> { { "Description", "For demo purpose only" } };
            request.Body.Users.Add("donald.duck");
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            userGroup = JsonSerializer.Deserialize<UserGroupDTO>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(userGroup.Metadata.Count == 1);
            Assert.Contains("donald.duck", userGroup.Users);
            Assert.Contains("john.doe", userGroup.Users);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{userGroup.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{userGroup.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddUserIsOk()
        {
            const string userId = "guest";
            var request = new
            {
                Url = $"/api/usergroups/user/{userId}",
                Body = new[] { "Administrators", "Editors" }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync("api/usergroups/Administrators");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var administrators = JsonSerializer.Deserialize<UserGroupDTO>(json, _options);
            Assert.Contains(userId, administrators.Users);

            response = await _client.GetAsync("api/usergroups/Editors");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            json = await response.Content.ReadAsStringAsync();
            var editors = JsonSerializer.Deserialize<UserGroupDTO>(json, _options);
            Assert.Contains(userId, editors.Users);

            response = await _client.GetAsync("api/usergroups/Users");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<UserGroupDTO>(json, _options);
            Assert.DoesNotContain(userId, users.Users);
        }

        [Fact]
        public async Task DeleteUserFromGroupIsOk()
        {
            const string userId = "admin";
            var response = await _client.DeleteAsync($"api/usergroups/user/{userId}?groupId=Editors");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync("api/usergroups/Editors");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var editors = JsonSerializer.Deserialize<UserGroupDTO>(json, _options);
            Assert.DoesNotContain(userId, editors.Users);

            response = await _client.GetAsync("api/usergroups/Administrators");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            json = await response.Content.ReadAsStringAsync();
            var administrators = JsonSerializer.Deserialize<UserGroupDTO>(json, _options);
            Assert.Contains(userId, administrators.Users);

            response = await _client.GetAsync($"api/usergroups/ids?userId={userId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json).ToArray();
            Assert.DoesNotContain("Editors", ids);
        }

        [Fact]
        public async Task DeleteUserFromAllGroupsIsOk()
        {
            const string userId = "editor";
            var response = await _client.DeleteAsync($"api/usergroups/user/{userId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync($"api/usergroups/ids?userId={userId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json).ToArray();
            Assert.Empty(ids);
        }
    }
}