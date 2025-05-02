namespace DHI.Services.Jobs.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     Data transfer object for a job update resource representation.
    /// </summary>
    public class JobUpdateDTO
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="JobUpdateDTO" /> class.
        /// </summary>
        /// <param name="id">The job identifier.</param>
        /// <param name="taskId">The task identifier.</param>
        /// <param name="requested">The time requested.</param>
        /// <param name="status">The job status.</param>
        public JobUpdateDTO(Guid id, string taskId, DateTime requested, JobStatus status)
        {
            Id = id;
            TaskId = taskId;
            Requested = requested;
            Status = status;
        }

        /// <summary>
        ///     Gets the job identifier.
        /// </summary>
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets or sets the task identifier.
        /// </summary>
        [Required]
        public string TaskId { get; set; }

        /// <summary>
        ///     Gets or sets the account identifier.
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        ///     Gets or sets the time finished.
        /// </summary>
        public DateTime? Finished { get; set; }

        /// <summary>
        ///     Gets or sets the host identifier.
        /// </summary>
        public string HostId { get; set; }

        /// <summary>
        ///     Gets or sets the host group.
        /// </summary>
        public string HostGroup { get; set; }

        /// <summary>
        ///     Gets or sets the parameters.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }

        /// <summary>
        ///     Gets or sets the progress.
        /// </summary>
        /// <value>The progress.</value>
        public int? Progress { get; set; }

        /// <summary>
        ///     Gets the time requested.
        /// </summary>
        [Required]
        public DateTime Requested { get; set; }

        /// <summary>
        ///     Gets or sets the time rejected.
        /// </summary>
        public DateTime? Rejected { get; set; }

        /// <summary>
        ///     Gets or sets the time started.
        /// </summary>
        public DateTime? Started { get; set; }

        /// <summary>
        ///     Gets or sets the time starting.
        /// </summary>
        public DateTime? Starting { get; set; }

        /// <summary>
        ///     Gets or sets the heartbeat time.
        /// </summary>
        public DateTime? Heartbeat { get; set; }

        /// <summary>
        ///     Gets the job status.
        /// </summary>
        [Required]
        public JobStatus Status { get; set; }

        /// <summary>
        ///     Gets or sets the status message.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        ///     Gets or sets the tag.
        /// </summary>
        public string Tag { get; set; }

        public int Priority { get; set; }

        /// <summary>
        ///     Gets or sets the metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        ///     Converts the DTO to a job object.
        /// </summary>
        public Job ToJob()
        {
            var job = new Job(Id, TaskId, AccountId)
            {
                Status = Status,
                Requested = Requested,
                Rejected = Rejected,
                Started = Started,
                Starting = Starting,
                Finished = Finished,
                HostGroup = HostGroup,
                HostId = HostId,
                Priority = Priority,
                Progress = Progress,
                Heartbeat = Heartbeat,
                StatusMessage = StatusMessage,
                Tag = Tag
            };

            if (Parameters != null && Parameters.Count > 0)
            {
                foreach (var pair in Parameters)
                {
                    job.Parameters.Add(pair.Key, pair.Value);
                }
            }

            if (Metadata != null && Metadata.Count > 0)
            {
                foreach (var pair in Metadata)
                {
                    job.Metadata.Add(pair.Key, pair.Value);
                }
            }

            return job;
        }
    }
}