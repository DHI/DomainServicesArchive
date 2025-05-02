namespace DHI.Services.Jobs.Automations;

using System;

internal interface IJobCompletedTriggerParameters
{
    public Type RepositoryType { get; }

    public string ConnectionString { get; }

    public string TaskId { get; }
}