namespace DHI.Services.Jobs.Workflows
{
    using System;

    [Obsolete("This type will eventually be removed.")]
    public interface IActivityRepository : IRepository<Activity, string>, IDiscreteRepository<Activity, string>
    {
    }
}