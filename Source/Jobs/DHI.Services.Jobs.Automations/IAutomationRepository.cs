namespace DHI.Services.Jobs.Automations;

using System;

public interface IAutomationRepository<TTaskId> : IRepository<Automation<TTaskId>, string>,
    IDiscreteRepository<Automation<TTaskId>, string>,
    IUpdatableRepository<Automation<TTaskId>, string>,
    IGroupedRepository<Automation<TTaskId>>
{
    DateTime GetVersionTimestamp();
    DateTime TouchVersion();
}

public interface IAutomationRepository : IAutomationRepository<string>
{
}