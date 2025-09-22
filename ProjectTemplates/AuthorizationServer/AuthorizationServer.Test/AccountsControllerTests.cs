namespace AuthorizationServer.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DHI.Services.Security.WebApi.DTOs;
    using Xunit.Abstractions;

    public class AccountsControllerTests : IClassFixture<JsonWebAppFactory>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;

        public AccountsControllerTests(ITestOutputHelper output, JsonWebAppFactory factory)
        {
            _output = output;
            _client = factory.CreateClient();
            _options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        [Fact]
        public async Task AccountLifecycle_ShouldCreateGetUpdateAndDelete()
        {
            const string id = "test_user_json";
            var requestUrl = "/api/accounts";

            // Create
            var create = new AccountDTO(id, "Test User")
            {
                Email = "test.user@json.com",
                Password = "QwertY1234!@",
                UserGroups = new[] { "Users" }
            };

            var post = await _client.PostAsync(requestUrl, ContentHelper.GetStringContent(create));
            string details = await post.Content.ReadAsStringAsync();
            _output.WriteLine(details);
            Assert.Equal(HttpStatusCode.Created, post.StatusCode);

            // Get All
            var getAll = await _client.GetAsync(requestUrl);
            var json = await getAll.Content.ReadAsStringAsync();
            var accounts = JsonSerializer.Deserialize<List<AccountDTO>>(json, _options);
            Assert.Contains(accounts, a => a.Id == id);

            // Get by ID
            var get = await _client.GetAsync($"{requestUrl}/{id}");
            var getJson = await get.Content.ReadAsStringAsync();
            var account = JsonSerializer.Deserialize<AccountDTO>(getJson, _options);
            Assert.Equal(id, account.Id);
            Assert.Equal("Test User", account.Name);

            // Update
            var update = new AccountUpdateDTO(id, "Test User")
            {
                Company = "Updated Co.",
                Email = "test.user@json.com",
                Password = "QwertY1234!@",
                UserGroups = new[] { "Editors" }
            };
            var put = await _client.PutAsync(requestUrl, ContentHelper.GetStringContent(update));
            var updated = JsonSerializer.Deserialize<AccountDTO>(await put.Content.ReadAsStringAsync(), _options);
            Assert.Equal("Updated Co.", updated.Company);

            // Delete
            var delete = await _client.DeleteAsync($"{requestUrl}/{id}");
            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

            var afterDelete = await _client.GetAsync($"{requestUrl}/{id}");
            Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
        }
    }
}
