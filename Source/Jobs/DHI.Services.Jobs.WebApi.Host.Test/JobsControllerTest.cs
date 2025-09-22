namespace DHI.Services.Jobs.WebApi.Host.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;
    using JsonConvert = System.Text.Json.JsonSerializer;

    [Collection("Controllers collection")]
    public class JobsControllerTest
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private const string _connectionId = "wf-jobs";

        public JobsControllerTest(ControllersFixture factory)
        {
            _client = factory.Client;
            _options = SerializerOptionsDefault.Options;
        }

        [Fact]
        public async Task GetNonExistingReturns404NotFound()
        {
            var response = await _client.GetAsync($"api/jobs/{_connectionId}/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteNonExistingReturns404NotFound()
        {
            var response = await _client.DeleteAsync($"api/jobs/{_connectionId}/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = $"api/jobs/{_connectionId}",
                Body = new JobUpdateDTO(Guid.NewGuid(), "WriteToFile", DateTime.Now, JobStatus.Error)
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CancelNonExistingReturns404NotFound()
        {
            var response = await _client.PutAsync($"api/jobs/{_connectionId}/{Guid.NewGuid()}/cancel", null);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CancelCompletedReturns400BadRequest()
        {
            var response = await _client.PutAsync($"api/jobs/{_connectionId}/b1d70255-42f8-4c3c-b435-c888319dff76/cancel", null);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CancelManyForNonExistingReturns404NotFound()
        {
            var request = new
            {
                Url = $"api/jobs/{_connectionId}/cancel",
                Body = new[]
                {
                    Guid.NewGuid()
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCountIsOk()
        {
            var response = await _client.GetAsync($"api/jobs/{_connectionId}/count");
            var json = await response.Content.ReadAsStringAsync();
            var count = JsonConvert.Deserialize<int>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(6, count);
        }

        [Fact]
        public async Task GetIsOk()
        {
            var response = await _client.GetAsync($"api/jobs/{_connectionId}?status=Completed");
            var json = await response.Content.ReadAsStringAsync();
            var jobs = JsonConvert.Deserialize<IEnumerable<Job<Guid, string>>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, jobs.Count());

            response = await _client.GetAsync($"api/jobs/{_connectionId}?since=2015-01-3T09:00:00&status=Error");
            json = await response.Content.ReadAsStringAsync();
            jobs = JsonConvert.Deserialize<IEnumerable<Job<Guid, string>>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(jobs);
            Assert.Equal("Job4", jobs.Last().Tag);
        }

        [Fact]
        public async Task GetByQueryIsOk()
        {
            var request = new
            {
                Url = $"/api/jobs/{_connectionId}/query",
                Body = new object[]
                {
                    new { Item = "Status", QueryOperator = "Equal", Value = "Completed" },
                }
            };

            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var jobs = JsonConvert.Deserialize<Job<Guid, string>[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, jobs.Length);

            request = new
            {
                Url = $"/api/jobs/{_connectionId}/query",
                Body = new object[]
                {
                    new { Item = "Status", QueryOperator = "Equal", Value = "Error" },
                    new { Item = "Requested", QueryOperator = "GreaterThanOrEqual", Value = "2015-01-3T09:00:00" }
                }
            };

            response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            json = await response.Content.ReadAsStringAsync();
            jobs = JsonConvert.Deserialize<Job<Guid, string>[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(jobs);
            Assert.Equal("Job4", jobs.Last().Tag);
        }

        [Fact]
        public async Task GetByQueryNotMetReturnsEmpty()
        {
            var queryRequest = new
            {
                Url = $"/api/jobs/{_connectionId}/query",
                Body = new object[]
                {
                    new { Item = "Status", QueryOperator = "Equal", Value = "Cancelled" },
                }
            };

            var response = await _client.PostAsync(queryRequest.Url, ContentHelper.GetStringContent(queryRequest.Body));
            var json = await response.Content.ReadAsStringAsync();
            var jobs = JsonConvert.Deserialize<Job<Guid, string>[]>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(jobs);
        }

        [Fact]
        public async Task GetReturnsEmptyArrayIfNothingFound()
        {
            var response = await _client.GetAsync($"api/jobs/{_connectionId}?since=2099-01-3");
            var json = await response.Content.ReadAsStringAsync();
            var jobs = JsonConvert.Deserialize<IEnumerable<Job<Guid, string>>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(jobs);
        }

        [Fact]
        public async Task GetLastIsOk()
        {
            var response = await _client.GetAsync($"api/jobs/{_connectionId}/last?status=Completed");
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Job3", job.Tag);

            response = await _client.GetAsync($"api/jobs/{_connectionId}/last?status=Completed&account=User1");
            json = await response.Content.ReadAsStringAsync();
            job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Job2", job.Tag);
        }

        [Fact]
        public async Task GetLastReturns204NoContentIfNothingFound()
        {
            var response = await _client.GetAsync($"api/jobs/{_connectionId}/last?status=Pending");
            var json = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", json);
        }

        // [Fact]
        public async Task AddWithParamsIsOk()
        {
            var request = new
            {
                Url = $"/api/jobs/{_connectionId}",
                Body = new JobDTO
                {
                    TaskId = "WriteToFileWParam",
                    Priority = 1,
                    Parameters = new Dictionary<string, object>
                    {
                        ["FolderName"] = "hello/"
                    }
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/jobs/{_connectionId}/{job.Id}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.Parameters["FolderName"], job.Parameters["FolderName"]);

            response = await _client.DeleteAsync($"{request.Url}/{job.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{job.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddUpdateAndDeleteByIdIsOk()
        {
            var request = new
            {
                Url = $"/api/jobs/{_connectionId}",
                Body = new JobDTO
                {
                    TaskId = "WriteToFile",
                    Priority = 1,
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/jobs/{_connectionId}/{job.Id}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.HostGroup, job.HostGroup);

            // Update
            var updateRequest = new
            {
                Url = $"/api/jobs/{_connectionId}",
                Body = new JobUpdateDTO(job.Id, job.TaskId, job.Requested, job.Status)
                {
                    Priority = 99,
                    Tag = "MyTag"
                }
            };

            response = await _client.PutAsync(updateRequest.Url, ContentHelper.GetStringContent(updateRequest.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            json = await response.Content.ReadAsStringAsync();
            job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);
            Assert.Equal(updateRequest.Body.Priority, job.Priority);
            Assert.Equal(updateRequest.Body.Tag, job.Tag);
            response = await _client.GetAsync($"api/jobs/{_connectionId}/{job.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            json = await response.Content.ReadAsStringAsync();
            job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);
            Assert.Equal(updateRequest.Body.Priority, job.Priority);
            Assert.Equal(updateRequest.Body.Tag, job.Tag);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{job.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{job.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddAndDeleteJobsByQueryIsOk()
        {
            var request = new
            {
                Url = $"/api/jobs/{_connectionId}",
                Body = new JobDTO
                {
                    TaskId = "WriteToFile",
                    Priority = 1
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/jobs/{_connectionId}/{job.Id}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.HostGroup, job.HostGroup);

            // Delete jobs by user
            response = await _client.DeleteAsync($"{request.Url}?account=john.doe");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{job.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CancelInProgressIsOk()
        {
            var response = await _client.GetAsync($"api/jobs/{_connectionId}?status=InProgress");
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<IEnumerable<Job<Guid, string>>>(json, _options).First();

            // Cancel
            response = await _client.PutAsync($"api/jobs/{_connectionId}/{job.Id}/cancel", null);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            response = await _client.GetAsync($"api/jobs/{_connectionId}/{job.Id}");
            json = await response.Content.ReadAsStringAsync();
            job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);
            Assert.Equal(JobStatus.Cancel, job.Status);
        }

        [Fact]
        public async Task AddAndCancelManyIsOk()
        {
            var request = new
            {
                Url = $"/api/jobs/{_connectionId}",
                Body = new JobDTO
                {
                    TaskId = "WriteToFile",
                    Priority = 1
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Cancel
            var cancelRequest = new
            {
                Url = $"api/jobs/{_connectionId}/cancel",
                Body = new[]
                {
                    job.Id
                }
            };

            response = await _client.PutAsync(cancelRequest.Url, ContentHelper.GetStringContent(cancelRequest.Body));
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            response = await _client.GetAsync($"api/jobs/{_connectionId}/{job.Id}");
            json = await response.Content.ReadAsStringAsync();
            job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);
            Assert.Equal(JobStatus.Cancel, job.Status);

            //should also delete the job in case of side effect to other test method within the same collection
            response = await _client.DeleteAsync($"{request.Url}/{job.Id}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{job.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateStatusIsOk()
        {
            // Update
            var updateRequest = new
            {
                Url = $"/api/jobs/{_connectionId}/status/111131e3-39f6-40be-b2c3-4aff8e196abc",
                Body = new
                {
                    StatusMessage = "Completed",
                    JobStatus = JobStatus.InProgress,
                    Progress = 100
                }
            };

            var response = await _client.PutAsync(updateRequest.Url, ContentHelper.GetStringContent(updateRequest.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync($"api/jobs/{_connectionId}/111131e3-39f6-40be-b2c3-4aff8e196abc");
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);
            Assert.Equal(JobStatus.InProgress, job.Status);
            Assert.Equal("Completed", job.StatusMessage);
            Assert.Equal(100, job.Progress);
        }

        [Fact]
        public async Task UpdateStatusNoMessageIsOk()
        {
            // Update
            var updateRequest = new
            {
                Url = $"/api/jobs/{_connectionId}/status/111131e3-39f6-40be-b2c3-4aff8e196abc",
                Body = new
                {
                    JobStatus = JobStatus.InProgress
                }
            };

            var response = await _client.PutAsync(updateRequest.Url, ContentHelper.GetStringContent(updateRequest.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync($"api/jobs/{_connectionId}/111131e3-39f6-40be-b2c3-4aff8e196abc");
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);
            Assert.Equal(JobStatus.InProgress, job.Status);
            Assert.Null(job.StatusMessage);
            Assert.Null(job.Progress);
        }

        [Fact]
        public async Task UpdateHeartbeatIsOk()
        {
            // Update
            var updateRequest = new
            {
                Url = $"/api/jobs/{_connectionId}/heartbeat/111131e3-39f6-40be-b2c3-4aff8e196abc"
            };

            var response = await _client.PutAsync(updateRequest.Url, null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _client.GetAsync($"api/jobs/{_connectionId}/111131e3-39f6-40be-b2c3-4aff8e196abc");
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);
            Assert.NotNull(job.Heartbeat);
        }

        [Fact]
        public async Task AddWithJobIdAndDeleteByIdIsOk()
        {
            var request = new
            {
                Url = $"/api/jobs/{_connectionId}",
                Body = new JobDTO
                {
                    TaskId = "WriteToFile",
                    Priority = 1,
                    Id = Guid.NewGuid()
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal($"http://localhost/api/jobs/{_connectionId}/{job.Id}", response.Headers.Location.ToString());
            Assert.Equal(request.Body.HostGroup, job.HostGroup);
            Assert.Equal(job.Id, request.Body.Id);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{job.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{job.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddAndDeleteByStatusIsOk()
        {
            var request = new
            {
                Url = $"/api/jobs/{_connectionId}",
                Body = new JobDTO
                {
                    TaskId = "WriteToFile",
                    Priority = 1,
                    Id = Guid.NewGuid()
                }
            };

            // Add
            var response = await _client.PostAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            var json = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);

            // Delete
            response = await _client.DeleteAsync($"{request.Url}/{job.Id}?Status={job.Status}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            response = await _client.GetAsync($"{request.Url}/{job.Id}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateStatus_OnFinalJob_Returns409Conflict_AndDoesNotChangeState()
        {
            var id = new Guid("b1d70255-42f8-4c3c-b435-c888319dff76");

            var request = new
            {
                Url = $"/api/jobs/{_connectionId}/status/{id}",
                Body = new
                {
                    StatusMessage = "trying to regress",
                    JobStatus = JobStatus.InProgress,
                    Progress = 10
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            var check = await _client.GetAsync($"api/jobs/{_connectionId}/{id}");
            var json = await check.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);

            Assert.Equal(JobStatus.Completed, job.Status);
        }

        [Fact]
        public async Task UpdateStatus_ResendingSameFinal_IsOk_AndRemainsFinal()
        {
            var id = new Guid("b1d70255-42f8-4c3c-b435-c888319dff76");

            var request = new
            {
                Url = $"/api/jobs/{_connectionId}/status/{id}",
                Body = new
                {
                    StatusMessage = "Completed again",
                    JobStatus = JobStatus.Completed,
                    Progress = (int?)null
                }
            };

            var response = await _client.PutAsync(request.Url, ContentHelper.GetStringContent(request.Body));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var check = await _client.GetAsync($"api/jobs/{_connectionId}/{id}");
            var json = await check.Content.ReadAsStringAsync();
            var job = JsonConvert.Deserialize<Job<Guid, string>>(json, _options);

            Assert.Equal(JobStatus.Completed, job.Status);
        }
    }
}