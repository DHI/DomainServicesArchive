namespace DHI.Services.Jobs.WorkflowWorker.Test.Fixtures
{
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using Workflows;
    using Xunit;

    public class SignalRWorkflowWorkerFixture : IDisposable
    {
        public SignalRWorkflowWorker Worker { get; }

        public SignalRWorkflowWorkerFixture()
        {
            var mockHub = new Mock<IHubContext<WorkerHub>>();
            var mockClients = new Mock<IHubClients>();
            var clientProxy = new Mock<IClientProxy>();

            mockHub.Setup(c => c.Clients.User("localhost")).Returns(clientProxy.Object);
            var mockLogger = new Mock<ILogger>();
            Worker = new SignalRWorkflowWorker(mockHub.Object, new AvailableCache(), mockLogger.Object);
        }

        public void ExecuteWorkflow(Guid jobId)
        {
            Worker.Execute(jobId, GetWorkflow(), new Dictionary<string, object>(), "localhost");
        }

        public static CodeWorkflow GetWorkflow()
        {
            return GetWorkflowWith(null, null);
        }

        public static CodeWorkflow GetWorkflowWith(string parameter, string value)
        {
            var defaults = new Dictionary<string, string>
            {
                ["id"] = "h.s",
                ["name"] = "harry",
                ["assemblyName"] = "oneD"
            };

            if (!string.IsNullOrWhiteSpace(parameter))
            {
                defaults[parameter] = value;
            }

            return new CodeWorkflow(defaults["id"], defaults["name"], defaults["assemblyName"]);
        }

        public StringContent CreateMessage(string progressChanged, Guid guid, string host, string text, int? progress)
        {
            var message = new
            {
                Type = progressChanged,
                JobId = guid.ToString(),
                HostId = host,
                Text = text,
                Progress = progress
            };
            return new StringContent(JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }), Encoding.UTF8, "application/json");
        }

        public void Dispose()
        {
        }
    }

    [CollectionDefinition("CodeWorkflowWorker collection")]
    public class CodeWorkflowWorkerCollection : ICollectionFixture<SignalRWorkflowWorkerFixture>
    {
    }
}