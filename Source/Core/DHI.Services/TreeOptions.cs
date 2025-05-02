namespace DHI.Services
{
    using System;

    /// <summary>
    ///     Options for configuring a hierarchical tree structure
    /// </summary>
    [Flags]
    public enum TreeOptions
    {
        NonRecursive = 1,
        GroupsOnly = 2
    }
}