namespace DHI.Services.Jobs
{
    using System;

    /// <summary>
    /// Interface can be implemented by tasks that have dynamic timeout requirements.
    /// </summary>
    public interface IDynamicTimeoutTask
    {
        /// <summary>
        /// Duration after which the task will be timed out, and a cancellation request will be sent to the task.
        /// </summary>
        TimeSpan? WorkflowTimeout { get; set; }

        /// <summary>
        /// Period of time after the WorkflowTimeout has been reached, before the task is terminated.
        /// </summary>
        TimeSpan? TerminationGracePeriod { get; set; }
    }
}