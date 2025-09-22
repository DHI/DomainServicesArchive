namespace DHI.Services.Jobs.Automations;

using DHI.Services.Scalars;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

public class AutomationService<TTaskId> : BaseGroupedUpdatableDiscreteService<Automation<TTaskId>, string>
{
    private readonly IAutomationRepository<TTaskId> _automationRepositoryOrNull;
    private readonly IScalarRepository<string, int> _scalarRepository;
    private readonly IJobRepository<Guid, string> _jobRepository;

    public AutomationService(IGroupedRepository<Automation<TTaskId>> repository, IScalarRepository<string, int> scalarRepository, IJobRepository<Guid, string> jobRepository)
        : base(repository)
    {
        _automationRepositoryOrNull = repository as IAutomationRepository<TTaskId>;
        _scalarRepository = scalarRepository;
        _jobRepository = jobRepository;
    }

    public AutomationService(IGroupedRepository<Automation<TTaskId>> repository, IScalarRepository<string, int> scalarRepository, IJobRepository<Guid, string> jobRepository, ILogger logger)
        : base(repository, logger)
    {
        _automationRepositoryOrNull = repository as IAutomationRepository<TTaskId>;
        _scalarRepository = scalarRepository;
        _jobRepository = jobRepository;
    }

    public DateTime? GetVersionTimestamp()
    {
        return _automationRepositoryOrNull?.GetVersionTimestamp();
    }

    public DateTime? TouchVersion()
    {
        return _automationRepositoryOrNull?.TouchVersion();
    }

    public override IEnumerable<Automation<TTaskId>> GetAll(System.Security.Claims.ClaimsPrincipal user = null)
    {
        var automations = base.GetAll(user).ToList();
        var scalars = _scalarRepository.GetAll(user);
        var scalarDict = scalars.ToDictionary(s => s.Id);

        foreach (var automation in automations)
        {
            EnrichAutomationWithScalarData(automation, scalarDict, user);
        }

        return automations;
    }

    public override Automation<TTaskId> Get(string id, System.Security.Claims.ClaimsPrincipal user = null)
    {
        var automation = base.Get(id, user);
        var scalars = _scalarRepository.GetAll(user);
        var scalarDict = scalars.ToDictionary(s => s.Id);

        EnrichAutomationWithScalarData(automation, scalarDict, user);

        return automation;
    }

    private void EnrichAutomationWithScalarData(
        Automation<TTaskId> automation,
        Dictionary<string, Scalar<string, int>> scalarDict,
        System.Security.Claims.ClaimsPrincipal user)
    {
        if (automation == null || string.IsNullOrWhiteSpace(automation.HostGroup))
            return;

        var resolver = new AutomationScalarResolver(scalarDict.Values);

        if (resolver.TryResolveIsMet(automation.Id, automation.HostGroup, out bool isMet))
        {
            automation.IsMet = isMet;
        }

        if (resolver.TryResolveLastJobId(automation.Id, automation.HostGroup, out Guid jobId))
        {
            try
            {
                var jobResult = _jobRepository.Get(jobId, user);
                if (jobResult.HasValue)
                {
                    automation.LastJob = jobResult.Value;
                }
            }
            catch
            {
                // Leave it nullable
            }
        }
    }
}

public class AutomationService : AutomationService<string>
{
    public AutomationService(IGroupedRepository<Automation<string>> repository, IScalarRepository<string, int> scalarRepository, IJobRepository<Guid, string> jobRepository) : base(repository, scalarRepository, jobRepository)
    {
    }

    public AutomationService(IGroupedRepository<Automation<string>> repository, IScalarRepository<string, int> scalarRepository, IJobRepository<Guid, string> jobRepository, ILogger logger) : base(repository, scalarRepository, jobRepository, logger)
    {
    }
}