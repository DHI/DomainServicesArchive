namespace DHI.Services.Jobs.Automations;

using Logging;
using Microsoft.Extensions.Logging;

public class AutomationService<TTaskId> : BaseGroupedUpdatableDiscreteService<Automation<TTaskId>, string>
{
    public AutomationService(IGroupedRepository<Automation<TTaskId>> repository) : base(repository)
    {
    }

    public AutomationService(IGroupedRepository<Automation<TTaskId>> repository, ILogger logger) : base(repository, logger)
    {
    }
}

public class AutomationService : AutomationService<string>
{
    public AutomationService(IGroupedRepository<Automation<string>> repository) : base(repository)
    {
    }

    public AutomationService(IGroupedRepository<Automation<string>> repository, ILogger logger) : base(repository, logger)
    {
    }
}