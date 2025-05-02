namespace DHI.Services.Jobs.Web
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     Data transfer object for a job resource representation.
    /// </summary>
    public class JobDTO
    {
        /// <summary>
        ///     Gets the parameters.
        /// </summary>
        public virtual Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        /// <summary>
        ///     Gets or sets the tag.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        ///     Gets or sets the host group.
        /// </summary>
        public string HostGroup { get; set; }

        /// <summary>
        ///     Gets or sets the priority.
        /// </summary>
        public int Priority { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the task identifier.
        /// </summary>
        [Required]
        [Key]
        public string TaskId { get; set; }
    }
}