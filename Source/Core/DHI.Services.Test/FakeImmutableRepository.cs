namespace DHI.Services.Test
{
    using System;
    using System.Collections.Generic;

    public class FakeImmutableRepository : FakeRepository<FakeImmutableEntity, Guid>, IImmutableRepository<FakeImmutableEntity>
    {
        public FakeImmutableRepository(IEnumerable<FakeImmutableEntity> entities) : base(entities)
        {
        }
    }
}