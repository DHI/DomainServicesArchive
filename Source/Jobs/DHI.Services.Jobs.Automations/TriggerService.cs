namespace DHI.Services.Jobs.Automations;

using Microsoft.Extensions.Logging;

public class TriggerService<TTaskId> : BaseDiscreteService<TriggerParameters<TTaskId>, TTaskId>
{
    public TriggerService(IDiscreteRepository<TriggerParameters<TTaskId>, TTaskId> repository) : base(repository)
    {
    }

    public TriggerService(IDiscreteRepository<TriggerParameters<TTaskId>, TTaskId> repository, ILogger logger) : base(repository, logger)
    {
    }
}

public class TriggerService : TriggerService<string>
{
    public TriggerService(IDiscreteRepository<TriggerParameters<string>, string> repository) : base(repository)
    {
    }

    public TriggerService(IDiscreteRepository<TriggerParameters<string>, string> repository, ILogger logger) : base(repository, logger)
    {
    }
}