namespace DHI.Services.Jobs.Workflows
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class TimeoutAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TimeoutAttribute"/> class.
        /// </summary>
        /// <param name="timeSpan">The timeout time span.</param>
        public TimeoutAttribute(string timeSpan)
        {
            Guard.Against.NullOrEmpty(timeSpan, nameof(timeSpan));
            Timespan = TimeSpan.Parse(timeSpan);
        }

        public TimeSpan Timespan { get; }
    }
}