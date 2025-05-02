namespace DHI.Services.Security.WebApi.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Accounts;
    using Authorization;
    using Xunit;

    public class Get2FAMetadataTest
    {
        private class MockUserGroupRepository : IUserGroupRepository
        {
            private readonly UserGroup _userGroup;
            private readonly UserGroup _otherUserGroup;
            public MockUserGroupRepository()
            {
            }

            public MockUserGroupRepository(UserGroup userGroup)
            {
                _userGroup = userGroup;
            }
            public MockUserGroupRepository(UserGroup userGroup, UserGroup otherUserGroup)
            {
                _userGroup = userGroup;
                _otherUserGroup = otherUserGroup;
            }
            public void Add(UserGroup entity, ClaimsPrincipal user = null)
            {
                throw new NotImplementedException();
            }

            public bool Contains(string id, ClaimsPrincipal user = null)
            {
                throw new NotImplementedException();
            }

            public int Count(ClaimsPrincipal user = null)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<UserGroup> GetAll(ClaimsPrincipal user = null)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<string> GetIds(ClaimsPrincipal user = null)
            {
                var x = new[] { _userGroup, _otherUserGroup };

                return x.Where(ug => ug != null).Select(ug => ug.Id);
            }

            public void Remove(string id, ClaimsPrincipal user = null)
            {
                throw new NotImplementedException();
            }

            public void Update(UserGroup entity, ClaimsPrincipal user = null)
            {
                throw new NotImplementedException();
            }

            Maybe<UserGroup> IRepository<UserGroup, string>.Get(string id, ClaimsPrincipal user)
            {
                var x = new[] { _userGroup, _otherUserGroup };

                return x.FirstOrDefault(ug => ug != null && ug.Id == id)?.ToMaybe() ?? Maybe.Empty<UserGroup>();

            }
        }

        private const string TwoFAMetadataKey = "Otp";

        [Fact]
        public void ReturnsDefaultIfAccountHasNoUserGroupMemberships()
        {
            var account = new Account("john.doe", "John Doe");
            var defaultOtpMetadata = new[] { "CIDR:123.123.123.123/32" };
            var metadata = new UserGroupService(new MockUserGroupRepository()).GetTwoFAMetadata(account, TwoFAMetadataKey, defaultOtpMetadata);

            Assert.Single(metadata);
            Assert.IsType<string[]>(metadata);
            Assert.Equal(defaultOtpMetadata, metadata);
        }

        [Fact]
        public void ReturnsDefaultIfUserGroupHasNoOtpMetadata()
        {
            var account = new Account("john.doe", "John Doe");
            var userGroup = new UserGroup("Acme", "Acme Inc.", new HashSet<string>());
            userGroup.Users.Add(account.Id);
            var defaultOtpMetadata = new[] { "CIDR:123.123.123.123/32" };
            var metadata = new UserGroupService(new MockUserGroupRepository(userGroup)).GetTwoFAMetadata(account, TwoFAMetadataKey, defaultOtpMetadata);

            Assert.Single(metadata);
            Assert.IsType<string[]>(metadata);
            Assert.Equal(defaultOtpMetadata, metadata);
        }

        [Fact]
        public void ReturnsEmptyIfUserGroupHasNoMetadata()
        {
            var account = new Account("john.doe", "John Doe");
            var userGroup = new UserGroup("Acme", "Acme Inc.", new HashSet<string>());
            userGroup.Users.Add(account.Id);
            var metadata = new UserGroupService(new MockUserGroupRepository(userGroup)).GetTwoFAMetadata(account, TwoFAMetadataKey);

            Assert.Empty(metadata);
        }

        [Fact]
        public void ReturnsEmptyIfUserGroupHasNoOtpMetadata()
        {
            var account = new Account("john.doe", "John Doe");
            var userGroup = new UserGroup("Acme", "Acme Inc.", new HashSet<string>());
            userGroup.Users.Add(account.Id);
            userGroup.Metadata.Add("SomeOtherConfig", "test");
            var metadata = new UserGroupService(new MockUserGroupRepository(userGroup)).GetTwoFAMetadata(account, TwoFAMetadataKey);

            Assert.Empty(metadata);
        }

        [Fact]
        public void ReturnsOtpMetadataOnValidAccount()
        {
            var account = new Account("john.doe", "John Doe");
            var userGroup = new UserGroup("Acme", "Acme Inc.", new HashSet<string>());
            userGroup.Users.Add(account.Id);
            var otpMetadata = new[] { "CIDR:123.123.123.123/32" };
            userGroup.Metadata.Add(TwoFAMetadataKey, otpMetadata);
            var metadata = new UserGroupService(new MockUserGroupRepository(userGroup)).GetTwoFAMetadata(account, TwoFAMetadataKey);

            Assert.Single(metadata);
            Assert.IsType<string[]>(metadata);
            Assert.Equal(otpMetadata, metadata);
        }

        [Fact]
        public void ReturnsOtpMetadataForAllValidUserGroups()
        {
            var account = new Account("john.doe", "John Doe");
            var userGroup = new UserGroup("Acme", "Acme Inc.", new HashSet<string>());
            userGroup.Users.Add(account.Id);
            var otpMetadata = new[] { "CIDR:123.123.123.123/32" };
            userGroup.Metadata.Add(TwoFAMetadataKey, otpMetadata);
            var otherUserGroup = new UserGroup("Acme2", "Acme2 Inc.", new HashSet<string>());
            otherUserGroup.Users.Add(account.Id);
            var otpMetadata2 = new[] { "CIDR:223.223.223.223/32" };
            otherUserGroup.Metadata.Add(TwoFAMetadataKey, otpMetadata2);

            var metadata = new UserGroupService(new MockUserGroupRepository(userGroup, otherUserGroup)).GetTwoFAMetadata(account, TwoFAMetadataKey);

            Assert.Collection(metadata, (e) => Assert.Equal("CIDR:123.123.123.123/32", e), f => Assert.Equal("CIDR:223.223.223.223/32", f));
        }


        [Theory]
        [InlineData(new string[0], "123.123.123.123", true)]
        [InlineData(new[] { "123.123.123.123/32" }, "123.123.123.123", true)]
        [InlineData(new[] { "222.222.222.222/32" }, "123.123.123.123", false)]
        [InlineData(new[] { "123.123.123.123/32&Comment:AComment" }, "123.123.123.123", true)]
        [InlineData(new[] { "222.222.222.222/32&Comment:AComment" }, "123.123.123.123", false)]
        public void IpWhitelistWillCorrectlyMatchClientIp(string[] cidrBlocks, string clientIp, bool shouldPass)
        {
            Assert.Equal(shouldPass, IpWhitelist.Validate(cidrBlocks, clientIp));
        }

        [Theory]
        [InlineData(new[] { "123.123.123.123/32" }, "999.123.123.123")]
        [InlineData(new[] { "123.123.123.999/32" }, "123.123.123.123")]
        [InlineData(new[] { "123.123.123.999/Foo" }, "123.123.123.123")]
        public void IpWhitelistWillCatchBadlt(string[] cidrBlocks, string clientIp)
        {
            Assert.Throws<FormatException>(() => IpWhitelist.Validate(cidrBlocks, clientIp));
        }
    }
}