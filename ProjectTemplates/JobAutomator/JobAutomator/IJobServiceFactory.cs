using DHI.Services.Jobs;
using DHI.Services.Jobs.Workflows;

namespace JobAutomator;
public interface IJobServiceFactory
{
    JobService<CodeWorkflow, string> GetJobService(string hostGroup);
}