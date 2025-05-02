namespace DHI.Services.Jobs.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Jobs;
    using WebApiCore;
    using Xunit;

    [Collection("Controllers collection")]
    public class JobHostControllerTest
    {
        public JobHostControllerTest(ControllersFixture fixture)
        {
            _client = fixture.Client;
            _options = SerializerOptionsDefault.Options;
        }

        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;

        [Fact]
        public async Task GetIllegalFullNameReturns400BadRequest()
        {
            var response = await _client.GetAsync("api/jobhosts/IllegalHostName");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The ID of a grouped entity must be a string with following format", json);
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync("api/jobhosts/MyGroup1|NonExistingHostName");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = "/api/jobhosts",
                Body = new HostDTO
                {
                    Id = "194.123.123.123",
                    Group = "NonExistingGroupGroup",
                    Name = "MyHost1",
                    Priority = 1,
                    RunningJobsLimit = 1
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
                Url = "/api/jobhosts",
                Body = new HostDTO
                {
                    Id = "194.123.123.123",
                    Name = "MyHost1",
                    Group = "MyGroup1",
                    Priority = 1,
                    RunningJobsLimit = 1
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("already exists", json);
        }

        [Fact]
        public async Task AddExistingIdReturns400BadRequest()
        {
            var request = new
            {
                Url = "/api/jobhosts",
                Body = new HostDTO
                {
                    Id = "194.345.345.345",
                    Name = "MyHost3",
                    Group = "MyGroup2",
                    Priority = 1,
                    RunningJobsLimit = 1
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
                Url = "/api/jobhosts",
                Body = new HostDTO
                {
                    Id = "194.123.123.123",
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The Name field is required", json);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync("api/jobhosts/MyGroup1|NonExistingHostName");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("not found", json);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync("api/jobhosts/MyGroup2|MyHost3");
            var json = await response.Content.ReadAsStringAsync();
            var host = JsonSerializer.Deserialize<Host>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("MyGroup2/MyHost3", host.FullName);
        }

        [Fact]
        public async Task GetAllIsOk()
        {
            var response = await _client.GetAsync("api/jobhosts");
            var json = await response.Content.ReadAsStringAsync();
            var hosts = JsonSerializer.Deserialize<IEnumerable<Host>>(json, _options).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, hosts.Length);
            Assert.Contains(hosts, host => host.Id == "194.234.234.234");
        }

        [Fact]
        public async Task GetByGroupIsOk()
        {
            var response = await _client.GetAsync("api/jobhosts?group=MyGroup1");
            var json = await response.Content.ReadAsStringAsync();
            var hosts = JsonSerializer.Deserialize<IEnumerable<Host>>(json, _options).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, hosts.Length);
            Assert.Contains(hosts, host => host.Id == "194.234.234.234");
            Assert.DoesNotContain(hosts, host => host.Name == "MyHost3");
        }

        [Fact]
        public async Task GetIdsIsOk()
        {
            var response = await _client.GetAsync("api/jobhosts/ids");
            var json = await response.Content.ReadAsStringAsync();
            var ids = JsonSerializer.Deserialize<IEnumerable<string>>(json).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, ids.Length);
            Assert.Contains("194.345.345.345", ids);
        }

        [Fact]
        public async Task GetfullnamesIsOk()
        {
            var response = await _client.GetAsync("api/jobhosts/fullnames");
            var json = await response.Content.ReadAsStringAsync();
            var fullnames = JsonSerializer.Deserialize<IEnumerable<string>>(json).ToArray();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, fullnames.Length);
            Assert.Contains("MyGroup1/MyHost1", fullnames);
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync("api/jobhosts/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonSerializer.Deserialize<int>(json);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, count);
        }

        [Fact]
        public async Task AddUpdateAndDeleteIsOk()
        {
            var request = new
            {
                Url = "/api/jobhosts",
                Body = new HostDTO
                {
                    Id = "194.456.456.456",
                    Group = "MyGroup3",
                    Name = "MyHost",
                    Priority = 5,
                    RunningJobsLimit = 10
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var host = JsonSerializer.Deserialize<Host>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("http://localhost/api/jobhosts/MyGroup3|MyHost", response.Headers.Location.ToString());
            Assert.Equal(request.Body.Id, host.Id);

            // Update
            request.Body.Priority = -5;
            response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            host = JsonSerializer.Deserialize<Host>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(-5, host.Priority);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{FullNameString.ToUrl(host.FullName)}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{FullNameString.ToUrl(host.FullName)}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}