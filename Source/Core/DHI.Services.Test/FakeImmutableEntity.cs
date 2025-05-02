namespace DHI.Services.Test
{
    using System;

    [Serializable]
    public class FakeImmutableEntity : BaseNamedEntity<Guid>
    {
        public FakeImmutableEntity(Guid id, string name) : base(id, name)
        {
        }
    }
}