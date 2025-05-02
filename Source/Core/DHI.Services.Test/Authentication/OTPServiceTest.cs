namespace DHI.Services.Test.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DHI.Services.Authentication.Otp;
    using Xunit;

    public class OtpServiceTest
    {
        private class MockOtpAuthenticator : IOtpAuthenticator
        {
            private readonly bool _shouldAuth;

            public MockOtpAuthenticator(bool shouldAuth)
            {
                _shouldAuth = shouldAuth;
            }

            public (string manualEntryCode, string qrCode) GenerateSetupCode(string accountName)
            {
                return ("1234", "aqr");
            }

            public bool Validate(string otp, IEnumerable<string> twoFAConfig)
            {
                return _shouldAuth;
            }
        }

        [Fact]
        public void CtorWillExceptionIfNoAuthenticationProvider()
        {
            Assert.Throws<ArgumentNullException>(() => new OtpService((IOtpAuthenticator)null));
            Assert.Throws<ArgumentNullException>(() => new OtpService((Dictionary<string, IOtpAuthenticator>)null));
            Assert.Throws<ArgumentException>(() => new OtpService(new Dictionary<string, IOtpAuthenticator>()));
        }

        [Fact]
        public void ExceptionShouldBeThrownIfNullMetadataFuncIsPassed()
        {
            var otpService = new OtpService(new Dictionary<string, IOtpAuthenticator> {{"test", new MockOtpAuthenticator(true)}});

            Assert.Throws<ArgumentNullException>(() => otpService.GetOtpConfiguration(null, null));
        }

        [Fact]
        public void OtpProcessWillExceptionIfAuthenticatorsDoNotIntersect()
        {
            var otpService = new OtpService(new Dictionary<string, IOtpAuthenticator> {{"test", new MockOtpAuthenticator(true)}});

            Assert.Throws<InvalidOperationException>(() => otpService.GetOtpConfiguration(new[] { "CIDR:123.123.123.123", "GA:98628722876" }, (sa) => false));
        }

        [Fact]
        public void OtpProcessWillReturnAccessForbiddenIfCIDRTestFailsButOtpIsNot()
        {
            var otpService = new OtpService(new Dictionary<string, IOtpAuthenticator> {{"test", new MockOtpAuthenticator(true)}});

            Assert.True(otpService.GetOtpConfiguration(new[] { "CIDR:123.123.123.123" }, sa => false).AccessForbidden);
        }

        [Fact]
        public void OtpProcessWillExceptionIfCIDRIsEmpty()
        {
            var otpService = new OtpService(new Dictionary<string, IOtpAuthenticator> { { "test", new MockOtpAuthenticator(true) } });

            Assert.Throws<InvalidOperationException>(() => otpService.GetOtpConfiguration(new[] { "CIDR:" }, (sa) => false));
        }

        [Fact]
        public void OtpProcessWillReturnFalseIfIpCheckPasses()
        {
            var otpService = new OtpService(new Dictionary<string, IOtpAuthenticator> {{"GA", new MockOtpAuthenticator(true)}});

            const string ipAddress = "123.123.123.123";
            var whitelistfunc = new Func<string[], bool>(whitelist => {
                Assert.Single(whitelist);
                Assert.Equal(ipAddress, whitelist[0]);
                return ipAddress == whitelist[0];
            });

            var result = otpService.GetOtpConfiguration(new[] { "CIDR:123.123.123.123" }, sa => true);

            Assert.False(result.OtpRequired);
        }


        [Fact]
        public void OtpProcessWillReturnFalseIfNoMetadataIsFound()
        {
            var otpService = new OtpService(new Dictionary<string, IOtpAuthenticator> { { "GA", new MockOtpAuthenticator(true) } });
                        
            var result = otpService.GetOtpConfiguration(Array.Empty<string>(), sa => true);

            Assert.False(result.OtpRequired);
        }

        [Fact]
        public void OtpProcessWillReturnValidAuthenticatorOptions()
        {
            var otpService = new OtpService(new Dictionary<string, IOtpAuthenticator> {{"GA", new MockOtpAuthenticator(true)}});

            var result = otpService.GetOtpConfiguration(new[] { "CIDR:123.123.123.123", "GA:98628722876" }, sa => false);

            Assert.True(result.OtpRequired);
            Assert.Single(result.OtpAuthenticatorIds);
            Assert.Equal("GA", result.OtpAuthenticatorIds.First());
        }

        [Fact]
        public void ValidateWillExceptionIfIncorrectKeyIsPassed()
        {
            var otpService = new OtpService(new MockOtpAuthenticator(false));
            Assert.Throws<Exception>(() => otpService.ValidateOtp("123", Array.Empty<string>(), "invalidkey"));
        }

        [Fact]
        public void ValidateWillFindAuthenticatorIfCorrectKeyIsPassed()
        {
            var otpService = new OtpService(new Dictionary<string, IOtpAuthenticator> {{"test", new MockOtpAuthenticator(true)}});
            Assert.True(otpService.ValidateOtp("123", Array.Empty<string>(), "test"));
        }

        [Fact]
        public void ValidateWillFindAuthenticatorIfImplicitKeyIsPassed()
        {
            var otpService = new OtpService(new MockOtpAuthenticator(true));
            Assert.True(otpService.ValidateOtp("123", Array.Empty<string>(), typeof(MockOtpAuthenticator).Name));
        }

        [Fact]
        public void ValidateWillFindAuthenticatorIfNoKeyIsPassed()
        {
            var otpService = new OtpService(new MockOtpAuthenticator(true));
            Assert.True(otpService.ValidateOtp("123", Array.Empty<string>(), typeof(MockOtpAuthenticator).Name));
        }

        [Theory]
        [InlineData(null, "", "", typeof(ArgumentNullException))]
        [InlineData("valid", null, "", typeof(ArgumentNullException))]
        [InlineData("valid", "x", null, typeof(ArgumentNullException))]
        [InlineData("invalid-key", "x", "x", typeof(ArgumentException))]        
        public void GenerateOtpCodeThrowsException(string metaDataStr, string authenticatorIdentifier, string accountName, Type exceptionType)
        {
            string[] metaData = null;
            if (metaDataStr == "empty")
            {
                metaData = Array.Empty<string>();
            }
            else if (metaDataStr == "valid")
            {
                metaData = new[]
                {
                    "test:jjhgfjhgf"
                };
            }
            else if (metaDataStr == "invalid-key")
            {
                metaData = new[]
                {
                    "tejst:jjhgfjhgf"
                };
            }
            var otpService = new OtpService(new MockOtpAuthenticator(true));
            Assert.Throws(exceptionType, () => otpService.GenerateOtpAuthenticatorSetupCode(accountName, metaData, authenticatorIdentifier));
        }

        [Fact]
        public void GenerateOtpCodeThrowsExceptionIfAuthenticatorsDontIntersect()
        {
            var metaData = new[]
            {
                "NOTauth:jjhgfjhgf"
            };
            var otpService = new OtpService(new Dictionary<string, IOtpAuthenticator> {{"auth", new MockOtpAuthenticator(true)}});
            Assert.Throws<ArgumentException>(() => otpService.GenerateOtpAuthenticatorSetupCode("anything", metaData, "auth"));
        }

        [Fact]
        public void GenerateOtpCodeReturnsAuthenticatorValue()
        {
            var metaData = new[]
            {
                "auth:jjhgfjhgf"
            };
            var otpService = new OtpService(new Dictionary<string, IOtpAuthenticator> { { "auth", new MockOtpAuthenticator(true) } });
            var (manualEntryCode, qrCode) = otpService.GenerateOtpAuthenticatorSetupCode("anything", metaData, "auth");

            Assert.Equal("1234", manualEntryCode);
            Assert.Equal("aqr", qrCode);
        }
    }
}