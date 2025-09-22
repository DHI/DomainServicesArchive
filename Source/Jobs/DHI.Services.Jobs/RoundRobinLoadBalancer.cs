namespace DHI.Services.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Workflows;
    using Microsoft.Extensions.Logging;
	using System.Threading.Tasks;
	using System.Threading;
	using System.Collections.Concurrent;	

	/// <summary>
	///     Default load balancer.
	/// </summary>
	/// <typeparam name="TTaskId">The type of the task identifier.</typeparam>
	/// <typeparam name="TTask">The type of the task</typeparam>
	public class RoundRobinLoadBalancer<TTask, TTaskId> : ILoadBalancer where TTask : ITask<TTaskId>
    {
        private readonly IWorker<Guid, TTaskId> _worker;
        private readonly IHostService _hostService;
        private readonly JobService<TTask, TTaskId> _jobService;
        private readonly ILogger _logger;
        private readonly string _defaultHostGroup;
        private readonly string _jobWorkerId;
		private readonly TimeSpan _hostResponseThreshold;
		private readonly ConcurrentDictionary<string, DateTime> _requestCache = new ConcurrentDictionary<string, DateTime>();

		/// <summary>
		///     Initializes a new instance of the <see cref="RoundRobinLoadBalancer{TTask, TTaskId}" /> class.
		/// </summary>
		/// <param name="jobWorkerId">The identifier of the calling job worker instance. Used for logging.</param>
		/// <param name="worker">The worker.</param>
		/// <param name="jobService">The job service.</param>
		/// <param name="hostService">the host service.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="defaultHostGroup">A default host group. Only relevant if the host repository is grouped.</param>
		/// <param name="hostResponseThreshold">The timeout for requesting availablilty from hosts. Default 2 seconds</param>
		public RoundRobinLoadBalancer(string jobWorkerId, IWorker<Guid, TTaskId> worker, JobService<TTask, TTaskId> jobService, IHostService hostService, ILogger logger = null, string defaultHostGroup = null, TimeSpan? hostResponseThreshold = null)
		{
			_jobWorkerId = jobWorkerId;
			_worker = worker ?? throw new ArgumentNullException(nameof(worker));
			_hostService = hostService ?? throw new ArgumentNullException(nameof(hostService));
			_jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
			_logger = logger;
			_defaultHostGroup = defaultHostGroup;
			_hostResponseThreshold = hostResponseThreshold.HasValue ? hostResponseThreshold.Value : TimeSpan.FromSeconds(2);
			_logger?.LogInformation("RoundRobinLoadBalancer initialized with job worker ID '{JobWorkerId}', default host group '{DefaultHostGroup}', and host response threshold of {HostResponseThreshold} seconds.", _jobWorkerId, _defaultHostGroup, _hostResponseThreshold.TotalSeconds);
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

			if (TrySelectHost(jobId, hosts, worker, out var host))
			{
				return host.ToMaybe();
			}

			_logger?.LogWarning("Found no available host.");
            return Maybe.Empty<Host>();
        }

		public bool TrySelectHost(Guid jobIdToExecute, IEnumerable<Host> hosts, IRemoteWorker<Guid, string> worker, out Host host)
		{
			var availableHosts = FindAllAvailableHostsAsync(hosts, worker).GetAwaiter().GetResult();

			var sortedAvailableHosts = availableHosts.Select(c => (Host: c, LastAssigned: _requestCache.TryGetValue(c.Id, out var assigned) ? assigned : DateTime.MinValue))
				.OrderBy(c => c.Host.Priority)
				.ThenBy(c => c.LastAssigned)
				.Select(c => c.Host)
				.ToList();

			_logger?.LogDebug("First 5 hosts {hosts} available hosts.", string.Join(",", sortedAvailableHosts.Take(5).Select(h => h.Id).ToArray()));

			var allJobsNotFinished = _jobService.GetJobsNotFinished();

			foreach (var candidate in sortedAvailableHosts)
			{
				var count = allJobsNotFinished.Count(c => c.HostId == candidate.Id);

				if (count >= candidate.RunningJobsLimit)
				{
					_logger?.LogInformation("Host '{HostId}' all {RunningJobsLimit} slots used. Skipping.", candidate.Id, candidate.RunningJobsLimit);
					continue;
				}

				_logger?.LogInformation("Host '{HostId}' {Count} / {RunningJobsLimit} slots used. It has spare capacity.", candidate.Id, count, candidate.RunningJobsLimit);

				_logger?.LogInformation("Host '{HostId}' is available and associated to job '{JobId}'.", candidate.Id, jobIdToExecute);

				if (!_requestCache.TryAdd(candidate.Id, DateTime.UtcNow))
				{
					_requestCache[candidate.Id] = DateTime.UtcNow;
				}

				host = candidate;
				return true;
			}

			var unavailableCloudHosts = hosts.Where(h => h.IsCloudInstance()).Except(availableHosts).ToList();

			var sortedUnavailableCloudHosts = unavailableCloudHosts.Select(c => (Host: c, LastAssigned: _requestCache.TryGetValue(c.Id, out var assigned) ? assigned : DateTime.MinValue)).OrderBy(c => c.LastAssigned).Select(c => c.Host).ToList();

			foreach (var candidate in sortedUnavailableCloudHosts)
			{
				var status = candidate.CloudInstanceHandler.GetStatus();
				switch (status)
				{
					case CloudInstanceStatus.Running:
						_logger?.LogWarning("Host '{HostId}' status is '{Status}', so nothing I can do.", candidate.Id, status);
						continue;
					case CloudInstanceStatus.Stopped:
						_logger?.LogInformation("Host '{HostId}' status is '{Status}'.", candidate.Id, status);
						_logger?.LogInformation("Starting cloud instance for host '{HostId}'. Be patient!", candidate.Id);
						candidate.CloudInstanceHandler.Start();
						_logger?.LogInformation("Associated host '{HostId}' to job '{JobId}'.", candidate.Id, jobIdToExecute);

						if (!_requestCache.TryAdd(candidate.Id, DateTime.UtcNow))
						{
							_requestCache[candidate.Id] = DateTime.UtcNow;
						}

						host = candidate;
						return true;
					case CloudInstanceStatus.Starting:
						_logger?.LogInformation("Host '{HostId}' status is '{Status}'. Be patient!", candidate.Id, status);
						_logger?.LogInformation("Associated host '{HostId}' to job '{JobId}'.", candidate.Id, jobIdToExecute);

						if (!_requestCache.TryAdd(candidate.Id, DateTime.UtcNow))
						{
							_requestCache[candidate.Id] = DateTime.UtcNow;
						}

						host = candidate;
						return true;
					case CloudInstanceStatus.Stopping:
						_logger?.LogInformation("Host '{HostId}' status is '{Status}'.", candidate.Id, status);
						continue;
					case CloudInstanceStatus.Unknown:
						_logger?.LogInformation("Host '{HostId}' status is '{Status}'.", candidate.Id, status);
						continue;
					default:
						throw new NotSupportedException($"'{status}' is not supported.");
				}
			}

			_logger?.LogWarning("Found no available host.");
			host = null;
			return false;
		}

		protected async Task<List<Host>> FindAllAvailableHostsAsync(IEnumerable<Host> hosts, IRemoteWorker<Guid, string> worker)
		{
			using var cts = new CancellationTokenSource(_hostResponseThreshold);
			var result = new ConcurrentBag<Host>();

			var tasks = hosts.Select(host => Task.Run(async () =>
			{
				var delay = Task.Delay(_hostResponseThreshold, cts.Token);
				var task = Task.Run(() =>
				{
					if (worker.IsHostAvailable(host.Id) && !cts.IsCancellationRequested)
					{
						result.Add(host);
					}
				});

				return await Task.WhenAny(task, delay); //first of either the IsAvailable or delay 
			})).ToList();

			await Task.WhenAll(tasks);

			_logger?.LogDebug("Found {Count} available hosts.", result.Count);

			return result
				.OrderBy(r => r.Name) //order for consistency in tests
				.ToList();
		}
	}

    public class RoundRobinLoadBalancer : RoundRobinLoadBalancer<CodeWorkflow, string>
    {
        public RoundRobinLoadBalancer(string jobWorkerId,
            IWorker<Guid, string> worker,
            JobService<CodeWorkflow, string> jobService,
            IHostService hostService,
            ILogger logger = null,
            string defaultHostGroup = null) : base(jobWorkerId, worker, jobService, hostService, logger, defaultHostGroup)
        {
        }
    }
}