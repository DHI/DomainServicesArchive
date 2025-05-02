namespace DHI.Services.Jobs.Workflows
{
    using System;

    public class TerminateWorkflowException : Exception
    {
        public TerminateWorkflowException()
        {
        }

        public TerminateWorkflowException(string message)
            : base(message)
        {
        }

        public TerminateWorkflowException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}