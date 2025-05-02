namespace DHI.Services.Jobs
{
    using System;
    using System.Collections.Generic;

    public interface IWorker<TJobId, in TTaskId>
    {
        event EventHandler<EventArgs<Tuple<TJobId, JobStatus, string>>> Executed;

        event EventHandler<EventArgs<Tuple<TJobId, string>>> Executing;

        event EventHandler<EventArgs<Tuple<TJobId, string>>> Cancelled;

        event EventHandler<EventArgs<TJobId>> Cancelling;

        event EventHandler<EventArgs<Tuple<TJobId, Progress>>> ProgressChanged;

        void Execute(TJobId jobId, ITask<TTaskId> taskId, Dictionary<string, object> parameters, string hostId = null);

        void Cancel(TJobId jobId, string hostId = null);

        void Timeout(TJobId jobId, string hostId = null);
    }
}