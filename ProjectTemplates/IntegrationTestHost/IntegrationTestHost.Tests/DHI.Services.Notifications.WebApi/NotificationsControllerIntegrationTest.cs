namespace IntegrationTestHost.Tests
{
    using DHI.Services.Notifications;
    using DHI.Services.Notifications.WebApi;
    using System.Net;
    using System.Text.Json;
    using Xunit.Abstractions;

    public class NotificationsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;

        private const string ConnectionId = "json-notifications";

        public NotificationsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full NotificationsController integration suite")]
        public async Task Run_NotificationsController_IntegrationFlow()
        {
            await Step("AddNotificationEntry", AddNotificationEntry);
            await Step("GetByQueryString", GetByQueryString);
            await Step("Last", Last);
            await Step("GetByQuery", GetByQuery);
        }

        private async Task Step(string name, Func<Task> func)
        {
            _output.WriteLine($">>> Running step: {name}");
            try
            {
                await func();
                _output.WriteLine($"✔ Step '{name}' passed.");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✖ Step '{name}' failed: {ex.Message}");
                throw;
            }
        }

        private async Task AddNotificationEntry()
        {
            var request = new
            {
                Url = $"api/notifications/{ConnectionId}",
                Body = new NotificationEntryDTO
                {
                    NotificationLevel = NotificationLevel.Warning,
                    Text = "Unit test notification",
                    Source = "UnitTest",
                    Tag = "test",
                    MachineName = "localhost",
                    Metadata = new Dictionary<string, object>
                    {
                        { "unit", "test" },
                        { "status", "pass" }
                    }
                }
            };

            var response = await _client.PostAsync(request.Url, NotificationsContentHelper.GetStringContent(request.Body));
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task GetByQueryString()
        {
            var url = $"api/notifications/{ConnectionId}?Tag=tag";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task Last()
        {
            var notification = await CreateTestNotification("test");

            var body = new List<object>
            {
                new { Item = "Tag", QueryOperator = "Equal", Value = "test" }
            };
            var response = await _client.PostAsync($"api/notifications/{ConnectionId}/last", NotificationsContentHelper.GetStringContent(body));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetByQuery()
        {
            var notification = await CreateTestNotification("test");

            var body = new List<object>
            {
                new { Item = "Tag", QueryOperator = "Equal", Value = "test" }
            };

            var response = await _client.PostAsync("api/notifications/json-notifications/query", NotificationsContentHelper.GetStringContent(body));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task<NotificationEntryDTO> CreateTestNotification(string tag = "test")
        {
            var notification = new NotificationEntryDTO
            {
                NotificationLevel = NotificationLevel.Warning,
                Text = "Unit test notification",
                Source = "UnitTest",
                Tag = tag,
                MachineName = "localhost",
                Metadata = new Dictionary<string, object> { { "unit", "test" } }
            };

            var response = await _client.PostAsync($"api/notifications/{ConnectionId}", NotificationsContentHelper.GetStringContent(notification));
            response.EnsureSuccessStatusCode();

            return notification;
        }
    }
}
