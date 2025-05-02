namespace DHI.Services.Security.WebApi.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    [Serializable]
    public class LoginAttemptPolicyDTO
    {
        /// <summary>
        ///     Gets or sets the maximum login tries. Defaults to 3.
        /// </summary>
        [JsonPropertyName("maxNumberOfLoginAttempts")]
        public int MaxNumberOfLoginAttempts { get; set; } = 3;

        /// <summary>
        ///     Gets or sets the Time Span of the reset interval. Defaults to 1 Minutes.
        /// </summary>
        [JsonPropertyName("resetInterval")]
        public TimeSpan ResetInterval { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        ///     Gets or sets the Days of the Locked Account Time. Defaults to 30 Days.
        /// </summary>
        [JsonPropertyName("lockedPeriod")]
        public TimeSpan LockedPeriod { get; set; } = TimeSpan.FromDays(30);
    }
}
