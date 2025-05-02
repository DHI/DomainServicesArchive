namespace DHI.Services.Jobs.WebApi.Host.Test
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;
    using JsonConvert = System.Text.Json.JsonSerializer;

    [Collection("Controllers collection")]
    public class JobHealthControllerTest
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private const string _connectionId = "wf-jobs";
        private readonly IJobService<string> _jobService;

        public JobHealthControllerTest(ControllersFixture factory)
        {
            _client = factory.Client;
            _options = SerializerOptionsDefault.Options;
            _jobService = Services.Get<IJobService<string>>(_connectionId);
        }

        /// <summary>
        ///  Test method DHI.Services.Jobs.WebApi.JobController.GapJobsInPastHoursByFieldForTaskId(string connectionId, string field, double pastHours, string taskId = null)
        /// </summary>
        /// <remarks> 
        /// The data source comes from jobs.json within test project, referring to ConnectionId and ControllersFixture
        /// </remarks>
        [Theory]
        [InlineData("Requested")]
        [InlineData("requested")]
        public async Task GapJobsInPastHoursByFieldForTaskIdIsOk(string field)
        {
            // prepare input argument
            var hours = Math.Ceiling((DateTime.UtcNow - new DateTime(2015, 01, 04, 0, 0, 0, DateTimeKind.Utc)).TotalHours);

            // prepare expected result
            var expectedJobs = _jobService.GetAll().Where(job => job.Requested >= new DateTime(2015, 01, 04, 0, 0, 0, DateTimeKind.Local));

            // request and assert
            var response = await _client.GetAsync($"/api/jobs/{_connectionId}/health/gap/{field}/{hours}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var actualJobs = JsonConvert.Deserialize<Job<Guid, string>[]>(json, _options);

            Assert.Equal(expectedJobs.Count(), actualJobs.Length);
        }

        /// <summary>
        ///  Test method DHI.Services.Jobs.WebApi.JobController.GapJobsInPastHoursByFieldForTaskId(string connectionId, string field, double pastHours, string taskId = null)
        /// </summary>
        /// <remarks> 
        /// The data source comes from jobs.json within test project, referring to ConnectionId and ControllersFixture
        /// </remarks>
        [Theory]
        [InlineData("Requested")]
        public async Task GapJobsInPastHoursByFieldForTaskIdReturns404NotFound(string field)
        {
            var hours = 1;
            var response = await _client.GetAsync($"/api/jobs/{_connectionId}/health/gap/{field}/{hours}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        ///  Test method DHI.Services.Jobs.WebApi.JobController..GapJobsInPastHoursByFieldForTaskId(string connectionId, string field, double pastHours, string taskId = null)
        /// </summary>
        /// <remarks> 
        /// The data source comes from jobs.json within test project, referring to ConnectionId and ControllersFixture
        /// </remarks>
        [Theory]
        [InlineData("invalidFieldName")]
        [InlineData("AccountId")] // the field type is not DateTime
        public async Task GapJobsInPastHoursByFieldForTaskIdReturns400BadRequest(string fieldName)
        {
            var hours = 1;
            var response = await _client.GetAsync($"/api/jobs/{_connectionId}/health/gap/{fieldName}/{hours}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// Test method DHI.Services.Jobs.WebApi.JobController.IncompleteJobsInPastHoursForTaskId(string connectionId, double pastHours, string taskId = default)
        /// </summary>
        /// <remarks> 
        /// The data source comes from jobs.json within test project, referring to ConnectionId and ControllersFixture
        /// </remarks>
        [Fact]
        public async Task IncompleteJobsInPastHoursForTaskIdIsOk()
        {
            var since = new DateTime(2015, 01, 02, 0, 0, 0, DateTimeKind.Utc);
            var hours = Math.Ceiling((DateTime.UtcNow - since).TotalHours);

            // prepare expected result
            var expectedJobs = _jobService.GetAll().Where(job => job.Requested > since && job.Status != JobStatus.Completed);

            // request and assert
            var response = await _client.GetAsync($"/api/jobs/{_connectionId}/health/incomplete/{hours}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var actualJobs = JsonConvert.Deserialize<Job<Guid, string>[]>(json, _options);
            Assert.Equal(expectedJobs.Count(), actualJobs.Length);
        }

        /// <summary>
        /// Test method DHI.Services.Jobs.WebApi.JobController.IncompleteJobsInPastHoursForTaskId(string connectionId, double pastHours, string taskId = default)
        /// </summary>
        /// <remarks> 
        /// The data source comes from jobs.json within test project, referring to ConnectionId and ControllersFixture
        /// </remarks>
        [Fact]
        public async Task IncompleteJobsInPastHoursForTaskIdReturns404NotFound()
        {
            var hours = 2;
            var response = await _client.GetAsync($"/api/jobs/{_connectionId}/health/incomplete/{hours}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Test method DHI.Services.Jobs.WebApi.JobController.ErrorRatioInPastHours(string connectionId, double pastHours, double maxErrorRatio) 
        /// </summary>
        /// <remarks> 
        /// The data source comes from jobs.json within test project, referring to ConnectionId and ControllersFixture
        /// </remarks>
        [Fact]
        public async Task ErrorRatioInPastHoursIsOk()
        {
            var since = new DateTime(2015, 01, 02, 0, 0, 0, DateTimeKind.Utc);
            var hours = Math.Ceiling((DateTime.UtcNow - since).TotalHours);
            var maxErrorRatio = 40;

            var expectedJobsCount = _jobService.Get(null, since, status: null)
                                               .Count(job => job.Status == JobStatus.Error || job.Status == JobStatus.Completed);

            var response = await _client.GetAsync($"/api/jobs/{_connectionId}/health/errorratio/{hours}/{maxErrorRatio}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var actualJobs = JsonConvert.Deserialize<Job<Guid, string>[]>(json, _options);
            Assert.Equal(expectedJobsCount, actualJobs.Length);
        }

        /// <summary>
        /// Test method DHI.Services.Jobs.WebApi.JobController.ErrorRatioInPastHours(string connectionId, double pastHours, double maxErrorRatio) 
        /// </summary>
        /// <remarks> 
        /// The data source comes from jobs.json within test project, referring to ConnectionId and ControllersFixture
        /// </remarks>
        [Fact]
        public async Task ErrorRatioInPastHoursReturn404NotFound()
        {
            var hours = 2;
            var maxErrorRatio = 50;
            var response = await _client.GetAsync($"/api/jobs/{_connectionId}/health/errorratio/{hours}/{maxErrorRatio}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Test method DHI.Services.Jobs.WebApi.JobController.HealthDelayedJobsInPastHourss(string connectionId, double pastHours, double maxDelayMinutes)
        /// </summary>
        /// <remarks>
        /// The data source comes from jobs.json within test project, referring to ConnectionId and ControllersFixture
        /// </remarks>
        [Fact]
        public async Task DelayedJobsInPastHoursIsOk()
        {
            double maxDelayMinutes = 60;
            var hours = Math.Ceiling((DateTime.UtcNow - new DateTime(2015, 01, 04, 0, 0, 0, DateTimeKind.Utc)).TotalHours);

            var response = await _client.GetAsync($"/api/jobs/{_connectionId}/health/delay/{hours}/{maxDelayMinutes}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var actualJobs = JsonConvert.Deserialize<Job<Guid, string>[]>(json, _options);
            Assert.Single(actualJobs);
        }

        /// <summary>
        /// Test method DHI.Services.Jobs.WebApi.JobController.DelayedJobsInPastHours(string connectionId, double pastHours, double maxDelayMinutes)
        /// </summary>
        /// <remarks>
        /// The data source comes from jobs.json within test project, referring to ConnectionId and ControllersFixture
        /// </remarks>
        [Fact]
        public async Task DelayedJobsInPastHoursReturns404NotFound()
        {
            double maxDelayMinutes = 60;
            var hours = Math.Ceiling((DateTime.UtcNow - new DateTime(2015, 01, 04, 12, 0, 0, DateTimeKind.Utc)).TotalHours);

            var response = await _client.GetAsync($"/api/jobs/{_connectionId}/health/delay/{hours}/{maxDelayMinutes}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Test method DHI.Services.Jobs.WebApi.JobController.MinimumStartedJobsInPastHours(string connectionId, double pastHours, int expectedNumberOfJobs)
        /// </summary>
        /// <remarks> 
        /// The data source comes from jobs.json within test project, referring to ConnectionId and ControllersFixture
        /// </remarks>
        [Fact]
        public async Task MinimumStartedInPastHoursIsOk()
        {
            var expectedNumberOfJobs = 2;
            var hours = Math.Ceiling((DateTime.UtcNow - new DateTime(2015, 01, 04, 0, 0, 0, DateTimeKind.Utc)).TotalHours);

            var response = await _client.GetAsync($"/api/jobs/{_connectionId}/health/minimumstarted/{hours}/{expectedNumberOfJobs}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var actualJobs = JsonConvert.Deserialize<Job<Guid, string>[]>(json, _options);

            var expectedJobs = _jobService.Get(null, DateTime.UtcNow.AddHours(-hours), null, default, null, null);
            Assert.Equal(expectedJobs.Count(), actualJobs.Length);
        }

        /// <summary>
        /// Test method DHI.Services.Jobs.WebApi.JobController.MinimumStartedJobsInPastHours(string connectionId, double pastHours, int expectedNumberOfJobs)
        /// </summary>
        /// <remarks> 
        /// The data source comes from jobs.json within test project, referring to ConnectionId and ControllersFixture
        /// </remarks>
        [Fact]
        public async Task MinimumStartedInPastHoursReturns404NotFound()
        {
            var expectedNumberOfJobs = 1;
            var hours = 2;

            var response = await _client.GetAsync($"/api/jobs/{_connectionId}/health/minimumstarted/{hours}/{expectedNumberOfJobs}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
