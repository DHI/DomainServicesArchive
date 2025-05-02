namespace DHI.Services.Test.Authorization
{
    using System.Collections.Generic;
    using DHI.Services.Authorization;

    internal class FakeUserGroupRepository : FakeRepository<UserGroup, string>, IUserGroupRepository
    {
        public FakeUserGroupRepository()
        {
        }

        public FakeUserGroupRepository(IEnumerable<UserGroup> userGroups)
            : base(userGroups)
        {
        }
    }
}