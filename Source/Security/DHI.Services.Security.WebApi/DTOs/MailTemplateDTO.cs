namespace DHI.Services.Security.WebApi.DTOs
{
    using System.ComponentModel.DataAnnotations;
    using Mails;

    /// <summary>
    ///     Data transfer object for mail template resource representation.
    /// </summary>
    public class MailTemplateDTO
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
        ///     Gets or sets the subject.
        /// </summary>
        [Required]
        public string Subject { get; set; }

        /// <summary>
        ///     Gets or sets the body.
        /// </summary>
        [Required]
        public string Body { get; set; }

        /// <summary>
        ///     Gets or sets from.
        /// </summary>
        [Required]
        [EmailAddress]
        public string From { get; set; }

        /// <summary>
        ///     Gets or sets from display name.
        /// </summary>
        [Required]
        public string FromDisplayName { get; set; }

        /// <summary>
        ///     Converts the DTO to a MailTemplate object.
        /// </summary>
        public MailTemplate ToMailTemplate()
        {
            var template = new MailTemplate(Id, Name)
            {
                Subject = Subject,
                Body = Body,
                From = From,
                FromDisplayName = FromDisplayName
            };

            return template;
        }
    }
}