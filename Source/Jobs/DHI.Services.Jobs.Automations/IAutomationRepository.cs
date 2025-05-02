namespace DHI.Services.Jobs.Automations;

public interface IAutomationRepository<TTaskId> : IRepository<Automation<TTaskId>, string>,
    IDiscreteRepository<Automation<TTaskId>, string>,
    IUpdatableRepository<Automation<TTaskId>, string>,
    IGroupedRepository<Automation<TTaskId>>
{
}

public interface IAutomationRepository : IAutomationRepository<string>
{
}