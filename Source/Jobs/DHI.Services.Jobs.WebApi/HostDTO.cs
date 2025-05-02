namespace DHI.Services.Jobs.WebApi
{
    using System.ComponentModel.DataAnnotations;
    using Jobs;

    /// <summary>
    ///     Data transfer object for job host resource representation.
    /// </summary>
    public class HostDTO
    {
        /// <summary>
        ///     Gets or sets the identifier.
        /// </summary>
        [Required]
        [Key]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the group.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        ///     Gets or sets the priority.
        /// </summary>
        [Required]
        public int Priority { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the running jobs limit.
        /// </summary>
        [Required]
        public int RunningJobsLimit { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the type of the cloud instance handler.
        /// </summary>
        /// <value>The type of the cloud instance handler.</value>
        public string CloudInstanceHandlerType { get; set; }

        /// <summary>
        ///     Gets or sets the cloud instance parameters.
        /// </summary>
        public Parameters CloudInstanceParameters { get; set; }

        /// <summary>
        ///     Converts the DTO to a Host object.
        /// </summary>
        public Host ToHost()
        {
            var host = new Host(Id, Name, Group)
            {
                Priority = Priority,
                RunningJobsLimit = RunningJobsLimit,
                CloudInstanceHandlerType = CloudInstanceHandlerType
            };

            if (CloudInstanceParameters != null)
            {
                foreach (var parameter in CloudInstanceParameters)
                {
                    host.CloudInstanceParameters.Add(parameter.Key, parameter.Value);
                }
            }

            return host;
        }
    }
}