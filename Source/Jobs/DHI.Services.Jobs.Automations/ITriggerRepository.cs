namespace DHI.Services.Jobs.Automations;

public interface ITriggerRepository<TTaskId> : IRepository<TriggerParameters<TTaskId>, TTaskId>,
                                               IDiscreteRepository<TriggerParameters<TTaskId>, TTaskId>
{
}

public interface ITriggerRepository : ITriggerRepository<string>
{
}