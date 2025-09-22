namespace DHI.Services.Notifications.WebApi.Host.Test
{
    using System.Net;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Xunit.Abstractions;

    [Collection("Controllers collection")]
    public class NotificationsControllerTest
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;

        public NotificationsControllerTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.Client;
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _options.Converters.Add(new JsonStringEnumConverter());
            _output = output;
        }

        [Fact]
        public async Task AddNotificationEntryIsCreated()
        {
            var request = new
            {
                Url = "api/notifications/json-notifications",
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

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = JsonSerializer.Deserialize<NotificationEntry>(json, _options);
            Assert.Equal(request.Body.Text, created.Text);
        }

        [Fact]
        public async Task GetByQueryStringReturnsEntry()
        {
            var url = "api/notifications/json-notifications?Tag=tag";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var entries = JsonSerializer.Deserialize<IEnumerable<NotificationEntry>>(json, _options);
            Assert.NotEmpty(entries);
            Assert.Contains(entries, e => e.Tag == "tag");
        }

        [Fact]
        public async Task LastNotificationReturnsEntry()
        {
            var notification = await CreateTestNotification("test");

            var body = new List<object>
            {
                new { Item = "Tag", QueryOperator = "Equal", Value = "test" }
            };
            var response = await _client.PostAsync("api/notifications/json-notifications/last", ContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var entry = JsonSerializer.Deserialize<NotificationEntry>(json, _options);
            Assert.Equal("test", entry.Tag);
        }

        [Fact]
        public async Task PostQueryReturnsEntries()
        {
            var notification = await CreateTestNotification("test");

            var body = new List<object>
            {
                new { Item = "Tag", QueryOperator = "Equal", Value = "test" }
            };

            var response = await _client.PostAsync("api/notifications/json-notifications/query", ContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();
            var entries = JsonSerializer.Deserialize<IEnumerable<NotificationEntry>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(entries);
        }

        [Fact]
        public async Task LastWithNoMatchReturns404()
        {
            var body = new List<object>
        {
            new { Item = "Tag", QueryOperator = "Equal", Value = "non-existent-tag" }
        };
            var response = await _client.PostAsync("api/notifications/json-notifications/last", ContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("No notification entry found", json);
        }

        [Fact]
        public async Task AddNotificationEntryWithMissingFieldsReturnsBadRequest()
        {
            var invalidBody = new NotificationEntryDTO
            {
                NotificationLevel = NotificationLevel.Warning,
                Source = "UnitTest",
                Tag = "missing-text",
                MachineName = "localhost",
                Metadata = new Dictionary<string, object> { { "test", "value" } }
            };

            var response = await _client.PostAsync("api/notifications/json-notifications", ContentHelper.GetStringContent(invalidBody));
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Text", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddNotificationEntryWithEmptyMetadataIsAccepted()
        {
            var notification = new NotificationEntryDTO
            {
                NotificationLevel = NotificationLevel.Information,
                Text = "No metadata",
                Source = "UnitTest",
                Tag = Guid.NewGuid().ToString(),
                MachineName = "localhost",
                Metadata = new Dictionary<string, object>()
            };

            var response = await _client.PostAsync("api/notifications/json-notifications", ContentHelper.GetStringContent(notification));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task QueryWithInvalidFieldReturnsError()
        {
            var body = new List<object>
            {
                new { Item = "NonExistentField", QueryOperator = "Equal", Value = "something" }
            };

            var response = await _client.PostAsync("api/notifications/json-notifications/query", ContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("NonExistentField", json);
        }

        [Fact]
        public async Task AddNotificationWithLongTextIsHandled()
        {
            var longText = new string('A', 5000);
            var notification = new NotificationEntryDTO
            {
                NotificationLevel = NotificationLevel.Debug,
                Text = longText,
                Source = "特殊字符",
                Tag = "long-text",
                MachineName = "localhost",
                Metadata = new Dictionary<string, object>()
            };

            var response = await _client.PostAsync("api/notifications/json-notifications", ContentHelper.GetStringContent(notification));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Contains("特", json);
        }

        [Fact]
        public async Task QueryIsCaseInsensitive()
        {
            var tag = "CaseTest-" + Guid.NewGuid();
            await CreateTestNotification(tag.ToLower());

            var body = new List<object>
            {
                new { Item = "Tag", QueryOperator = "Equal", Value = tag.ToUpper() }
            };

            var response = await _client.PostAsync("api/notifications/json-notifications/query", ContentHelper.GetStringContent(body));
            var json = await response.Content.ReadAsStringAsync();
            var entries = JsonSerializer.Deserialize<IEnumerable<NotificationEntry>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _output.WriteLine(json);
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

            var response = await _client.PostAsync("api/notifications/json-notifications", ContentHelper.GetStringContent(notification));
            response.EnsureSuccessStatusCode();

            return notification;
        }

    }
}
