namespace DHI.Services.Test
{
    using DHI.Services.Authorization;
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class FakeGroupedEntity : BaseGroupedEntity<string>

    {
        public FakeGroupedEntity(string name, string group, IDictionary<string, object> metadata = null, IList<Permission> permissions = null)
            : base(Guid.NewGuid().ToString(), name, group, metadata, permissions)
        {
        }

        [JsonConstructor]
        public FakeGroupedEntity(string id, string name, string group, IDictionary<string, object> metadata = null, IList<Permission> permissions = null)
            : base(id, name, group, metadata, permissions)
        {
        }
    }
}