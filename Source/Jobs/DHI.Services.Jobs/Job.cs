namespace DHI.Services.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    [Serializable]
    public class Job<TJobId, TTaskId> : BaseEntity<TJobId>
    {
        public Job(TJobId id, TTaskId taskId, string accountId = null)
            : base(id)
        {
            if (taskId == null)
            {
                throw new ArgumentNullException(nameof(taskId));
            }

            AccountId = accountId;
            TaskId = taskId;
            Requested = DateTime.UtcNow;
            Status = JobStatus.Pending;
        }

        [JsonInclude]
        public string AccountId { get; }

        public DateTime? Finished { get; set; }

        public string HostId { get; set; }

        public string HostGroup { get; set; }

        public Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        public int? Progress { get; set; }

        public DateTime Requested { get; set; }

        public DateTime? Rejected { get; set; }

        public DateTime? Starting { get; set; }

        public DateTime? Started { get; set; }

        public JobStatus Status { get; set; }

        public string StatusMessage { get; set; }

        public string Tag { get; set; }

        [JsonInclude]
        public TTaskId TaskId { get; }

        public int Priority { get; set; } = 1;

        public DateTime? Heartbeat { get; set; }

        public bool ShouldSerializeParameters()
        {
            return Parameters.Count > 0;
        }
    }

    [Serializable]
    public class Job : Job<Guid, string>
    {
        public Job(Guid id, string taskId, string accountId = null)
            : base(id, taskId, accountId)
        {
        }
    }
}