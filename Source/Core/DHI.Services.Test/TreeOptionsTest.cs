namespace DHI.Services.Test
{
    using System;
    using Xunit;

    public class TreeOptionsTest
    {
        [Theory]
        [InlineData("nonrecursive, groupsonly", TreeOptions.NonRecursive | TreeOptions.GroupsOnly)]
        [InlineData("Nonrecursive, Groupsonly", TreeOptions.NonRecursive | TreeOptions.GroupsOnly)]
        [InlineData("Nonrecursive,Groupsonly", TreeOptions.NonRecursive | TreeOptions.GroupsOnly)]
        [InlineData("NonRecursive", TreeOptions.NonRecursive)]
        [InlineData("Groupsonly", TreeOptions.GroupsOnly)]
        public void TryParseIsOk(string s, TreeOptions expected)
        {
            Assert.True(Enum.TryParse<TreeOptions>(s, true, out var options));
            Assert.Equal(expected, options);
        }

        [Theory]
        [InlineData("myGroup;nonrecursive", TreeOptions.NonRecursive)]
        [InlineData("myGroup;nonrecursive,groupsonly", TreeOptions.NonRecursive| TreeOptions.GroupsOnly)]
        [InlineData("myGroup;NonRecursive,GroupsOnly", TreeOptions.NonRecursive | TreeOptions.GroupsOnly)]
        [InlineData("myGroup;groupsonly, nonrecursive", TreeOptions.NonRecursive | TreeOptions.GroupsOnly)]
        public void TryGetTreeOptionsIsOk(string s, TreeOptions expected)
        {
            Assert.True(s.TryGetTreeOptions(out var options, out var trimmedString));
            Assert.Equal(expected, options);
            Assert.Equal("myGroup", trimmedString);
        }

        [Theory]
        [InlineData("myGroup;non-recursive")]
        [InlineData("myGroup;nonrecursive|groupsonly")]
        [InlineData("myGroup")]
        [InlineData("myGroup;groupsonly, non-recursive")]
        public void TryGetTreeOptionsFailsAsExpected(string s)
        {
            Assert.False(s.TryGetTreeOptions(out var options, out var trimmedString));
            Assert.Equal(default, options);
            Assert.Equal(s, trimmedString);
        }
    }
}