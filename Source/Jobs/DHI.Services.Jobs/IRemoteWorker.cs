namespace DHI.Services.Jobs
{
    using System;

    public interface IRemoteWorker<TJobId, TTaskId> : IWorker<TJobId, TTaskId>
    {
        event EventHandler<EventArgs<TJobId>> HostNotAvailable;

        bool IsHostAvailable(string hostId);
    }
}