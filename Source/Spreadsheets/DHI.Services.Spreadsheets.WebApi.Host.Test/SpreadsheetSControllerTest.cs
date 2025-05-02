namespace DHI.Services.Spreadsheets.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using WebApiCore;
    using Xunit;

    [Collection("Controllers collection")]
    public class SpreadsheetsControllerTest
    {
        public SpreadsheetsControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
            _options = fixture.SerializerOptions;
        }

        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private const string ConnectionId = "xlsx";

        [Fact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync($"api/spreadsheets/{ConnectionId}/WhiteNile.xlsx");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/spreadsheets/{ConnectionId}/NonExisting.xlsx");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = $"/api/spreadsheets/{ConnectionId}",
                Body = new SpreadsheetDTO
                {
                    FullName = "NonExisting.xlsx",
                    Data = new List<object[,]>()
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddExistingReturns400BadRequest()
        {
            var request = new
            {
                Url = $"/api/spreadsheets/{ConnectionId}",
                Body = new SpreadsheetDTO
                {
                    FullName = "WhiteNile.xlsx",
                    Data = new List<object[,]>()
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already exists", json);
        }


        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync($"api/spreadsheets/{ConnectionId}/NonExistingSpreadsheet.xlsx");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteByNonExistingGroupReturns404NotFound()
        {
            var response = await _client.DeleteAsync($"api/spreadsheets/{ConnectionId}/group/NonExistingGroup");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/spreadsheets/{ConnectionId}/WhiteNile.xlsx");
            var json = await response.Content.ReadAsStringAsync();
            var spreadsheet = JsonSerializer.Deserialize<Spreadsheet<string>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var sheetNames = JsonSerializer.Deserialize<string[]>(spreadsheet.Metadata["SheetNames"].ToString());
            Assert.Contains("Catchments", sheetNames);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync($"api/spreadsheets/{ConnectionId}");
            var json = await response.Content.ReadAsStringAsync();
            var spreadsheets = JsonSerializer.Deserialize<Spreadsheet<string>[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(spreadsheets);
            Assert.Contains(spreadsheets, s => s.Name == "WhiteNile.xlsx");
        }

        [Fact]
        public async Task GetFullnamesIsOk()
        {
            var response = await _client.GetAsync($"api/spreadsheets/{ConnectionId}/fullnames");
            var json = await response.Content.ReadAsStringAsync();
            var fullnames = JsonSerializer.Deserialize<string[]>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(fullnames);
            Assert.Contains("WhiteNile.xlsx", fullnames);
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync($"api/spreadsheets/{ConnectionId}/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task GetCellValueIsOk()
        {
            var response = await _client.GetAsync($"api/spreadsheets/{ConnectionId}/WhiteNile.xlsx/Catchments/cell;R2C1");
            var json = await response.Content.ReadAsStringAsync();
            var value = JsonSerializer.Deserialize<double>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(70627.46, value);
        }

        [Fact]
        public async Task GetRangeValuesIsOk()
        {
            var response = await _client.GetAsync($"api/spreadsheets/{ConnectionId}/WhiteNile.xlsx/Catchments/range;R1C1,R7C1");
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<double[,]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3108.55, data.GetValue(3, 0));
        }

        [Fact]
        public async Task GetUsedRangeValuesIsOk()
        {
            var response = await _client.GetAsync($"api/spreadsheets/{ConnectionId}/WhiteNile.xlsx/Catchments/usedrange");
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object[,]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Jebel Aulia", data.GetValue(3, 0));
        }

        [Fact]
        public async Task GetUsedRangeFormatsIsOk()
        {
            var response = await _client.GetAsync($"api/spreadsheets/{ConnectionId}/WhiteNile.xlsx/Catchments/formats/usedrange");
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<CellFormat[,]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(CellFormat.Text, data.GetValue(3, 0));
            Assert.Equal(CellFormat.Number, data.GetValue(3, 4));
        }

        [Fact]
        public async Task GetNamedRangeValuesIsOk()
        {
            var response = await _client.GetAsync($"api/spreadsheets/{ConnectionId}/WhiteNile.xlsx/Catchments/namedrange;CatchmentData");
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object[,]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3108.55, data.GetValue(4, 1));
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            var request = new
            {
                Url = $"/api/spreadsheets/{ConnectionId}",
                Body = new SpreadsheetDTO
                {
                    FullName = "TestData.xlsx",
                    SheetNames = new List<string> { "Catchments", "Cities" },
                    Data = new List<object[,]>
                    {
                        new object[,]
                        {
                            {"Catchment Name", "Area"},
                            {"MyCatchmentName", 56250.87}
                        },
                        new object[,]
                        {
                            {"City Name", "Population"},
                            {"Copenhagen", 1000000},
                            {"London", 10000000}
                        }
                    }
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var spreadsheet = JsonSerializer.Deserialize<Spreadsheet<string>>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/spreadsheets/{ConnectionId}/TestData.xlsx", response.Headers.Location.ToString());
            Assert.Equal(request.Body.FullName, spreadsheet.FullName);

            // Update
            request.Body.Data[1].SetValue(900000, 1, 1);
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            response = await _client.GetAsync($"api/spreadsheets/{ConnectionId}/{request.Body.FullName}/Cities/usedrange");
            json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object[,]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(900000, data.GetValue(1, 1));

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{spreadsheet.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{spreadsheet.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddAndDeleteByGroupIsOk()
        {
            var request = new
            {
                Url = $"/api/spreadsheets/{ConnectionId}",
                Body = new SpreadsheetDTO
                {
                    FullName = "MyGroup/MySubGroup/TestData.xlsx",
                    SheetNames = new List<string> { "Catchments", "Cities" },
                    Data = new List<object[,]>
                    {
                        new object[,]
                        {
                            {"Catchment Name", "Area"},
                            {"MyCatchmentName", 56250.87}
                        },
                        new object[,]
                        {
                            {"City Name", "Population"},
                            {"Copenhagen", 1000000},
                            {"London", 10000000}
                        }
                    }
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var spreadsheet = JsonSerializer.Deserialize<Spreadsheet<string>>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/spreadsheets/{ConnectionId}/MyGroup|MySubGroup|TestData.xlsx", response.Headers.Location.ToString());
            Assert.Equal(request.Body.FullName, spreadsheet.FullName);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/group/{FullNameString.ToUrl(spreadsheet.Group)}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{FullNameString.ToUrl(spreadsheet.FullName)}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetStreamIsOk()
        {
            var response = await _client.GetAsync($"api/spreadsheets/{ConnectionId}/stream/WhiteNile.xlsx");
            var stream = await response.Content.ReadAsStreamAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", response.Content.Headers.ContentType.ToString());
            Assert.Equal(12798, stream.Length);
        }
    }
}