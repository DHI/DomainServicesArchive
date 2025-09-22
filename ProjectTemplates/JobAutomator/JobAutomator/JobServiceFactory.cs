using System;
using DHI.Services.Jobs;
using DHI.Services.Jobs.Workflows;
using Microsoft.Extensions.Logging;

namespace JobAutomator;
public class JobServiceFactory : IJobServiceFactory
{
    private readonly Dictionary<string, JobService<CodeWorkflow, string>> _jobServices;
    private readonly ILogger _logger;

    public JobServiceFactory(Dictionary<string, JobService<CodeWorkflow, string>> jobServices, ILogger logger)
    {
        _jobServices = jobServices.ToDictionary(kvp => kvp.Key.ToLowerInvariant(), kvp => kvp.Value);
        _logger = logger;
    }

    public JobService<CodeWorkflow, string> GetJobService(string hostGroup)
    {
        if (string.IsNullOrWhiteSpace(hostGroup))
        {
            _logger.LogWarning("HostGroup was null or empty; defaulting to wf-jobs-Minion");
            return _jobServices.GetValueOrDefault("wf-jobs-minion");
        }

        var hostKey = hostGroup.ToLowerInvariant().Contains("minion") ? "wf-jobs-minion" : "wf-jobs-titan";

        if (_jobServices.TryGetValue(hostKey, out var jobService))
        {
            return jobService;
        }

        _logger.LogWarning("HostGroup '{HostGroup}' not recognized. Defaulting to wf-jobs-minion", hostGroup);
        return _jobServices["wf-jobs-minion"];
    }
}
