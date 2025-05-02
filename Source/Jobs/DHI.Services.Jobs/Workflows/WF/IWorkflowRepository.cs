namespace DHI.Services.Jobs.Workflows
{
    using System;

    [Obsolete("Use ICodeWorkflowRepository instead. This type will eventually be removed.")]
    public interface IWorkflowRepository : IRepository<Workflow, string>, IDiscreteRepository<Workflow, string>, IUpdatableRepository<Workflow, string>
    {
    }
}