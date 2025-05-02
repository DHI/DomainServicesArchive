namespace DHI.Services.Jobs.Workflows
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class HostGroupAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostGroupAttribute" /> class.
        /// </summary>
        /// <param name="hostGroup">The host group used for workflow execution.</param>
        public HostGroupAttribute(string hostGroup)
        {
            Guard.Against.NullOrEmpty(hostGroup, nameof(hostGroup));
            HostGroup = hostGroup;
        }

        public string HostGroup { get; }
    }
}