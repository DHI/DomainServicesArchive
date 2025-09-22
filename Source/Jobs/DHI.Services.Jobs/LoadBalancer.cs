namespace DHI.Services.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Workflows;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    ///     Default load balancer.
    /// </summary>
    /// <typeparam name="TTaskId">The type of the task identifier.</typeparam>
    /// <typeparam name="TTask">The type of the task</typeparam>
    public class LoadBalancer<TTask, TTaskId> : ILoadBalancer where TTask : ITask<TTaskId>
    {
        private readonly IWorker<Guid, TTaskId> _worker;
        private readonly IHostService _hostService;
        private readonly JobService<TTask, TTaskId> _jobService;
        private readonly ILogger _logger;
        private readonly string _defaultHostGroup;
        private readonly string _jobWorkerId;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoadBalancer{TTask, TTaskId}" /> class.
        /// </summary>
        /// <param name="jobWorkerId">The identifier of the calling job worker instance. Used for logging.</param>
        /// <param name="worker">The worker.</param>
        /// <param name="jobService">The job service.</param>
        /// <param name="hostService">the host service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="defaultHostGroup">A default host group. Only relevant if the host repository is grouped.</param>
        public LoadBalancer(string jobWorkerId, IWorker<Guid, TTaskId> worker, JobService<TTask, TTaskId> jobService, IHostService hostService, ILogger logger = null, string defaultHostGroup = null)
        {
            _jobWorkerId = jobWorkerId;
            _worker = worker ?? throw new ArgumentNullException(nameof(worker));
            _hostService = hostService ?? throw new ArgumentNullException(nameof(hostService));
            _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
            _logger = logger;
            _defaultHostGroup = defaultHostGroup;
        }

        /// <summary>
        ///     Gets a job host - if any is available.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="hostGroup">A host group. Only relevant if the host repository is grouped. If not given, the default host group will be used.</param>
        /// <returns>Maybe a host.</returns>
        public Maybe<Host> GetHost(Guid jobId, string hostGroup = null)
        {
            if (_hostService is IGroupedHostService && hostGroup is null)
            {
                if (_defaultHostGroup is null)
                {
                    _logger?.LogError("The host service is configured as a grouped host service, but no host group is provided in the job execution request and no default host group is defined. You can define a default host group through the constructor of a custom {Name}", nameof(ILoadBalancer));
                    throw new Exception($"The host service is configured as a grouped host service, but no host group is provided in the job execution request and no default host group is defined. You can define a default host group through the constructor of a custom {nameof(ILoadBalancer)}");
                }

                hostGroup = _defaultHostGroup;
            }

            if (!(_hostService is IGroupedHostService) && !string.IsNullOrEmpty(hostGroup))
            {
                _logger?.LogError("A host group is provided ({HostGroup}) but the service is not grouped.", hostGroup);
                throw new ArgumentException($"A host group is provided ({hostGroup}) but the service is not grouped.", nameof(hostGroup));
            }

            if (_worker is not IRemoteWorker<Guid, string> worker)
            {
                _logger?.LogWarning("The defined worker type is not a remote worker and will always run locally.");
                return Maybe.Empty<Host>();
            }

            IEnumerable<Host> hosts;
            if (hostGroup is null)
            {
                _logger?.LogInformation("Getting an available host for pending job '{JobId}'.", jobId);
                hosts = _hostService.GetAll().OrderBy(host => host.Priority).ToArray();
            }
            else
            {
                _logger?.LogInformation("Getting an available host in host group '{HostGroup}' for pending job '{JobId}'.", hostGroup, jobId);
                hosts = ((IGroupedHostService)_hostService).GetByGroup(hostGroup).OrderBy(host => host.Priority).ToArray();
            }

            _logger?.LogInformation("Available hosts {Hosts}", string.Join(", ", hosts.Select(host => $"'{host.Id}'")));
            foreach (var host in hosts)
            {
                var count = _jobService.GetJobsNotFinished(host.Id).Count();

                if (count >= host.RunningJobsLimit)
                {
                    _logger?.LogInformation("Host '{HostId}' all {RunningJobsLimit} slots used. Skipping.", host.Id, host.RunningJobsLimit);
                    continue;
                }

                _logger?.LogInformation("Host '{HostId}' {Count} / {RunningJobsLimit} slots used. It has spare capacity.", host.Id, count, host.RunningJobsLimit);

                if (worker.IsHostAvailable(host.Id))
                {
                    _logger?.LogInformation("Host '{HostId}' is available and associated to job '{JobId}'.", host.Id, jobId);
                    return host.ToMaybe();
                }

                _logger?.LogInformation("Host '{HostId}' is not available.", host.Id);

                if (!host.IsCloudInstance())
                {
                    continue;
                }

                var status = host.CloudInstanceHandler.GetStatus();
                switch (status)
                {
                    case CloudInstanceStatus.Running:
                        _logger?.LogWarning("Host '{HostId}' status is '{Status}', so nothing I can do.", host.Id, status);
                        continue;
                    case CloudInstanceStatus.Stopped:
                        _logger?.LogInformation("Host '{HostId}' status is '{Status}'.", host.Id, status);
                        _logger?.LogInformation("Starting cloud instance for host '{HostId}'. Be patient!", host.Id);
                        host.CloudInstanceHandler.Start();
                        _logger?.LogInformation("Associated host '{HostId}' to job '{JobId}'.", host.Id, jobId);
                        return host.ToMaybe();
                    case CloudInstanceStatus.Starting:
                        _logger?.LogInformation("Host '{HostId}' status is '{Status}'. Be patient!", host.Id, status);
                        _logger?.LogInformation("Associated host '{HostId}' to job '{JobId}'.", host.Id, jobId);
                        return host.ToMaybe();
                    case CloudInstanceStatus.Stopping:
                        _logger?.LogInformation("Host '{HostId}' status is '{Status}'.", host.Id, status);
                        continue;
                    case CloudInstanceStatus.Unknown:
                        _logger?.LogInformation("Host '{HostId}' status is '{Status}'.", host.Id, status);
                        continue;
                    default:
                        throw new NotSupportedException($"'{status}' is not supported.");
                }
            }

            _logger?.LogWarning("Found no available host.");
            return Maybe.Empty<Host>();
        }
    }

    public class LoadBalancer : LoadBalancer<CodeWorkflow, string>
    {
        public LoadBalancer(string jobWorkerId,
            IWorker<Guid, string> worker,
            JobService<CodeWorkflow, string> jobService,
            IHostService hostService,
            ILogger logger = null,
            string defaultHostGroup = null) : base(jobWorkerId, worker, jobService, hostService, logger, defaultHostGroup)
        {
        }
    }
}