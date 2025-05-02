namespace DHI.Services.Jobs
{
    using System;
    using System.Collections.Generic;

    public abstract class BaseWorker<TJobId, TTaskId> : IWorker<TJobId, TTaskId>
    {
        public event EventHandler<EventArgs<Tuple<TJobId, JobStatus, string>>> Executed;

        public event EventHandler<EventArgs<Tuple<TJobId, string>>> Starting;

        public event EventHandler<EventArgs<Tuple<TJobId, string>>> Executing;

        public event EventHandler<EventArgs<Tuple<TJobId, string>>> Cancelled;

        public event EventHandler<EventArgs<TJobId>> Cancelling;

        public event EventHandler<EventArgs<Tuple<TJobId, Progress>>> ProgressChanged;

        public abstract void Execute(TJobId jobId, ITask<TTaskId> taskId, Dictionary<string, object> parameters, string hostId = null);

        public abstract void Cancel(TJobId jobId, string hostId = null);

        public abstract void Timeout(TJobId jobId, string hostId = null);

        protected virtual void OnExecuted(TJobId jobId, JobStatus status, string message = "")
        {
            Executed?.Invoke(this, new EventArgs<Tuple<TJobId, JobStatus, string>>(new Tuple<TJobId, JobStatus, string>(jobId, status, message)));
        }

        protected virtual void OnStarting(Tuple<TJobId, string> jobInfo)
        {
            Starting?.Invoke(this, new EventArgs<Tuple<TJobId, string>>(jobInfo));
        }

        protected virtual void OnExecuting(Tuple<TJobId, string> jobInfo)
        {
            Executing?.Invoke(this, new EventArgs<Tuple<TJobId, string>>(jobInfo));
        }

        protected virtual void OnCancelled(TJobId jobId, string message = "")
        {
            Cancelled?.Invoke(this, new EventArgs<Tuple<TJobId, string>>(new Tuple<TJobId, string>(jobId, message)));
        }

        protected virtual void OnCancelling(TJobId jobId)
        {
            Cancelling?.Invoke(this, new EventArgs<TJobId>(jobId));
        }

        protected virtual void OnProgressChanged(TJobId jobId, Progress progress)
        {
            ProgressChanged?.Invoke(this, new EventArgs<Tuple<TJobId, Progress>>(new Tuple<TJobId, Progress>(jobId, progress)));
        }
    }
}