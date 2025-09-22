namespace IntegrationTestHost.Tests
{
    using global::DHI.Services.Spreadsheets.WebApi;
    using System.Net.Http.Json;
    using System.Net;
    using System.Text.Json;
    using Xunit.Abstractions;
    using DHI.Services.WebApiCore;

    public class SpreadsheetsControllerIntegrationTest : IClassFixture<ControllersFixture>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly ITestOutputHelper _output;
        private const string ConnectionId = "xlsx";
        private const string SpreadsheetFullName = "TestData.xlsx";

        public SpreadsheetsControllerIntegrationTest(ControllersFixture fixture, ITestOutputHelper output)
        {
            _client = fixture.CreateAuthenticatedClientAsAdmin();
            _options = fixture.SerializerOptions;
            _output = output;
        }

        [Fact(DisplayName = "Run full SpreadsheetsController integration suite")]
        public async Task Run_SpreadsheetsController_IntegrationFlow()
        {
            await Step("EnsureSpreadsheetDoesNotExist", EnsureSpreadsheetDoesNotExist);
            await Step("AddSpreadsheet", AddSpreadsheet);
            await Step("GetSpreadsheet", GetSpreadsheet);
            await Step("UpdateSpreadsheet", UpdateSpreadsheet);
            await Step("GetSpreadsheetList", GetSpreadsheetList);
            await Step("GetSpreadsheetCount", GetSpreadsheetCount);
            await Step("GetSpreadsheetFullNames", GetSpreadsheetFullNames);
            await Step("GetCellValue", GetCellValue);
            await Step("GetRange", GetRange);
            await Step("GetNamedRange", GetNamedRange);
            await Step("GetUsedRange", GetUsedRange);
            await Step("GetUsedRangeFormats", GetUsedRangeFormats);
            await Step("GetStream", GetStream);
            await Step("DeleteSpreadsheet", DeleteSpreadsheet);
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

        private async Task EnsureSpreadsheetDoesNotExist()
        {
            var id = FullNameString.ToUrl(SpreadsheetFullName);
            var response = await _client.GetAsync($"/api/spreadsheets/{ConnectionId}/{id}");
            _output.WriteLine(await response.Content.ReadAsStringAsync());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var delete = await _client.DeleteAsync($"/api/spreadsheets/{ConnectionId}/{id}");
                _output.WriteLine(await delete.Content.ReadAsStringAsync());
                Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
            }
        }

        private async Task AddSpreadsheet()
        {
            var dto = new SpreadsheetDTO
            {
                FullName = SpreadsheetFullName,
                SheetNames = new List<string> { "Sheet1" },
                Data = new List<object[,]>
                {
                    new object[,]
                    {
                        { "A", "B" },
                        { 1, 2 }
                    }
                }
            };

            _output.WriteLine(await SpreadsheetsContentHelper.GetStringContent(dto).ReadAsStringAsync());
            var response = await _client.PostAsync($"/api/spreadsheets/{ConnectionId}", SpreadsheetsContentHelper.GetStringContent(dto));
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task GetSpreadsheet()
        {
            var id = FullNameString.ToUrl(SpreadsheetFullName);
            var response = await _client.GetAsync($"/api/spreadsheets/{ConnectionId}/{id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task UpdateSpreadsheet()
        {
            var dto = new SpreadsheetDTO
            {
                FullName = SpreadsheetFullName,
                SheetNames = new List<string> { "Sheet1" },
                Data = new List<object[,]>
                {
                    new object[,]
                    {
                        { "Updated", 999 }
                    }
                }
            };

            var response = await _client.PutAsync($"/api/spreadsheets/{ConnectionId}", SpreadsheetsContentHelper.GetStringContent(dto));
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetSpreadsheetList()
        {
            var response = await _client.GetAsync($"/api/spreadsheets/{ConnectionId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetSpreadsheetCount()
        {
            var response = await _client.GetAsync($"/api/spreadsheets/{ConnectionId}/count");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetSpreadsheetFullNames()
        {
            var response = await _client.GetAsync($"/api/spreadsheets/{ConnectionId}/fullnames");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetCellValue()
        {
            var id = FullNameString.ToUrl(SpreadsheetFullName);
            var response = await _client.GetAsync($"/api/spreadsheets/{ConnectionId}/WhiteNile.xlsx/Catchments/cell;R2C1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetRange()
        {
            var id = FullNameString.ToUrl(SpreadsheetFullName);
            var response = await _client.GetAsync($"/api/spreadsheets/{ConnectionId}/WhiteNile.xlsx/Catchments/range;R1C1,R7C1");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetNamedRange()
        {
            var response = await _client.GetAsync($"/api/spreadsheets/{ConnectionId}/WhiteNile.xlsx/Catchments/namedrange;CatchmentData");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetUsedRange()
        {
            var id = FullNameString.ToUrl(SpreadsheetFullName);
            var response = await _client.GetAsync($"/api/spreadsheets/{ConnectionId}/WhiteNile.xlsx/Catchments/usedrange");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetUsedRangeFormats()
        {
            var id = FullNameString.ToUrl(SpreadsheetFullName);
            var response = await _client.GetAsync($"/api/spreadsheets/{ConnectionId}/WhiteNile.xlsx/Catchments/formats/usedrange");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task GetStream()
        {
            var id = FullNameString.ToUrl(SpreadsheetFullName);
            var response = await _client.GetAsync($"/api/spreadsheets/{ConnectionId}/stream/WhiteNile.xlsx");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task DeleteSpreadsheet()
        {
            var id = FullNameString.ToUrl(SpreadsheetFullName);
            var response = await _client.DeleteAsync($"/api/spreadsheets/{ConnectionId}/{id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var confirm = await _client.GetAsync($"/api/spreadsheets/{ConnectionId}/{id}");
            Assert.Equal(HttpStatusCode.NotFound, confirm.StatusCode);
        }
    }
}
