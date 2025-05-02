using DHI.Services.Jobs.Workflows;
using System.Threading.Tasks;

namespace DHI.Services.Jobs.WorkflowWorker
{
    /// <summary>
    /// Interface describing communications with workflow client.
    /// </summary>
    public interface IWorkerClient
    {
        /// <summary>
        /// Instructs the recipient SignalR client to execute the workflow.
        /// </summary>
        /// <param name="workflowDto">WorkflowDto object including job meta data.</param>
        /// <param name="workflowDefinition">The serialized workflow definition.</param>
        void OnRunJob(WorkflowDto workflowDto, string workflowDefinition);

        /// <summary>
        /// Instructs the reciepient SignalR client to update the availability cache.
        /// </summary>
        /// <returns></returns>
        Task OnAvailable();

        /// <summary>
        /// Instructs the reciepient SignalR client to update the report cache.
        /// </summary>
        void OnReport();
    }
}
