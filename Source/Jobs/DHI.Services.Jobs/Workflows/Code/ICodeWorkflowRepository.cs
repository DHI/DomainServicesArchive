namespace DHI.Services.Jobs.Workflows
{
    public interface ICodeWorkflowRepository : IRepository<CodeWorkflow, string>,
        IDiscreteRepository<CodeWorkflow, string>,
        IImmutableRepository<CodeWorkflow, string>
    {
    }
}