using DHI.Services.Jobs;
using DHI.Workflow.Actions.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace DHI.Workflow.Engine.Example
{
    public class MyFirstCodeWorkflow : BaseCodeWorkflow, IDynamicTimoutTask
    {
        public MyFirstCodeWorkflow(ILogger logger, CancellationToken cancellationToken) : base(logger, cancellationToken)
        {
        }

        public TimeSpan? WorkflowTimeout { get; set; }
        public TimeSpan? TerminationGracePeriod { get; set; }

        public override void Run()
        {
            Logger.LogInformation($"Workflow running");
            Logger.LogInformation($"Hello world");
            Thread.Sleep(45000);
            Logger.LogInformation("Workflow exited");
        }
    }
}
