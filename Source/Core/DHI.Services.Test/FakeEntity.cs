namespace DHI.Services.Test
{
    using DHI.Services.Authorization;
    using System;
    using System.Collections.Generic;

    public class FakeEntity : BaseGroupedEntity<string>
    {

        public FakeEntity(string id, string name, string group = null, IDictionary<string, object> metadata = null, IList<Permission> permissions = null, bool foo = true, DateTime? bar = default)
            : base(id, name, group, metadata, permissions)
        {
            Foo = foo;
            Bar = bar;
        }

        public bool Foo { get; }

        public DateTime? Bar { get; }
    }
}