namespace DHI.Services.Jobs.Automations.Test
{
    using DHI.Services.Scalars;
    using Moq;
    using System.Security.Claims;
    using Xunit;

    public class AutomationServiceTest
    {
        private readonly Mock<IAutomationRepository<string>> _repoMock = new();
        private readonly Mock<IScalarRepository<string, int>> _scalarRepoMock = new();
        private readonly Mock<IJobRepository<Guid, string>> _jobRepoMock = new();

        private AutomationService _service;

        public AutomationServiceTest()
        {
            _service = new AutomationService(_repoMock.Object, _scalarRepoMock.Object, _jobRepoMock.Object);
        }

        [Fact]
        public void GetVersionTimestamp_ReturnsUnderlyingValue()
        {
            var expected = DateTime.UtcNow;
            _repoMock.Setup(r => r.GetVersionTimestamp()).Returns(expected);

            var result = _service.GetVersionTimestamp();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void TouchVersion_ReturnsUnderlyingValue()
        {
            var expected = DateTime.UtcNow;
            _repoMock.Setup(r => r.TouchVersion()).Returns(expected);

            var result = _service.TouchVersion();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void EnrichAutomation_HandlesMissingScalars()
        {
            var automation = new Automation("auto", "group", "task") { HostGroup = "host" };
            var user = new ClaimsPrincipal();

            _repoMock.Setup(r => r.GetAll(user)).Returns(new[] { automation });
            _scalarRepoMock.Setup(s => s.GetAll(user)).Returns(new List<Scalar<string, int>>());

            var result = _service.GetAll(user);

            var enriched = Assert.Single(result);
            Assert.False(enriched.IsMet);
            Assert.Null(enriched.LastJob?.Id);
            Assert.Null(enriched.LastJob?.Status);
            Assert.Null(enriched.LastJob?.Requested);
        }

        [Fact]
        public void EnrichAutomation_ParsesIsMetScalarCorrectly()
        {
            var automation = new Automation("auto", "group", "task") { HostGroup = "host" };
            var id = automation.Id;
            var user = new ClaimsPrincipal();
            var prefix = $"Job Automator/{Environment.GetEnvironmentVariable("COMPUTERNAME")}/{id}/host";

            var isMetScalar = new Scalar<string, int>(
                $"{prefix}/Is Met",
                "Is Met",
                "System.String",
                "group",
                new ScalarData("true", DateTime.UtcNow)
            );

            _repoMock.Setup(r => r.GetAll(user)).Returns(new[] { automation });
            _scalarRepoMock.Setup(s => s.GetAll(user)).Returns(new[] { isMetScalar });

            var result = _service.GetAll(user);

            var enriched = Assert.Single(result);
            Assert.True(enriched.IsMet);
        }

        [Fact]
        public void EnrichAutomation_WithInvalidGuid_DoesNotCrash()
        {
            var automation = new Automation("auto", "group", "task") { HostGroup = "host" };
            var id = automation.Id;
            var user = new ClaimsPrincipal();
            var prefix = $"Job Automator/{Environment.GetEnvironmentVariable("COMPUTERNAME")}/{id}/host";

            var jobIdScalar = new Scalar<string, int>(
                $"{prefix}/Last Job Id",
                "Last Job Id",
                "System.String",
                "group",
                new ScalarData("not-a-guid", DateTime.UtcNow)
            );

            _repoMock.Setup(r => r.GetAll(user)).Returns(new[] { automation });
            _scalarRepoMock.Setup(s => s.GetAll(user)).Returns(new[] { jobIdScalar });

            var result = _service.GetAll(user);

            var enriched = Assert.Single(result);
            Assert.Equal(enriched.Id, automation.Id);
            Assert.Null(enriched.LastJob?.Id);
        }

        [Fact]
        public void EnrichAutomation_WithValidGuidAndJob_SetsProperties()
        {
            var automation = new Automation("auto", "group", "task") { HostGroup = "host" };
            var id = automation.Id;
            var user = new ClaimsPrincipal();
            var prefix = $"Job Automator/{Environment.GetEnvironmentVariable("COMPUTERNAME")}/{id}/host";

            var jobId = Guid.NewGuid();

            var jobIdScalar = new Scalar<string, int>(
                $"{prefix}/Last Job Id",
                "Last Job Id",
                "System.String",
                "group",
                new ScalarData(jobId.ToString(), DateTime.UtcNow)
            );

            var job = new Job(jobId, "task-id")
            {
                Requested = new DateTime(2024, 1, 1),
                Status = JobStatus.Completed
            };

            _repoMock.Setup(r => r.GetAll(user)).Returns(new[] { automation });
            _scalarRepoMock.Setup(s => s.GetAll(user)).Returns(new[] { jobIdScalar });
            _jobRepoMock.Setup(j => j.Get(jobId, user)).Returns(new Maybe<Job<Guid, string>>(job));

            var result = _service.GetAll(user);

            var enriched = Assert.Single(result);
            Assert.Equal(jobId, enriched.LastJob?.Id);
            Assert.Equal(job.Requested, enriched.LastJob?.Requested);
            Assert.Equal("Completed", enriched.LastJob?.Status.ToString());
        }
    }
}
