namespace DHI.Services.WebApiCore.Test
{
    using FluentAssertions;
    using WebApiCore;
    using Xunit;

    public class FullNameTest
    {
        [Theory]
        [InlineData("Group|ts:||345", "Group/ts:|345")]
        [InlineData("Group|||Sub||||Group|ts:||345", "Group|/Sub||Group/ts:|345")]
        public void ParseIsOk(string urlFullName, string expectedFullName)
        {
            FullNameString.FromUrl(urlFullName).Should().Be(expectedFullName);
        }
    }
}