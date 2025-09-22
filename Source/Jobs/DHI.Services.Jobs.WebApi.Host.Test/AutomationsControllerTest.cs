namespace DHI.Services.Jobs.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Text.Json;
    using Automations;
    using WebApiCore;
    using Xunit;
    using System;
    using DHI.Services.Jobs.WebApi.DTOs;
    using System.IO;
    using System.Reflection;

    [Collection("Controllers collection")]
    public class AutomationsControllerTest
    {
        private readonly ControllersFixture _fixture;

        public AutomationsControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
            _serializerOptions = SerializerOptionsDefault.Options;
        }

        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _serializerOptions;

        [Fact]
        public async Task GetIllegalFullNameReturns400BadRequest()
        {
            var response = await _client.GetAsync("api/automations/IllegalAutomationName");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The ID of a grouped entity must be a string with following format", json);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync("api/automations/MyGroup1|NonExistingAutomationName");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = "/api/automations",
                Body = new Dictionary<string, string>
                {
                    { "$type", "DHI.Services.Jobs.Automations.Automation, DHI.Services.Jobs" },
                    { "taskId", "my-task" },
                    { "group", "my-group-1" },
                    { "name", "NonExisting" }
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
                Url = "/api/automations",
                Body = new Dictionary<string, string>
                {
                    { "$type", "DHI.Services.Jobs.Automations.Automation, DHI.Services.Jobs" },
                    { "taskId", "my-task" },
                    { "group", "my-group-1" },
                    { "name", "my-automation" }
                }
            };

            var response = await _client.PostAsync(request.Url, JsonContent.Create(request.Body, options: _serializerOptions));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already exists", json);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync("api/automations/my-group-1|non-existing-automation");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync("api/automations/my-group-1|my-automation");
            var json = await response.Content.ReadAsStringAsync();
            var automation = JsonSerializer.Deserialize<JsonElement>(json, _serializerOptions);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("my-group-1/my-automation", automation.GetProperty("id").GetString());
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync("api/automations");
            var json = await response.Content.ReadAsStringAsync();
            var automations = JsonSerializer.Deserialize<IEnumerable<JsonElement>>(json, _serializerOptions).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(automations.Any());
            Assert.Contains(automations, a => a.GetProperty("id").GetString() == "my-group-1/my-automation");
            Assert.Contains(automations, a => a.GetProperty("id").GetString() == "my-group-2/my-automation");
        }

        [Fact]
        public async Task GetByGroupIsOk()
        {
            var response = await _client.GetAsync("api/automations?group=my-group-1");
            var json = await response.Content.ReadAsStringAsync();
            var automations = JsonSerializer.Deserialize<IEnumerable<JsonElement>>(json, _serializerOptions).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(automations);
            Assert.Contains(automations, a => a.GetProperty("id").GetString() == "my-group-1/my-automation");
            Assert.DoesNotContain(automations, a => a.GetProperty("id").GetString() == "my-group-2/my-automation");
        }

        [Fact]
        public async Task GetIdsIsOk()
        {
            var response = await _client.GetAsync("api/automations/ids");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json, _serializerOptions).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, ids.Length);
            Assert.Contains("my-group-1/my-automation", ids);
        }

        [Fact]
        public async Task GetFullNamesIsOk()
        {
            var response = await _client.GetAsync("api/automations/fullnames");
            var json = await response.Content.ReadAsStringAsync();
            var fullNames = JsonSerializer.Deserialize<IEnumerable<string>>(json, _serializerOptions).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, fullNames.Length);
            Assert.Contains("my-group-1/my-automation", fullNames);
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync("api/automations/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json, _serializerOptions);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            var request = new
            {
                Url = "/api/automations",
                Body = new Dictionary<string, object>
                {
                    { "$type", "DHI.Services.Jobs.Automations.Automation, DHI.Services.Jobs" },
                    { "taskId", "my-task" },
                    { "group", "my-group-3" },
                    { "name", "my-automation" },
                    { "priority", 10 }
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var automation = JsonSerializer.Deserialize<Automation>(json, _serializerOptions);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("http://localhost/api/automations/my-group-3|my-automation", response.Headers.Location.ToString());
            Assert.Equal("my-group-3/my-automation", automation.Id);

            // Update
            request.Body["priority"] = -5;
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            automation = JsonSerializer.Deserialize<Automation>(json, _serializerOptions);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(-5, automation.Priority);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{FullNameString.ToUrl(automation.FullName)}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{FullNameString.ToUrl(automation.FullName)}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddAutomationWithTriggerConditionIsOk()
        {
            var request = new
            {
                Url = "/api/automations",
                Body = new Dictionary<string, object>
        {
            { "$type", "DHI.Services.Jobs.Automations.Automation, DHI.Services.Jobs" },
            { "name", "test" },
            { "group", "test" },
            { "taskId", "test" },
            { "hostGroup", "test" },
            { "priority", 0 },
            { "tag", "test" },
            { "isEnabled", true },
            { "fullName", "test/test" },
            { "id", "test/test" },
            { "triggerCondition", new Dictionary<string, object>
                {
                    { "triggers", new List<Dictionary<string, object>>
                        {
                            new Dictionary<string, object>
                            {
                                { "$type", "DHI.Services.Jobs.Automations.Triggers.ScheduledTrigger, DHI.Services.Jobs" },
                                { "id", "test" },
                                { "description", "test" },
                                { "isEnabled", true },
                                { "type", "DHI.Services.Jobs.Automations.Triggers.ScheduledTrigger, DHI.Services.Jobs" },
                                { "startTimeUtc", "2025-05-28T09:06:10.462Z" },
                                { "interval", "1.00:00:00" }
                            }
                        }
                    },
                    { "conditional", "test" },
                    { "isEnable", true }
                }
            },
            { "taskParameters", new Dictionary<string, object>
                {
                    { "test", "test" }
                }
            },
            { "parameters", new Dictionary<string, object>
                {
                    { "utcNow", "2025-05-28T09:06:19.214Z" },
                    { "triggerNow", false }
                }
            }
        }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var automation = JsonSerializer.Deserialize<Automation>(json, _serializerOptions);
            Assert.Equal("test/test", automation.Id);

            // Cleanup
            var deleteResponse = await _client.DeleteAsync($"{request.Url}/{FullNameString.ToUrl(automation.FullName)}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task DisableIsOk()
        {
            var request = new
            {
                Url = "/api/automations/my-group-1%2Fmy-automation/enable",
                Body = new Dictionary<string, object>
                {
                    { "flag", false }
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var automation = JsonSerializer.Deserialize<JsonElement>(json, _serializerOptions);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(automation.GetProperty("isEnabled").GetBoolean());
        }

        [Fact]
        public async Task EnableIsOk()
        {
            var request = new
            {
                Url = "/api/automations/my-group-1%2Fmy-automation/enable",
                Body = new Dictionary<string, object>
                {
                    { "flag", true }
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var automation = JsonSerializer.Deserialize<JsonElement>(json, _serializerOptions);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(automation.GetProperty("isEnabled").GetBoolean());
        }

        [Fact]
        public async Task GetVersionIsOk()
        {
            var response = await _client.GetAsync("api/automations/version");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var isoString = JsonSerializer.Deserialize<string>(json);
            var parsed = DateTime.Parse(isoString, null, System.Globalization.DateTimeStyles.RoundtripKind);

            Assert.Equal(DateTimeKind.Utc, parsed.Kind);
            Assert.True(parsed > DateTime.UtcNow.AddMinutes(-5));
        }

        [Fact]
        public async Task GetVersionCreatesFileIfMissing()
        {
            var versionFilePath = Path.Combine(_fixture.TempAppDataPath, "version.txt");

            if (File.Exists(versionFilePath))
                File.Delete(versionFilePath);

            Assert.False(File.Exists(versionFilePath), "Expected version file to be deleted before test");

            var response = await _client.GetAsync("api/automations/version");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(File.Exists(versionFilePath), "Expected version file to be created");

            var text = File.ReadAllText(versionFilePath);
            var parsed = DateTime.Parse(text, null, System.Globalization.DateTimeStyles.RoundtripKind);
            Assert.Equal(DateTimeKind.Utc, parsed.Kind);
        }
    }
}