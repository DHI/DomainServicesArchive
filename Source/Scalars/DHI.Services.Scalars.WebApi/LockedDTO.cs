namespace DHI.Services.Scalars.WebApi
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     Data transfer object for scalar locked resource representation
    /// </summary>
    public class LockedDTO
    {
        /// <summary>
        ///     Gets or sets the locked value.
        /// </summary>
        [Required]
        public bool Locked { get; set; }
    }
}