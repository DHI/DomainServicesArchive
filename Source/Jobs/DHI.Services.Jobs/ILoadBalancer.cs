namespace DHI.Services.Jobs
{
    using System;

    public interface ILoadBalancer
    {
        Maybe<Host> GetHost(Guid jobId, string hostGroup = null);
    }
}