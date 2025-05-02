using DHI.Services.Jobs.Workflows;
using DHI.Services.Jobs.Workflows.Code;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DHI.Services.Jobs.WorkflowWorker
{
    /// <summary>
    /// Worker class allowing communication with a remote SignalR workflow host client.
    /// </summary>
    /// <remarks>
    ///     Workflow Host is a DHI proprietary workflow execution service implemented in DHI.Workflow.Service.* namespace.
    /// </remarks>
    public class SignalRWorkflowWorker : IRemoteWorker<Guid, string>
    {
        private readonly IHubContext<WorkerHub> _workerHubContext;
        private readonly AvailableCache _availableCache;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SignalRRemoteWorker class.
        /// </summary>
        /// <param name="workerHubContext">The SignalR Hub.</param>
        /// <param name="availableCache">The Host availability cache.</param>
        /// <param name="logger">The logger.</param>
        public SignalRWorkflowWorker(IHubContext<WorkerHub> workerHubContext, AvailableCache availableCache, ILogger logger)
        {
            _workerHubContext = workerHubContext;
            _availableCache = availableCache;
            _logger = logger;

        }

        public event EventHandler<EventArgs<Guid>> HostNotAvailable;
        public event EventHandler<EventArgs<Tuple<Guid, JobStatus, string>>> Executed;
        public event EventHandler<EventArgs<Tuple<Guid, string>>> Executing;
        public event EventHandler<EventArgs<Tuple<Guid, string>>> Cancelled;
        public event EventHandler<EventArgs<Guid>> Cancelling;
        public event EventHandler<EventArgs<Tuple<Guid, Progress>>> ProgressChanged;
        public event EventHandler<EventArgs<Tuple<Guid, string>>> Interrupted;

        /// <inheritdoc />
        public void Cancel(Guid jobId, string hostId = null)
        {
            _logger.LogInformation("Cancellation requested for Job {jobId} on Host {hostId}", jobId, hostId);
            if (string.IsNullOrEmpty(hostId))
            {
                Cancelled?.Invoke(this, new EventArgs<Tuple<Guid, string>>(new Tuple<Guid, string>(jobId, hostId)));
                return;
            }
            var clientProxy = _workerHubContext?.Clients?.User(hostId);

            if (clientProxy == null)
            {
                _logger.LogWarning("Cancellation request failed for Job {jobId} on Host {hostId}, Host is not connected.", jobId, hostId);
                HostNotAvailable?.Invoke(this, new EventArgs<Guid>(jobId));
                return;
            }

            clientProxy.SendAsync("OnCancelJob", jobId).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public void Timeout(Guid jobId, string hostId = null)
        {
            _logger.LogInformation("Timeout for Job {jobId} on Host {hostId}", jobId, hostId);
            if (string.IsNullOrEmpty(hostId))
            {
                Cancelled?.Invoke(this, new EventArgs<Tuple<Guid, string>>(new Tuple<Guid, string>(jobId, hostId)));
                return;
            }
            var clientProxy = _workerHubContext?.Clients?.User(hostId);

            if (clientProxy == null)
            {
                _logger.LogWarning("Timeout request failed for Job {jobId} on Host {hostId}, Host is not connected.", jobId, hostId);
                HostNotAvailable?.Invoke(this, new EventArgs<Guid>(jobId));
                return;
            }

            clientProxy.SendAsync("OnTimeoutJob", jobId).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public void Execute(Guid jobId, ITask<string> task, Dictionary<string, object> parameters, string hostId = null)
        {
            if (string.IsNullOrWhiteSpace(hostId))
            {
                throw new ArgumentNullException(nameof(hostId));
            }

            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            var clientProxy = _workerHubContext?.Clients?.User(hostId);

            if (clientProxy == null)
            {
                HostNotAvailable?.Invoke(this, new EventArgs<Guid>(jobId));
                return;
            }

            if (task is CodeWorkflow codeWorkflow)
            {
                var workflowDto = new CodeWorkflowDTO
                {
                    Definition = codeWorkflow.ToDefinition(),
                    HostId = hostId,
                    JobId = jobId,
                    Parameters = parameters
                };

                Task.Factory.StartNew(async () =>
                {
                    await clientProxy.SendAsync("OnRunJob", workflowDto, JsonSerializer.Serialize(codeWorkflow.ToDefinition()));
                    _logger.LogInformation("Host {host} sent {name} ({jobId})", hostId, task.Name, jobId);
                });                
            }
            else if (task is Workflow workflow)
            {
                if (string.IsNullOrWhiteSpace(workflow.Definition))
                {
                    throw new ArgumentException("Workflow should not be empty");
                }

                var workflowDTO = new WorkflowDto
                {
                    HostId = hostId,
                    JobId = jobId,
                    Parameters = parameters
                };

                Task.Factory.StartNew(async () =>
                {
                    await clientProxy.SendAsync("OnRunJob", workflowDTO, workflow.Definition);
                    _logger.LogInformation("Host {host} sent {name} ({jobId})", hostId, task.Name, jobId);
                });               
            }
            else
            {
                throw new ArgumentException("Task should be a CodeWorkflow or a Workflow");
            }
        }

        /// <inheritdoc />
        public bool IsHostAvailable(string hostId)
        {
            var stopWatch = new Stopwatch();

            var clientProxy = _workerHubContext?.Clients?.User(hostId);

            if (clientProxy == null)
            {
                _logger.LogWarning("Host {hostId} is not connected to the hub", hostId);
                return false;
            }

            clientProxy.SendAsync("OnAvailable").GetAwaiter().GetResult();

            stopWatch.Start();

            (bool Available, DateTime LastSeen) result;

            while (!_availableCache.TryRemove(hostId, out result) && stopWatch.ElapsedMilliseconds < 5000)
            {
                Thread.Sleep(20);
            }

            stopWatch.Stop();

            if (stopWatch.ElapsedMilliseconds >= 5000)
            {
                _logger.LogWarning("OnAvailable call to {host} timed out", hostId);
            }
            else
            {
                _logger.LogInformation("Host {host} responded {result}", hostId, result.Available);
            }
            return result.Available;
        }

        internal void OnHostNotAvailable(Guid jobId)
        {
            HostNotAvailable?.Invoke(this, new EventArgs<Guid>(jobId));
        }

        internal void OnExecuting(Guid jobId, string message)
        {
            Executing?.Invoke(this, new EventArgs<Tuple<Guid, string>>(new Tuple<Guid, string>(jobId, message)));
        }

        internal void OnExecuted(Guid jobId, JobStatus status, string message)
        {
            Executed?.Invoke(this, new EventArgs<Tuple<Guid, JobStatus, string>>(new Tuple<Guid, JobStatus, string>(jobId, status, message)));
        }

        internal void OnCancelled(Guid jobId, string message)
        {
            Cancelled?.Invoke(this, new EventArgs<Tuple<Guid, string>>(new Tuple<Guid, string>(jobId, message)));
        }
        internal void OnCancelling(Guid jobId)
        {
            Cancelling?.Invoke(this, new EventArgs<Guid>(jobId));
        }
        internal void OnInterrupted(Guid jobId, string message)
        {
        }

        internal void OnProgressChanged(Guid jobId, Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Tuple<Guid, Progress>>(new Tuple<Guid, Progress>(jobId, progress)));
        }

        public void Interrupt(Guid jobId, string recoveryInformation = "")
        {
            throw new NotImplementedException();
        }
    }
}
