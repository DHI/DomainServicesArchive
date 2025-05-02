namespace DHI.Services.WebApiCore.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using Accounts;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using Xunit;

    public class ToQueryTest
    {
        [Fact]
        public void ToQueryIsOk()
        {
            var dictionary = new Dictionary<string, StringValues> {{"activated", "true"}};
            IQueryCollection queryCollection = new QueryCollection(dictionary);
            var query = queryCollection.ToQuery<Account>();

            query.Count().Should().Be(1);
            query.Single().Item.Should().Be("activated");
            query.Single().Value.Should().Be(true);
        }
    }
}