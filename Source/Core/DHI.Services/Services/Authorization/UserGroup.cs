namespace DHI.Services.Authorization
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class UserGroup : BaseNamedEntity<string>
    {
        [JsonConstructor]
        public UserGroup(string id, string name, HashSet<string> users = null)
            : base(id, name)
        {
            Users = users;
        }

        //public UserGroup(string id, string name, HashSet<string> users)
        //    : base(id, name)
        //{
        //    Users = users;
        //}

        public HashSet<string> Users { get; }
    }
}