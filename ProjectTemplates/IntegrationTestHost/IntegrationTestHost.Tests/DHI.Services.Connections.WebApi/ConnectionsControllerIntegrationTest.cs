namespace IntegrationTestHost.Tests
{
    using DHI.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit.Abstractions;

    public class ConnectionsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private const string ConnectionId = "connections-csv";
        private const string TypeId = "TimeSeriesServiceConnection";

        public ConnectionsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full ConnectionsController integration suite")]
        public async Task Run_ConnectionsController_IntegrationFlow()
        {
            await Step("GetAll", async () => await AssertSuccess("api/connections"));
            await Step("GetById", async () => await AssertSuccess($"api/connections/{ConnectionId}"));
            await Step("GetIds", async () => await AssertSuccess("api/connections/ids"));
            await Step("GetAllTypes", async () => await AssertSuccess("api/connections/types"));
            await Step("GetType", async () => await AssertSuccess($"api/connections/types/{TypeId}"));
            await Step("GetTypeIds", async () => await AssertSuccess("api/connections/types/ids"));
            await Step("GetCount", async () => await AssertSuccess("api/connections/count"));
            await Step("VerifyExisting", async () => await AssertSuccess($"api/connections/{ConnectionId}/verification"));
            await Step("VerifyNew", async () => await PostAndAssert("api/connections/verification", CreateConnectionBody("csv-verify")));
            await Step("AddUpdateDelete", async () => await AddUpdateDeleteRoundtrip());
        }

        private async Task Step(string name, Func<Task> func)
        {
            _output.WriteLine($">>> Running step: {name}");
            try
            {
                await func();
                _output.WriteLine($"✔ Step '{name}' passed.");
            }
            catch (HttpRequestException ex)
            {
                _output.WriteLine($"✖ Step '{name}' failed with HTTP error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"✖ Step '{name}' failed: {ex.Message}");
                throw;
            }
        }

        private async Task AssertSuccess(string url)
        {
            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _output.WriteLine(content);
                throw new HttpRequestException($"GET {url} failed with {(int)response.StatusCode}: {content}");
            }
        }

        private async Task PostAndAssert(string url, object body)
        {
            var response = await _client.PostAsJsonAsync(url, body, _options);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"POST {url} failed with {(int)response.StatusCode}: {content}");
            }
        }

        private Dictionary<string, string> CreateConnectionBody(string id)
        {
            return new Dictionary<string, string>
            {
                {"$type", "DHI.Services.TimeSeries.WebApi.DiscreteTimeSeriesServiceConnection, DHI.Services.TimeSeries.WebApi"},
                {"ConnectionString", "[AppData]"},
                {"RepositoryType", "DHI.Services.TimeSeries.CSV.TimeSeriesRepository, DHI.Services.TimeSeries"},
                {"Name", $"CSV time series connection {id}"},
                {"Id", id}
            };
        }

        private async Task AddUpdateDeleteRoundtrip()
        {
            var connection = CreateConnectionBody("csv-discrete-int");
            var url = "api/connections";

            // Add
            var addResponse = await _client.PostAsync(url, ConnectionsContentHelper.GetStringContent(connection));
            addResponse.EnsureSuccessStatusCode();

            // Update
            connection["Name"] = "UPDATED connection name";
            var updateResponse = await _client.PutAsync(url, ConnectionsContentHelper.GetStringContent(connection));
            updateResponse.EnsureSuccessStatusCode();

            // Delete
            var deleteResponse = await _client.DeleteAsync($"{url}/{connection["Id"]}");
            if (deleteResponse.StatusCode != HttpStatusCode.NoContent)
                throw new HttpRequestException($"DELETE failed with {deleteResponse.StatusCode}");
        }
    }
}
