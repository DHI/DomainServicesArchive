namespace DHI.Services.GIS.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Maps;
    using Microsoft.AspNetCore.Http;
    using Xunit;

    [Collection("Controllers collection")]
    public class MapStylesControllerTest : IDisposable
    {
        public MapStylesControllerTest(ControllersFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
            _options = fixture.SerializerOptions;
            fixture.CopyFileToTempAppDataPath("styles.json");
        }

        private readonly ControllersFixture _fixture;
        private readonly HttpClient _client;
        private readonly System.Text.Json.JsonSerializerOptions _options;

        [Fact]
        public async Task GetWithoutBearerTokenReturns401Unauthorized()
        {
            var client = _fixture.CreateClient();
            var response = await client.GetAsync("api/mapstyles/MyStyle");
            Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync("api/mapstyles/NonExistingMapStyle");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetPaletteForNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync("api/mapstyles/NonExistingMapStyle/palette");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddExistingReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/mapstyles",
                Body = new MapStyleDTO
                {
                    Id = "MyStyle",
                    Name = "My style",
                    StyleCode = "0^10:green,yellow,red"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already exists", json);
        }

        [Fact]
        public async Task AddWithMissingStyleCodeReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/mapstyles",
                Body = new MapStyleDTO
                {
                    Id = "MyNewStyle",
                    Name = "My new style"
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The StyleCode field is required", json);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync("api/mapstyles/NonExistingMapStyle");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync("api/mapstyles/Ecoli");
            var json = await response.Content.ReadAsStringAsync();
            var style = JsonSerializer.Deserialize<MapStyle>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Ecoli", style.Id);
        }

        [Fact]
        public async Task GetPaletteIsOk()
        {
            var response = await _client.GetAsync("api/mapstyles/Ecoli/palette");
            var json = await response.Content.ReadAsStringAsync();
            var palette = JsonSerializer.Deserialize<Dictionary<double, MapStyleBand>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(10, palette.Count);
            Assert.Contains(palette, band => band.Value.BandValue.Equals(10));
        }


        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync("api/mapstyles");
            var json = await response.Content.ReadAsStringAsync();
            var styles = JsonSerializer.Deserialize<IEnumerable<MapStyle>>(json, _options).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, styles.Length);
            Assert.Contains(styles, style => style.Id == "MyStyle");
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync("api/mapstyles/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, count);
        }

        [Fact]
        public async Task AddAndDeleteIsOk()
        {
            var request = new
            {
                Url = "/api/mapstyles",
                Body = new MapStyleDTO
                {
                    Id = "MyNewStyle",
                    Name = "My new style",
                    StyleCode = "0^10:green,yellow,red"
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var style = JsonSerializer.Deserialize<MapStyle>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/mapstyles/{request.Body.Id}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.Id, style.Id);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{style.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{style.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        public void Dispose()
        {
            File.Delete(Path.Combine(_fixture.TempAppDataPath, "styles.json"));
        }
    }
}
