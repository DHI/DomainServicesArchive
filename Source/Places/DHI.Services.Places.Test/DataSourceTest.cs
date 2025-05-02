namespace DHI.Services.Places.Test
{
    using System;
    using AutoFixture.Xunit2;
    using Xunit;

    public class DataSourceTest
    {
        [Fact]
        public void CreateWithNullOrEmptyConnectionIdThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new DataSource(DataSourceType.Scalar, null, "entityId"));
            Assert.Throws<ArgumentException>(() => new DataSource(DataSourceType.Scalar, "", "entityId"));
        }

        [Fact]
        public void CreateWithNullEntityIdThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new DataSource(DataSourceType.Scalar, "connectionId", null));
        }

        [Theory]
        [AutoData]
        public void EqualityIsOk(DataSourceType dataSourceType, string connectionId, string entityId)
        {
            var dataSource1 = new DataSource(dataSourceType, connectionId, entityId);
            var dataSource2 = new DataSource(dataSource1.Type, dataSource1.ConnectionId, dataSource1.EntityId);
            var dataSource3 = new DataSource(dataSource1.Type, dataSource1.ConnectionId, "someEntityId");

            Assert.Equal(dataSource1, dataSource2);
            Assert.True(dataSource1 == dataSource2);
            Assert.NotEqual(dataSource1, dataSource3);
            Assert.True(dataSource1 != dataSource3);
        }
    }
}