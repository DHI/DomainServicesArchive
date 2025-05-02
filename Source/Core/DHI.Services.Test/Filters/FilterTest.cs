namespace DHI.Services.Test.Filters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using Accounts;
    using DHI.Services.Filters;
    using Xunit;

    public class FilterTest
    {
        [Fact]
        public void CreateWithNullOrEmptyDataTypeThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new Filter(null, new List<QueryCondition>()));
            Assert.Throws<ArgumentException>(() => new Filter("", new List<QueryCondition>()));
        }

        [Fact]
        public void CreateIsOk()
        {
            var query = new Query<Account>
            {
                new QueryCondition("Name", QueryOperator.Like, "John"),
                new QueryCondition("Activated", false),
                new QueryCondition("TokenExpiration", QueryOperator.GreaterThan, new DateTime(2020, 11, 9))
            };

            var filter = new Filter(typeof(Account).FullName, query);
            Assert.Equal(3, filter.QueryConditions.Count());
            Assert.Equal(typeof(Account).FullName, filter.DataType);
        }

        [Fact]
        public void IdSerializationIsOk()
        {
            var query = new Query<Account>
            {
                new QueryCondition("Name", QueryOperator.Like, "John"),
                new QueryCondition("Activated", false),
                new QueryCondition("TokenExpiration", QueryOperator.GreaterThan, new DateTime(2020, 11, 9))
            };

            var filter = new Filter(typeof(Account).FullName, query);

            var data = Convert.FromBase64String(filter.Id);
            var s = Encoding.UTF8.GetString(data);
            var f = JsonSerializer.Deserialize<Filter>(s, Filter.DeserializerOption);

            Assert.Equal(3, f.QueryConditions.Count());
            Assert.Equal(typeof(Account).FullName, f.DataType);
            Assert.Null(f.DataConnectionId);
        }
    }
}