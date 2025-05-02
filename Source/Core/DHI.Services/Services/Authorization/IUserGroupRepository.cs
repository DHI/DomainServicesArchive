namespace DHI.Services.Authorization
{
    public interface IUserGroupRepository : IRepository<UserGroup, string>, IDiscreteRepository<UserGroup, string>, IUpdatableRepository<UserGroup, string>
    {
    }
}