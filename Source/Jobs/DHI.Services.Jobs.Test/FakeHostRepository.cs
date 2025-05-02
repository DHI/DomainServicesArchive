namespace DHI.Services.Jobs.Test
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Jobs;

    internal class FakeHostRepository : FakeRepository<Host, string>, IHostRepository
    {
        public FakeHostRepository()
        {
        }

        public FakeHostRepository(IEnumerable<Host> hostList)
            : base(hostList)
        {
        }

        public void AdjustJobCapacity(int desiredJobCapacity, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }

        public void CreateHost(ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();
        }
    }
}