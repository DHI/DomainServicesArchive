namespace DHI.Services.WebApiCore.Test
{
    using System.Collections.Generic;
    using AutoFixture;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using Xunit;

    public class GetAdditionalParametersTest
    {
        [Fact]
        public void GetAdditionalParametersIsOk()
        {
            var dictionary = new Dictionary<string, StringValues> {{"foo", "bar"}};
            var fixture = new Fixture();
            fixture.AddManyTo(dictionary);

            IQueryCollection queryCollection = new QueryCollection(dictionary);
            var parameters = queryCollection.GetAdditionalParameters(new[] {"foo"});

            parameters.Keys.Should().NotContain("foo");
            parameters.Count.Should().Be(3);
        }
    }
}