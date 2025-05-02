namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using Jobs;

    internal class FakeWorker : BaseWorker<Guid, string>, IRemoteWorker<Guid, string>
    {
        private readonly bool _isHostAvailable;
        private readonly Dictionary<Guid, BackgroundWorker> _currentJobs;

        public FakeWorker() : this(true)
        {
        }

        public FakeWorker(bool isHostAvailable)
        {
            _isHostAvailable = isHostAvailable;
            _currentJobs = new Dictionary<Guid, BackgroundWorker>();
        }

#pragma warning disable 67
        public event EventHandler<EventArgs<Guid>> HostNotAvailable;
#pragma warning restore 67

        public override void Execute(Guid jobId, ITask<string> task, Dictionary<string, object> parameters, string hostId = null)
        {
            var backgroundWorker = new BackgroundWorker {WorkerReportsProgress = true, WorkerSupportsCancellation = true};
            _currentJobs.Add(jobId, backgroundWorker);
            backgroundWorker.DoWork += _BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += _BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += _BackgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync(new Tuple<Guid, string>(jobId, hostId));
        }

        public bool IsHostAvailable(string hostId)
        {
            return _isHostAvailable;
        }

        public override void Cancel(Guid jobId, string hostId = null)
        {
            OnCancelling(jobId);
            _currentJobs.TryGetValue(jobId, out var worker);
            worker?.CancelAsync();
        }

        public override void Timeout(Guid jobId, string hostId = null)
        {
            _currentJobs.TryGetValue(jobId, out var worker);
            worker?.CancelAsync();
        }

        private void _BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var jobInfo = (Tuple<Guid, string>)e.Argument;
            var jobId = jobInfo.Item1;
            OnExecuting(jobInfo);
            for (int i = 1; i <= 100; i++)
            {
                Thread.Sleep(10);
                if (worker.CancellationPending)
                {
                    e.Result = (jobId, "Cancelled");
                    return;
                }

                worker.ReportProgress(i, jobId);
            }

            e.Result = (jobId, "OK");
        }

        private void _BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnProgressChanged((Guid)e.UserState, new Progress(e.ProgressPercentage));
        }

        private void _BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var (jobId, result) = (ValueTuple<Guid , string>)e.Result;
            if (result == "Cancelled")
            {
                OnCancelled(jobId);
            }
            else
            {
                OnExecuted(jobId, JobStatus.Completed);
            }
            
            _currentJobs.Remove(jobId);
        }
    }
}