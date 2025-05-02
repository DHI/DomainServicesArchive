namespace DHI.Services.Jobs.Workflows
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class WorkflowNameAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WorkflowNameAttribute"/> class.
        /// </summary>
        /// <param name="workflowName">Name of the workflow.</param>
        public WorkflowNameAttribute(string workflowName)
        {
            Guard.Against.NullOrEmpty(workflowName, nameof(workflowName));
            WorkflowName = workflowName;
        }

        public string WorkflowName { get; }
    }
}