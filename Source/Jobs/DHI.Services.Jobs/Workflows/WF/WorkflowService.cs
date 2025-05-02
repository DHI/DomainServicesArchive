namespace DHI.Services.Jobs.Workflows
{
    using System;
    using Jobs;

    [Obsolete("Use CodeWorkflowService instead. This type will eventually be removed.")]
    public class WorkflowService : BaseUpdatableDiscreteService<Workflow, string>, ITaskService<Workflow, string>
    {
        public WorkflowService(IWorkflowRepository repository)
            : base(repository)
        {
        }

        public static Type[] GetRepositoryTypes(string path = null)
        {
            return Service.GetProviderTypes<IWorkflowRepository>(path);
        }
    }
}