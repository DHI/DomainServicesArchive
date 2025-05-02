namespace DHI.Services.WebApiCore.Test
{
    using System;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Xunit;

    public class GetMandatoryValueTest : IClassFixture<ConfigurationFixture>
    {
        private readonly IConfiguration _configuration;

        public GetMandatoryValueTest(ConfigurationFixture fixture)
        {
            _configuration = fixture.Configuration;
        }

        [Fact]
        public void GetNonExistingMandatoryValueThrows()
        {
            const string key = "NonExistingKey";
            Action getMandatoryValue = () => _configuration.GetMandatoryValue<string>(key);
            getMandatoryValue.Should().Throw<ArgumentException>().WithMessage($"The configuration does not contain the mandatory key '{key}'.*");
        }

        [Fact]
        public void GetMandatoryValueIsOk()
        {
            _configuration["Key1"].Should().Be("Value1");
            _configuration["Nested:Key1"].Should().Be("NestedValue1");
            _configuration["Nested:Key2"].Should().Be("NestedValue2");
        }
    }
}
