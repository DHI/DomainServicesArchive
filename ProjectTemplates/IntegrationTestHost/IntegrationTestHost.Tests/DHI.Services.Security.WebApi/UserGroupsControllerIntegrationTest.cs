namespace IntegrationTestHost.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http.Json;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DHI.Services.Security.WebApi.DTOs;
    using Xunit.Abstractions;

    public class UserGroupsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private readonly ControllersFixture _fixture;

        private const string TestUserGroupId = "TestUserGroup";
        private const string TestUserId = "hardcoded_user_group";
        private const string TestUserPassword = "S3cur3P@sswordGroup!";

        public UserGroupsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full UserGroupsController integration suite")]
        public async Task Run_UserGroupsController_IntegrationFlow()
        {
            await Step("EnsureHardcodedUserExists", EnsureHardcodedUserExists);
            await Step("EnsureTestUserGroupDoesNotExist", EnsureTestUserGroupDoesNotExist);
            await Step("AddTestUserGroup", AddTestUserGroup);
            await Step("GetUserGroupById", GetUserGroupById);
            await Step("UpdateUserGroup", UpdateUserGroup);
            await Step("GetAllUserGroups", GetAllUserGroups);
            await Step("GetUserGroupCount", GetUserGroupCount);
            await Step("GetUserGroupIds", GetUserGroupIds);
            await Step("GetUserGroupIdsByUser", GetUserGroupIdsByUser);
            await Step("AddUserToGroup", AddUserToGroup);
            await Step("DeleteUserFromGroup", DeleteUserFromGroup);
            await Step("DeleteUserFromAllGroups", DeleteUserFromAllGroups);
            await Step("DeleteUserGroup", DeleteUserGroup);
        }

        private async Task Step(string name, Func<Task> action)
        {
            _output.WriteLine($">>> Running step: {name}");
            try
            {
                await action();
                _output.WriteLine($"✔ Step '{name}' passed.");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✖ Step '{name}' failed: {ex.Message}");
                throw;
            }
        }

        private async Task EnsureHardcodedUserExists()
        {
            var getResponse = await _client.GetAsync($"api/accounts/{TestUserId}");
            if (getResponse.StatusCode == HttpStatusCode.OK)
            {
                var deleteResponse = await _client.DeleteAsync($"api/accounts/{TestUserId}");
                if (deleteResponse.StatusCode != HttpStatusCode.NoContent)
                {
                    var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to delete existing hardcoded user. Status: {deleteResponse.StatusCode}, Content: {deleteContent}");
                }
            }

            var body = new AccountDTO(TestUserId, "Hardcoded User")
            {
                Password = TestUserPassword,
                Email = "hardcoded@dhi.local",
                Company = "DHI",
                UserGroups = new List<string> { "Editors" }
            };

            var createResponse = await _client.PostAsJsonAsync("api/accounts", body);
            var createContent = await createResponse.Content.ReadAsStringAsync();

            if (createResponse.StatusCode != HttpStatusCode.Created)
                throw new Exception($"Failed to create hardcoded user. Status: {createResponse.StatusCode}, Content: {createContent}");
        }

        private async Task EnsureTestUserGroupDoesNotExist()
        {
            var response = await _client.GetAsync($"api/usergroups/{TestUserGroupId}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var delete = await _client.DeleteAsync($"api/usergroups/{TestUserGroupId}");
                Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
            }
        }

        private async Task AddTestUserGroup()
        {
            var group = new UserGroupDTO
            {
                Id = TestUserGroupId,
                Name = "Integration Test Group",
                Users = new HashSet<string>()
            };

            var response = await _client.PostAsJsonAsync("api/usergroups", group);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task GetUserGroupById()
        {
            var response = await _client.GetAsync($"api/usergroups/{TestUserGroupId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task UpdateUserGroup()
        {
            var updated = new UserGroupDTO
            {
                Id = TestUserGroupId,
                Name = "Updated Integration Test Group",
                Metadata = new Dictionary<string, object> { { "Updated", true } },
                Users = new HashSet<string>()
            };

            var response = await _client.PutAsJsonAsync("api/usergroups", updated);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetAllUserGroups()
        {
            var response = await _client.GetAsync("api/usergroups");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetUserGroupCount()
        {
            var response = await _client.GetAsync("api/usergroups/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetUserGroupIds()
        {
            var response = await _client.GetAsync("api/usergroups/ids");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetUserGroupIdsByUser()
        {
            var response = await _client.GetAsync($"api/usergroups/ids?userId={TestUserId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task AddUserToGroup()
        {
            var body = new[] { TestUserGroupId };
            var response = await _client.PostAsJsonAsync($"api/usergroups/user/{TestUserId}", body);
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task DeleteUserFromGroup()
        {
            var response = await _client.DeleteAsync($"api/usergroups/user/{TestUserId}?groupId={TestUserGroupId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task DeleteUserFromAllGroups()
        {
            var response = await _client.DeleteAsync($"api/usergroups/user/{TestUserId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task DeleteUserGroup()
        {
            var response = await _client.DeleteAsync($"api/usergroups/{TestUserGroupId}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var confirm = await _client.GetAsync($"api/usergroups/{TestUserGroupId}");
            Assert.Equal(HttpStatusCode.NotFound, confirm.StatusCode);
        }
    }
}
