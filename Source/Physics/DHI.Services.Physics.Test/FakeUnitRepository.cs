namespace DHI.Services.Physics.Test
{
    using System.Collections.Generic;

    internal class FakeUnitRepository : FakeRepository<Unit, string>, IUnitRepository
    {
        public FakeUnitRepository()
        {
        }

        public FakeUnitRepository(IEnumerable<Unit> unitList)
            : base(unitList)
        {
        }
    }
}