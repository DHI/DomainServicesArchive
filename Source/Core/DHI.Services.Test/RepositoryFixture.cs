namespace DHI.Services.Test
{
    using System.Linq;
    using AutoFixture;

    public class RepositoryFixture
    {
        public RepositoryFixture()
        {
            var fixture = new Fixture();
            var fakeEntityList = fixture.CreateMany<FakeEntity>().ToList();
            Repository = new FakeGroupedRepository<FakeEntity, string>(fakeEntityList);
            RepeatCount = fixture.RepeatCount;
        }

        public FakeGroupedRepository<FakeEntity, string> Repository { get; }

        public int RepeatCount { get; }
    }
}