namespace DHI.Services.Notifications.WebApi
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class NotificationEntryDTO
    {
        [Required]
        public NotificationLevel NotificationLevel { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public string Source { get; set; }

        public string? Tag { get; set; }

        public string? MachineName { get; set; }

        public IDictionary<string, object>? Metadata { get; set; }
    }
}
